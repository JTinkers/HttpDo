using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDo
{
    /// <summary>
    /// Class responsible for identifying methods hooked to a GET request.
    /// </summary>
    public class HttpGet : HttpRoute
    {
        /// <summary>
        /// Create and initialize a new instance of <see cref="HttpGet"/> class.
        /// </summary>
        /// <param name="route">Route under which request will be available.</param>
        /// <param name="secure">Defines whether request will have to go through HasAccess() method.</param>
        public HttpGet(string route, bool secure = false) : base(route, secure) => HttpMethod = "GET";
    }
}
