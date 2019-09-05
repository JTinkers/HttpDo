using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDo
{
    /// <summary>
    /// Class responsible for providing specific functionality after 
    /// methods hooked to routes have finished executing.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Route to which to redirect to.
        /// </summary>
        public string Redirect { get; set; }

        /// <summary>
        /// Create and initialize a new instance of <see cref="Response"/> class.
        /// </summary>
        /// <param name="redirect">Route to which to redirect to when method execution finishes.</param>
        public Response(string redirect) => Redirect = redirect;
    }
}
