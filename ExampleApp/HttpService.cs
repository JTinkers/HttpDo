using HttpDo;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ExampleApp
{
    public class HttpService : HttpHandler
    {
        public HttpService(string rootUrl, string rootPath) : base(rootUrl, rootPath) { }
    }
}
