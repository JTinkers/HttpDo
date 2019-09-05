using HttpDo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ExampleApp
{
    public class HttpService : HttpHandler
    {
        public HttpService(string rootUrl, string rootPath) : base(rootUrl, rootPath) { }

        protected override bool HasAccess() => GetSession()["login"];

        protected override bool HasFileAccess(string path) => Path.GetExtension(path) == ".html" 
            || Path.GetDirectoryName(path) == "downloadables";
    }
}
