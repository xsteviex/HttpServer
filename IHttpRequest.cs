using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public class HttpHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class HttpRequest
    {
        public string Method { get; set; }
        public Uri RequestUri { get; set; }
        public string HttpVersion { get; set; }
        public Uri Host { get; set; }
        public IList<HttpHeader> Headers { get; set; }
        public string Body { get; set; }

        public static HttpRequest Parse(string req)
        {
            try
            {
                if (req != null)
                {
                    var lines = req.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var FirstLine = lines[0].Split(new[] { ' ' });
                    var Host = new Uri(@"http://" + lines[1].Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries).Last());
                    return new HttpRequest
                    {
                        Method = FirstLine[0],
                        RequestUri = new Uri(Host, FirstLine[1]),
                        HttpVersion = FirstLine[2],
                        Host = Host,
                        Headers = lines.Skip(2).TakeWhile(r => r != "\n").Select(c => new HttpHeader() { Key = c.Replace("\n", "").Split(new[] { ':' }).First(), Value = c.Replace("\n", "").Split(new[] { ':' }).Last() }).ToList(),
                        Body = string.Join("", lines.SkipWhile(r => r != "\n").Skip(1).Select(c => c.Replace("\n", "")))
                    };
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
            return null;
        }
    }
}
