using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDo
{
    /// <summary>
    /// Class responsible for identifying methods hooked to a POST request.
    /// </summary>
    public class HttpPost : HttpRoute
    {
        /// <summary>
        /// Create and initialize a new instance of <see cref="HttpPost"/> class.
        /// </summary>
        /// <param name="route">Route under which request will be available.</param>
        /// <param name="secure">Defines whether request will have to go through HasAccess() method.</param>
        public HttpPost(string route, bool secure = false) : base(route, secure) => HttpMethod = "POST";
    }
}
