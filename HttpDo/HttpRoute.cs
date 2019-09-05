using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HttpDo
{
    /// <summary>
    /// Class responsible for marking methods for access through HTTP requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class HttpRoute : Attribute
    {
        /// <summary>
        /// Type of the HTTP request.
        /// </summary>
        public string HttpMethod { get; protected set; }

        /// <summary>
        /// Route under which method is accessible.
        /// </summary>
        public string Route { get; protected set; }

        /// <summary>
        /// Defines whether or not route will have to go through HasAccess().
        /// </summary>
        public bool Secure { get; protected set; }

        /// <summary>
        /// Create and initialize a new instance of <see cref="HttpRoute"/> class.
        /// </summary>
        /// <param name="route">Route under which request will be available.</param>
        /// <param name="secure">Defines whether request will have to go through HasAccess() method.</param>
        public HttpRoute(string route, bool secure = false)
        {
            Route = route;
            Secure = secure;

            if (Route.StartsWith("/"))
                Route = Route.TrimStart('/');
        }

        /// <summary>
        /// Retrieve method hooked to this instance of HttpRoute.
        /// </summary>
        /// <returns>The hooked attribute.</returns>
        public MethodInfo GetAssociatedMethod()
        {
            return Assembly.GetEntryAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods())
                .Select(x => new { Method = x, Attribute = x.GetCustomAttribute<HttpRoute>() })
                .Where(x => x.Attribute != null && x.Attribute.Equals(this))
                .SingleOrDefault()?.Method;
        }
    }
}
