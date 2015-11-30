using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class HttpStatusCode
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }

    public interface IHttpResponse
    {
        byte[] Body { get; set; }
    }

    public class HttpResponse : IHttpResponse
    {
        public HttpStatusCode Status { get; set; }        
        public byte[] Body { get; set; }
        public IList<HttpHeader> Headers { get; set; }

        public HttpResponse()
        {
        }
        public static IHttpResponse GetResponse(HttpRequest req)
        {
            if (req == null)
            {
                return MakeNullResponse();
            }
            return new HttpResponse { Status = new HttpStatusCode() { Code = 200, Message = "OK", Description = "" }, Body = Encoding.Default.GetBytes("<h1>Hello World Test</h1>"), Headers = new List<HttpHeader> { new HttpHeader { Key = "Content-type", Value = "text/html" }, new HttpHeader { Key = "Date", Value = DateTime.Now.ToString() } } };
        }

        public static HttpResponse MakeNullResponse()
        {
            return new HttpResponse { Status = new HttpStatusCode { Code = 400, Message = "Bad Request", Description = "The Request was malformed" } };
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("HTTP/1.1 " + Status?.Code + " " + Status?.Message);            
            foreach(var header in Headers)
            {
                builder.AppendLine(header.Key + ": " + header.Value);
            }
            builder.AppendLine("Content-Length: " + Body.Length);
            builder.AppendLine();            
            return builder.ToString();
        }
        public byte[] Encode()
        {
            return Encoding.Default.GetBytes(ToString()).Concat(Body).ToArray();
        }
    }
}
