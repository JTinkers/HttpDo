using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HttpDo
{
    public class HttpHandler
    {
        private HttpListener Listener { get; set; } = new HttpListener();

        private List<HttpRoute> Routes { get; set; } = new List<HttpRoute>();

        private List<Session> Sessions { get; set; } = new List<Session>();

        /// <summary>
        /// Points to the last request, used to reduce amount of parameters.
        /// </summary>
        protected HttpListenerRequest Request { get; set; }

        /// <summary>
        /// Points to the last response, used to reduce amount of parameters.
        /// </summary>
        protected HttpListenerResponse Response { get; set; }

        /// <summary>
        /// Points to the last route, used to reduce amount of parameters.
        /// </summary>
        protected HttpRoute Route { get; set; }

        public string RootUrl { get; private set; }

        public string RootPath { get; private set; }

        public HttpHandler(string rootUrl, string rootPath)
        {
            RootUrl = rootUrl;
            RootPath = Path.Combine(AppContext.BaseDirectory, rootPath);

            if (!RootUrl.EndsWith("/"))
                RootUrl += "/";

            if (!Directory.Exists(RootPath))
                throw new Exception("Path for HttpHandler is invalid.");

            if (!HttpListener.IsSupported)
                throw new Exception("HttpServer requires functionality that isn't supported by your OS.");

            // add all routes
            Routes.AddRange(Assembly.GetEntryAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Select(x => x.GetCustomAttribute<HttpRoute>())
                .Where(x => x != null));

            // check if there aren't duplicates
            var duplicate = Routes.GroupBy(x => new { Route = x.Route, Method = x.HttpMethod })
                .Where(x => x.Count() > 1)
                .FirstOrDefault()?.Key;

            if (duplicate != null)
                throw new Exception($"There's a duplicate route: {duplicate.Route}({duplicate.Method})");

            Listener.Prefixes.Add(RootUrl);

            Listener.Start();

            Task.Run(() =>
            {
                while (Listener.IsListening)
                {
                    var context = Listener.GetContext();

                    Request = context.Request;

                    Response = context.Response;

                    if (GetSession() == null)
                        Sessions.Add(new Session(Request.RemoteEndPoint.Address.ToString()));

                    var path = Request.Url.AbsolutePath;

                    if (path.StartsWith("/"))
                        path = path.TrimStart('/');

                    Route = GetRoute(path);

                    if (path.Equals(""))
                        path = "index.html";

                    path = Path.Combine(RootPath, path);

                    switch (Request.HttpMethod)
                    {
                        case "GET":
                            HandleGet(path);
                            continue;
                        case "POST":
                            HandlePost(path);
                            continue;
                    }
                }

                Listener.Close();
            });
        }

        public Session GetSession()
            => Sessions.SingleOrDefault(x => x.Address == Request.RemoteEndPoint.Address.ToString());

        private void Abort(int errorCode, string errorMessage)
        {
            var session = GetSession();
            session["ErrorMessage"] = $"{errorCode}: {errorMessage}";

            var output = Encoding.Default.GetBytes(TemplatingEngine.Parse(session, Pages.Error));

            Response.StatusCode = errorCode;
            Response.StatusDescription = errorMessage;
            Response.ContentEncoding = Encoding.Default;
            Response.ContentLength64 = output.LongLength;
            Response.OutputStream.WriteAsync(output, 0, output.Length);
            Response.OutputStream.Flush();
            Response.Close();
        }

        private void HandleGet(string path)
        {
            var session = GetSession();

            // GET request for a method
            if (Route != null)
            {
                if (!HasAccess())
                {
                    Abort(401, "Unauthorized access");

                    return;
                }

                var method = Route.GetAssociatedMethod();

                var parameters = new Dictionary<string, dynamic>();

                foreach (var p in method.GetParameters())
                {
                    if (p.IsOptional && !Request.QueryString.AllKeys.Contains(p.Name))
                    {
                        parameters[p.Name] = p.DefaultValue;

                        continue;
                    }

                    if (!p.IsOptional && Nullable.GetUnderlyingType(p.ParameterType) == null && !Request.QueryString.AllKeys.Contains(p.Name))
                    {
                        Abort(400, $"Parameter `{p.Name}` is missing value for a method call.");

                        return;
                    }

                    var pType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;

                    parameters[p.Name] = Request.QueryString[p.Name] != null ? Convert.ChangeType(Request.QueryString[p.Name], pType) : null;
                }

                var returnValue = method.Invoke(null, parameters.Values.Cast<object>().ToArray());

                if (returnValue is Response)
                {
                    var resp = returnValue as Response;

                    Response.StatusCode = 200;
                    Response.Redirect(RootUrl + resp.Redirect);
                    Response.Close();

                    return;
                }

                if (returnValue != null)
                {
                    var output = Encoding.Default.GetBytes(returnValue.ToString());

                    Response.ContentEncoding = Encoding.Default;
                    Response.ContentLength64 = output.LongLength;
                    Response.OutputStream.WriteAsync(output, 0, output.Length);
                    Response.OutputStream.Flush();
                }

                Response.StatusCode = 200;
                Response.Close();

                return;
            }

            // GET request for a file
            if (!Path.HasExtension(path))
                path += ".html";

            if (!File.Exists(path))
            {
                Abort(404, $"GET request leads to a file that doesn't exist. Verify your path.");

                return;
            }

            if (!HasFileAccess(path))
            {
                Abort(401, $"Unauthorized file access.");

                return;
            }

            var text = File.ReadAllText(path);

            var data = Encoding.Default.GetBytes(path.EndsWith(".html") ? TemplatingEngine.Parse(session, text) : text);

            Response.StatusCode = 200;
            Response.ContentEncoding = Encoding.Default;
            Response.ContentLength64 = data.LongLength;
            Response.OutputStream.WriteAsync(data, 0, data.Length);
            Response.OutputStream.Flush();
            Response.Close();

            return;
        }

        private void HandlePost(string path)
        {
            if (Path.HasExtension(path))
            {
                Abort(405, $"POST requests cannot lead to files.");

                return;
            }

            if (Route == null)
            {
                Abort(404, $"POST route not found.");

                return;
            }

            if (!HasAccess())
            {
                Abort(401, $"Unauthorized access.");

                return;
            }

            var method = Route.GetAssociatedMethod();

            var body = new StreamReader(Request.InputStream, Request.ContentEncoding).ReadToEnd();

            var bodyValues = new Dictionary<string, string>();

            foreach (var val in body.Split('&'))
            {
                bodyValues[val.Split('=')[0]] = HttpUtility.UrlDecode(val.Split('=')[1]) ?? null;

                if (string.IsNullOrEmpty(bodyValues.Last().Value))
                    bodyValues[bodyValues.Last().Key] = null;
            }

            var parameters = new Dictionary<string, dynamic>();

            foreach (var p in method.GetParameters())
            {
                if (p.ParameterType == typeof(FormData))
                {
                    parameters[p.Name] = new FormData();

                    foreach (var val in bodyValues)
                        parameters[p.Name].Add(val.Key, val.Value);

                    continue;
                }

                if (p.IsOptional && !bodyValues.Keys.Contains(p.Name))
                {
                    parameters[p.Name] = p.DefaultValue;

                    continue;
                }

                if (!p.IsOptional && Nullable.GetUnderlyingType(p.ParameterType) == null && !bodyValues.Keys.Contains(p.Name))
                {
                    Abort(400, $"Parameter `{p.Name}` is missing value for a method call.");

                    return;
                }

                var pType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;

                parameters[p.Name] = bodyValues[p.Name] != null ? Convert.ChangeType(bodyValues[p.Name], pType) : null;
            }

            var returnValue = method.Invoke(null, parameters.Values.Cast<object>().ToArray());

            if (returnValue is Response)
            {
                var resp = returnValue as Response;

                Response.StatusCode = 200;
                Response.Redirect(RootUrl + resp.Redirect);
                Response.Close();

                return;
            }

            var output = Encoding.Default.GetBytes(returnValue.ToString());

            if (output != null)
            {
                Response.ContentEncoding = Encoding.Default;
                Response.ContentLength64 = output.LongLength;
                Response.OutputStream.WriteAsync(output, 0, output.Length);
                Response.OutputStream.Flush();
            }

            Response.StatusCode = 200;
            Response.Close();

            return;
        }

        private HttpRoute GetRoute(string route) 
            => Routes.Where(x => x.Route.Equals(route)).SingleOrDefault(x => x.HttpMethod.Equals(Request.HttpMethod));

        protected virtual bool HasAccess() => !Route.Secure || Convert.ToBoolean(GetSession()["is_authorized"]);

        protected virtual bool HasFileAccess(string path) => path.EndsWith(".html") || Convert.ToBoolean(GetSession()["is_authorized"]);
    }
}
