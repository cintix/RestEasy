using System.Text;
using RestEasy.IO;
using RestEasy.Web.Generator;
using RestEasy.Web.HTML;

namespace RestEasy.Web {
    public class Response {
        private string _contentType = "application/json";
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        private static Dictionary<string, ModelGenerator> _generators = new Dictionary<string, ModelGenerator>();
        private Dictionary<string, string> _variables = new Dictionary<string, string>();
        private readonly string dateNow = DateTime.Now.ToString("EEE, dd MMM yyyy HH:mm:ss z");
        private byte[] _content = Array.Empty<byte>();
        private int _status = 200;

        public Response(){
        }
        public static Dictionary<string, ModelGenerator> Generators{
            get => _generators;
            set => _generators = value ?? throw new ArgumentNullException(nameof(value));
        }

        
        public static void AddGenerators(Dictionary<string, ModelGenerator> _knownGenerators){
            foreach (var _generator in _knownGenerators){
                _generators.Add(_generator.Key, _generator.Value);
            }
        }

        public Response ContentType(string contentType){
            _contentType = contentType;
            return this;
        }
        private ModelGenerator GetGenerator(){
            if (_generators.ContainsKey(_contentType)) return _generators[_contentType];
            _contentType = "text/plain";
            return _generators["default"];
        }

        public Response Variable(string name, string value){
            _variables.Add("@" + name, value);
            return this;
        }

        public Response OK(){
            _status = (int)RestEasy.Web.Base.Status.OK;
            return this;
        }

        public Response Created(){
            _status = (int)RestEasy.Web.Base.Status.Created;
            return this;
        }

        public Response Accpeted(){
            _status = (int)RestEasy.Web.Base.Status.Accpeted;
            return this;
        }

        public Response BadRequest(){
            _status = (int)RestEasy.Web.Base.Status.BadRequest;
            return this;
        }

        public Response Unauthorized(){
            _status = (int)RestEasy.Web.Base.Status.Unauthorized;
            return this;
        }

        public Response Forbidden(){
            _status = (int)RestEasy.Web.Base.Status.Forbidden;
            return this;
        }

        public Response NotFound(){
            _status = (int)RestEasy.Web.Base.Status.NotFound;
            return this;
        }

        public Response BadGateway(){
            _status = (int)RestEasy.Web.Base.Status.BadGateway;
            return this;
        }

        public Response ServiceUnavailable(){
            _status = (int)RestEasy.Web.Base.Status.ServiceUnavailable;
            return this;
        }

        public Response InternalServerError(){
            _status = (int)RestEasy.Web.Base.Status.InternalServerError;
            return this;
        }

        public int GetStatus (){
            return _status;
        }

        public Response Status(int code){
            _status = code;
            return this;
        }

        public Response MovedTemporary(){
            _status = (int)RestEasy.Web.Base.Status.MovedTemporary;
            return this;
        }

        public Response MovedPermanently(){
            _status = (int)RestEasy.Web.Base.Status.MovedPermanently;
            return this;
        }

        public Response NoContent(){
            _status = (int)RestEasy.Web.Base.Status.NoContent;
            return this;
        }

        public Response Location(string uri){
            if (Headers.ContainsKey("Location")){
                Headers["Location"] = uri;
                return this;
            }

            Headers.Add("Location", uri);
            return this;
        }

        public Response Model(object model){
            ModelGenerator generator = GetGenerator();
            _content = Encoding.UTF8.GetBytes(generator.FromModel(model));
            return this;
        }

        public Response Document(Request request, string name){
            _contentType = "text/html";
            string path = Configuration.Get("wwwroot") ?? ".";
            string file = path + "/" + name;
            if (File.Exists(file)){
                if (request.Headers.ContainsKey("Cookie")) {
                    if (Headers.ContainsKey("Set-Cookie")) Headers["Set-Cookie"] = request.Headers["Cookie"];
                    else Headers.Add("Set-Cookie",request.Headers["Cookie"]);
                }
                try{
                    Dictionary<string, string?> properties = new Dictionary<string, string?>();
                    foreach (var element in request.Post){
                        properties.Add(element.Key, element.Value);
                    }

                    foreach (var element in request.Query){
                        properties.Add(element.Key, element.Value);
                    }

                    foreach (var element in _variables){
                        properties.Add(element.Key, element.Value);
                    }

                    Dictionary<string, object> resources = new Dictionary<string, object>();
                    resources.Add("Request", request);
                    _content = Encoding.UTF8.GetBytes(Engine.Process(file, properties, resources));
                } catch (Exception ex){
                    Console.WriteLine(ex);
                }
            }

            return this;
        }

        public Response Content(byte[] content){
            _content = content;
            return this;
        }

        public Response Data(String data){
            _content = Encoding.UTF8.GetBytes(data);
            return this;
        }

        public byte[] Build(){
            byte[] _data = new byte[0]; 
            using (MemoryStream outputStream = new MemoryStream()){
                string response = "HTTP/1.1 " + _status + " " + MessageFromStatus(_status) + "\n";
                response += "Date: " + dateNow + "\n";

                if (!Headers.ContainsKey("Server")){
                    response += "Server: #Cintix-Application-Server(CAS)/1.1\n";
                }

                foreach (string key in Headers.Keys){
                    response += key + ": " + Headers[key] + "\n";
                }

                if (!Headers.ContainsKey("Content-Type") && _content.Length > 0){
                    response += "Content-Type: " + _contentType;
                    if (_contentType.ToLower().Contains("/text")){
                        response += "; charset=utf-8";
                    }

                    if (_contentType.ToLower().Contains("/json")){
                        response += "; charset=utf-8";
                    }

                    if (_contentType.ToLower().Contains("plain")){
                        response += "; charset=utf-8";
                    }

                    if (_contentType.ToLower().Contains("html")){
                        response += "; charset=utf-8";
                    }

                    response += "\n";
                }

                if (!Headers.ContainsKey("Connection")){
                    response += "Connection: Closed\n";
                }

                response += "Content-Length: " + _content.Length + "\n";
                response += "\n";

                outputStream.Write(Encoding.UTF8.GetBytes(response));
                if (_content.Length > 0){
                    outputStream.Write(_content);
                }

                _data = outputStream.GetBuffer();
            }

            return _data;
        }

        private String MessageFromStatus(int code){
            if (code == 200){
                return "OK";
            }

            if (code == 201){
                return "Created";
            }

            if (code == 202){
                return "Accepted";
            }

            if (code == 204){
                return "No Content";
            }

            if (code == 301){
                return "Moved Permanently";
            }

            if (code == 302){
                return "Temporary Redirect";
            }

            if (code == 400){
                return "Bad Request";
            }

            if (code == 401){
                return "Unauthorized";
            }

            if (code == 403){
                return "Forbidden";
            }

            if (code == 404){
                return "Not Found";
            }

            if (code == 502){
                return "Bad Gateway";
            }

            if (code == 503){
                return "Service Unavailable";
            }

            if (code == 500){
                return "Internal Server Error";
            }

            return "Status";
        }
    }
}