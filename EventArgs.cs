using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class ConnectionEventArgs : EventArgs
    {
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
