using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public interface IHttpServerConfiguration
    {
        int Port { get; set; }        
        IHttpRequestHandler Handler { get; set; }
    }

    public class DefaultHttpServerConfiguration : IHttpServerConfiguration
    {
        public int Port { get; set; }
        public IHttpRequestHandler Handler { get; set; }
    }
}
