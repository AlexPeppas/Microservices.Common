using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservices.Common.Types
{
    public class HttpClientParameters
    {
        public HttpClientParameters()
        {
            headers = new Dictionary<string, string>(); //initialize dictionary
            timeoutPolicy = 3; //default time
        }

        public string baseUrl { get; set; }

        public Dictionary<string, string> headers { get; set; }

        /// <summary>
        /// client's timeout policy in seconds
        /// </summary>
        public int timeoutPolicy { get; set; }
    }
}
