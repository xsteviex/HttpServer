using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public interface IHttpRequestHandler
    {
        byte[] Respond(HttpRequest request);
    }

    public class DefaultHtmlRequestHandler : IHttpRequestHandler
    {
        protected readonly Dictionary<string, string> MimeTypes;
        protected readonly string _server;
        protected readonly string _path;
        protected readonly string _error;
        protected readonly IEnumerable<string> Default;
        public DefaultHtmlRequestHandler(string serverRoot, IEnumerable<string> defaultFiles)
        {
            MimeTypes = new Dictionary<string, string>() { { ".html", "text/html" }, { ".jpg", "image/jpeg" }, { ".js", "application/x-javascript" } };
            _server = serverRoot;
            _path = _server + "/www";
            _error = _server + "/ErrorPages";
            Default = defaultFiles;
        }
        public virtual byte[] Respond(HttpRequest request)
        {            
            var requestedDocument = _path + request.RequestUri.LocalPath;
            try
            {
                if (request.Method == "GET")
                {
                    if (File.Exists(requestedDocument))
                    {
                        var info = new FileInfo(requestedDocument);
                        var mime = "";
                        if (!MimeTypes.TryGetValue(info.Extension.ToLower(), out mime))
                        {
                            mime = "text/html";
                        }
                        return new HttpResponse()
                        {
                            Body = File.ReadAllBytes(requestedDocument),
                            Status = new HttpStatusCode()
                            {
                                Code = 200,
                                Message = "OK",
                                Description = "File was found"
                            },
                            Headers = new List<HttpHeader>
                            {
                                new HttpHeader { Key = "Date", Value = DateTime.Now.ToString() },
                                new HttpHeader { Key = "Content-type", Value = mime }
                            }
                        }.Encode();
                    }
                    if (Directory.Exists(requestedDocument))
                    {
                        foreach (var file in Default)
                        {
                            try
                            {
                                return new HttpResponse()
                                {
                                    Body = File.ReadAllBytes(requestedDocument + file),
                                    Status = new HttpStatusCode()
                                    {
                                        Code = 200,
                                        Message = "OK",
                                        Description = "File was found"
                                    },
                                    Headers = new List<HttpHeader>
                                    {
                                        new HttpHeader { Key = "Date", Value = DateTime.Now.ToString() },
                                        new HttpHeader { Key = "Content-type", Value = "text/html" }
                                    }
                                }.Encode();
                            }
                            catch (FileNotFoundException)
                            {
                                //Continue
                            }
                        }                        
                    }
                    throw new FileNotFoundException();
                }
                return new HttpResponse()
                {
                    Body = File.ReadAllBytes(_error + "/501.html"),
                    Status = new HttpStatusCode()
                    {
                        Code = 501,
                        Message = "Not Implemented",
                        Description =
                            "We aren't able to service this request because we haven't implemented this functionality yet"
                    },
                    Headers = new List<HttpHeader>
                    {
                        new HttpHeader { Key = "Date", Value = DateTime.Now.ToString() },
                        new HttpHeader { Key = "Content-type", Value = "text/html" }
                    }
                }.Encode();
            } catch (FileNotFoundException)
            {
                return new HttpResponse()
                {
                    Body = File.ReadAllBytes(_error + "/404.html"),
                    Status =
                        new HttpStatusCode
                        {
                            Code = 404,
                            Message = "File Not Found",
                            Description = "We couldn't find the file you were looking for"
                        },
                        Headers = new List<HttpHeader>
                        {
                            new HttpHeader { Key = "Date", Value = DateTime.Now.ToString() },
                            new HttpHeader { Key = "Content-type", Value = "text/html" }
                        }
                }.Encode();
            }
        }
    }    
}
