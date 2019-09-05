using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDo
{
    /// <summary>
    /// Container providing access to shared data.
    /// </summary>
    public class TemplatingGlobals
    {
        /// <summary>
        /// HTTP Session
        /// </summary>
        public Session Session { get; set; }
    }
}
