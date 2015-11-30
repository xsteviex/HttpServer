using System;
using System.Threading;

namespace HttpServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new DefaultHttpServerConfiguration() { Port = 8080, Handler = new DefaultHtmlRequestHandler("U:/CustomServer", new[] { "index.html", "default.html" }) };
            var server = new HttpServer(config);
            server.ConnectionEstablished += Server_ConnectionEstablished;
            server.ConnectionLost += Server_ConnectionLost;
            server.MessageSent += server_MessageSent;
            server.MessageReceived += server_MessageReceived;
            server.Start();
            Thread.Sleep(new TimeSpan(0,5,0));
            server.Stop();
        }

        static void server_MessageReceived(object sender, MessageEventArgs args)
        {
            Console.WriteLine("Received {2}: \n{0}\nFrom {1}\n", args.Message, args.From, args.Type);
        }
        static void server_MessageSent(object sender, MessageEventArgs args)
        {
            Console.WriteLine("Sent {2}: \n{0}\nto {1}\n", args.Message, args.To, args.Type);
        }
        private static void Server_ConnectionLost(object sender, ConnectionEventArgs args)
        {
            Console.WriteLine("Connection Lost : {1} to {0}\n", args.LocalEndPoint, args.RemoteEndPoint);
        }
        private static void Server_ConnectionEstablished(object sender, ConnectionEventArgs args)
        {
            Console.WriteLine("Connection Established : {1} to {0}\n", args.LocalEndPoint, args.RemoteEndPoint);
        }
    }
}
