using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace HttpDo
{
    /// <summary>
    /// Class providing access to data shared between pages and requests.
    /// </summary>
    public class Session : Dictionary<string, dynamic>
    {
        /// <summary>
        /// IP address associated with the token. TO-DO: Make it more unique, IP addresses are quite.. common.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Create and initialize an instance of <see cref="Session"/> class.
        /// </summary>
        /// <param name="address">IP address associated with the session</param>
        public Session(string address) => Address = address;

        /// <summary>
        /// Retrieve value under given key.
        /// </summary>
        /// <param name="key">Key as an index.</param>
        /// <returns>Value under specific key.</returns>
        public dynamic this[string key]
        {
            get
            {
                this.TryGetValue(key, out dynamic r);

                return r;
            }
            set => (this as Dictionary<string, dynamic>)[key] = value;
        }
    }
}
