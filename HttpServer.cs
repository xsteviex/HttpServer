using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServer
{
    public class Client
    {
        public byte[] Buffer { get; set; }
        public Socket Socket { get; set; }
    }
    public class HttpServer
    {         
        //Private Members
        private readonly IHttpServerConfiguration _configuration;
        private readonly Socket _socket;

        //Constructor
        public HttpServer()
            : this(new DefaultHttpServerConfiguration { Port = 8080, Handler = new DefaultHtmlRequestHandler("/CustomServer", new[] { "index.html", "default.html" }) })
        {
        }
        public HttpServer(IHttpServerConfiguration configuration)
        {
            _configuration = configuration;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //Methods
        public void Start()
        {
            try
            {
                _socket.Bind(new IPEndPoint(IPAddress.Any, _configuration.Port));
                _socket.Listen(0);
                _socket.BeginAccept(AcceptCallback, null);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
#if DEBUG
                throw;
#endif
            }
        }
        public void Stop()
        {            
            _socket.Close();
        }
        private void SendResponse(HttpRequest request, Client client)
        {
            var response = _configuration.Handler.Respond(request);
            //Respond with encoded data from handler
            //var message = Encoding.Default.GetBytes(response.ToString());
            //var messageAndBody = message.Concat(response.Body).ToArray();
            client.Socket.Send(response, 0, response.Length, SocketFlags.None);
            OnMessageSent(new MessageEventArgs()
            {
                Message = Encoding.Default.GetString(response, 0, response.Length),
                From = client.Socket.LocalEndPoint.ToString(),
                To = client.Socket.RemoteEndPoint.ToString(),
                Type = "response"
            });
        }

        //Callbacks
        private void AcceptCallback(IAsyncResult result)
        {
            try
            {
                var clientSocket = _socket.EndAccept(result);
                var client = new Client { Buffer = new byte[clientSocket.ReceiveBufferSize], Socket = clientSocket };
                OnConnectionEstablished(client);
                client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, ReceiveCallback, client);                
                _socket.BeginAccept(AcceptCallback, null);
            }
            catch (ObjectDisposedException){}
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
#if DEBUG
                throw;
#endif
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            var client = (Client)result.AsyncState;
            try
            {
                //var client = _clients.Find(s => s.Socket == result.AsyncState);                
                var received = client.Socket.EndReceive(result);
                if (received == 0) return;
                //Turn message into a request
                var req = Encoding.Default.GetString(client.Buffer, 0, received);
                OnMessageReceived(new MessageEventArgs()
                {
                    Message = req,
                    From = client.Socket.RemoteEndPoint.ToString(),
                    To = client.Socket.LocalEndPoint.ToString(),
                    Type = "request"
                });
                SendResponse(HttpRequest.Parse(req), client);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
#if DEBUG
                throw;
#endif
            }
            finally
            {
                OnConnectionLost(client);
                client.Socket.Shutdown(SocketShutdown.Both);
                client.Socket.Close();
            }
        }        

        //Delegates
        public delegate void ConnectionHandler(object sender, ConnectionEventArgs args);
        public delegate void MessageHandler(object sender, MessageEventArgs args);

        //Events
        public event ConnectionHandler ConnectionEstablished;
        public event ConnectionHandler ConnectionLost;
        public event MessageHandler MessageReceived;
        public event MessageHandler MessageSent;

        //Triggers
        private void OnConnectionEstablished(Client client)
        {
            ConnectionEstablished?.Invoke(this, new ConnectionEventArgs { LocalEndPoint = client.Socket.LocalEndPoint, RemoteEndPoint = client.Socket.RemoteEndPoint });
        }

        private void OnConnectionLost(Client client)
        {
            ConnectionLost?.Invoke(this, new ConnectionEventArgs { LocalEndPoint = client.Socket.LocalEndPoint, RemoteEndPoint = client.Socket.RemoteEndPoint });
        }

        private void OnMessageReceived(MessageEventArgs args)
        {
            MessageReceived?.Invoke(this, args);
        }

        private void OnMessageSent(MessageEventArgs args)
        {
            MessageSent?.Invoke(this, args);
        }
    }
}
