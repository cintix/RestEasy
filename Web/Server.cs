using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using RestEasy.IO;
using RestEasy.Web.Annotations;
using RestEasy.Web.Base;
using RestEasy.Web.Documentation;
using RestEasy.Web.Generator;
using RestEasy.Web.HTML;
using Action = RestEasy.Web.Annotations.Action;

namespace RestEasy.Web {
    public class Server {
        public static Dictionary<string, ModelGenerator> Generators = new Dictionary<string, ModelGenerator>();
        private static Dictionary<string, Dictionary<string, Endpoint>> pathMapping = new Dictionary<string, Dictionary<string, Endpoint>>();
        private readonly Dictionary<string, Documentation.Service> documentationEndpoint = new Dictionary<string, Documentation.Service>();

        private readonly IPAddress _ipAddress;
        private readonly IPEndPoint _localEndPoint;
        private readonly int _maxConnections = 100;

        private volatile bool _running;
        private string _documentRoot = "web";
        private Thread? _serverThread;
        private Socket? _serverSocket;
        private static string _serverName = "localhost";


        public void SetDocumentRoot(string path){
            _documentRoot = path;
            if (_documentRoot != null && _documentRoot != "") {
                while (_documentRoot.Trim().EndsWith("/")) {
                    _documentRoot = _documentRoot.Trim().Substring(0, -1);
                }
            }
            Configuration.Set("wwwroot", GetDocumentRoot());
        }

        public string GetDocumentRoot(){
            _documentRoot ??= ".";
            while (_documentRoot.Trim().EndsWith("/")) {
                _documentRoot = _documentRoot.Trim().Substring(0, -1);
            }
            return _documentRoot;
        }

        public void AddEndpoint(string path, object endpoint){
            documentationEndpoint.Add(path + "?jsd", JsonServiceDescriptionEngine.GenerateServiceDefinition(path, null, endpoint));
            RegisterEndpoint(pathMapping, path, endpoint);
        }


        public Server(int port){
            _serverName = Dns.GetHostName();
            _ipAddress = IPAddress.Any;
            _localEndPoint = new IPEndPoint(_ipAddress, port);

            if (!pathMapping.ContainsKey("get"))
                pathMapping.Add("get", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("put"))
                pathMapping.Add("put", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("post"))
                pathMapping.Add("post", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("delete"))
                pathMapping.Add("delete", new Dictionary<string, Endpoint>());

            Response.AddGenerators(Server.Generators);
        }

        public Server(string hostOrIP, int port){
            IPHostEntry host;
            if (hostOrIP == "0.0.0.0") {
                _serverName = Dns.GetHostName();
                _ipAddress = IPAddress.Any;
            } else {
                _serverName = hostOrIP;
                host = Dns.GetHostEntry(hostOrIP);
                _ipAddress = host.AddressList[0];
            }

            _localEndPoint = new IPEndPoint(_ipAddress, port);

            if (!pathMapping.ContainsKey("get"))
                pathMapping.Add("get", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("put"))
                pathMapping.Add("put", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("post"))
                pathMapping.Add("post", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("delete"))
                pathMapping.Add("delete", new Dictionary<string, Endpoint>());

            Response.AddGenerators(Server.Generators);
        }

        public Server(string hostOrIP, int port, int connections){
            IPHostEntry host;
            if (hostOrIP == "0.0.0.0") {
                _serverName = Dns.GetHostName();
                _ipAddress = IPAddress.Any;
            } else {
                _serverName = hostOrIP;
                host = Dns.GetHostEntry(hostOrIP);
                _ipAddress = host.AddressList[0];
            }

            _localEndPoint = new IPEndPoint(_ipAddress, port);
            _maxConnections = connections;

            if (!pathMapping.ContainsKey("get"))
                pathMapping.Add("get", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("put"))
                pathMapping.Add("put", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("post"))
                pathMapping.Add("post", new Dictionary<string, Endpoint>());

            if (!pathMapping.ContainsKey("delete"))
                pathMapping.Add("delete", new Dictionary<string, Endpoint>());

            Response.AddGenerators(Server.Generators);
        }

        public void AddHTMLTag(string name, Type _type){
            Engine.Add(name, _type);
        }

        public bool IsRunning(){
            return _running;
        }

        private void HandleClientRequest(object? obj){
            if (obj != null) {
                Socket? clientSocket = (Socket) obj;
                byte[]? bytes = null;

                if (clientSocket == null) return;

                List<byte> byteArray = new List<byte>();
                bytes = new byte[1024 * 4];
                int bytesRec = 1;
                int totalRecBytes = 0;
                int contentLength = 0;

                while (bytesRec > 0) {
                    bytesRec = clientSocket.Receive(bytes);
                    totalRecBytes += bytesRec;
                    for (int index = 0; index < bytesRec; index++) byteArray.Add(bytes[index]);

                    string tmp = Encoding.UTF8.GetString(byteArray.ToArray());
                    if (tmp.Contains("Content-Length: ")) {
                        Regex reg = new Regex("\\\r\nContent-Length: (.*?)\\\r\n");
                        Match m = reg.Match(tmp);
                        contentLength = int.Parse(m.Groups[1].ToString());
                    }
                    if (contentLength == 0 || contentLength <= totalRecBytes) break;
                }

                string serverUri = _serverName;
                if (_localEndPoint.Port != 80) {
                    serverUri += ":" + _localEndPoint.Port;
                }

                try {
                    Request request = new Request(clientSocket, serverUri, byteArray.ToArray());
                    if (byteArray.Count < 1) {
                        clientSocket.Close();
                        return;
                    }
                    
                    Response response = HandleRequestMapping(pathMapping, request);
                    if (request.Headers.ContainsKey("Cookie")) {
                        if (response.Headers.ContainsKey("Set-Cookie")) {
                            response.Headers["Set-Cookie"] = request.Headers["Cookie"];
                        } else {
                            response.Headers.Add("Set-Cookie", request.Headers["Cookie"]);
                        }
                    }
                    clientSocket.Send(response.Build());
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
                clientSocket.Close();
            }
        }
        private bool IsRequestADocument(string context){
            
            /*
            Console.WriteLine(GetDocumentRoot() + context);
            Console.WriteLine(_documentRoot);
            Console.WriteLine(Path.GetFullPath(GetDocumentRoot() + context));
            Console.WriteLine();
            */
            
            if (File.Exists(GetDocumentRoot() + context)) {
                if (Path.GetFullPath(GetDocumentRoot() + context).StartsWith(Path.GetFullPath(_documentRoot))) {
                    return true;
                }
            }
            return false;
        }

        private Response HandleRequestMapping(Dictionary<string, Dictionary<string, Endpoint>> pathMapping, Request request){
            string contextPath = request.ContextPath;

            if ((contextPath.Trim().ToLower().Equals("") || contextPath.Trim().ToLower().Equals("/")) && request.Query.ContainsKey("jsd")) {
                API api = new API();
                foreach (Documentation.Service service in documentationEndpoint.Values) {
                    api.AddService(service);
                }
                return new Response().OK().ContentType("application/json").Model(api);
            }


            if (contextPath.Equals("") || contextPath == "/") {
                contextPath = "/index.htm";
                if (!IsRequestADocument(contextPath)) {
                    contextPath = "/index.html";
                }
            }

            if (IsRequestADocument(contextPath) && Configuration.Get("wwwroot") != null) {
                string documentFile = Path.GetFullPath(GetDocumentRoot() + contextPath);
                if (contextPath.ToLower().EndsWith(".htm") || contextPath.ToLower().EndsWith(".html")) {

                    Dictionary<string, string?> properties = new Dictionary<string, string?>();
                    foreach (var post in request.Post) {
                        properties.Add(post.Key, post.Value);
                    }
                    foreach (var post in request.Query) {
                        properties.Add(post.Key, post.Value);
                    }

                    Dictionary<string, object> resources = new Dictionary<string, object>();
                    resources.Add("Request", request);

                    string contentData = Engine.Process(documentFile, properties, resources);
                    return new Response().OK().ContentType("text/html").Data(contentData);
                }

                string fileExt = contextPath.Substring(contextPath.LastIndexOf(".") + 1);
                string contextType = MimeTypes.ContentType(fileExt);

                byte[] fileContent = File.ReadAllBytes(documentFile);
                return new Response().OK().ContentType(contextType).Content(fileContent);
            }


            contextPath = request.ContextPath;
            Dictionary<string, Endpoint> requestMap = pathMapping[request.Method.ToLower()];
            RestAction? restAction = LocateEndpoint(requestMap, contextPath.Trim());

            if (restAction != null) {
                return restAction.process(request);
            } else {
                return new Response().NotFound();
            }
        }

        private RestAction? LocateEndpoint(Dictionary<string, Endpoint> mapping, string contextPath){
            if (mapping.ContainsKey(contextPath)) {
                return new RestAction(mapping[contextPath], new List<string?>());
            }

            List<string> regexMapping = new List<string>();
            foreach (var map in mapping) {
                regexMapping.Add(map.Key);
            }

            regexMapping.OrderBy(c => c.Length);
            regexMapping?.Reverse();


            if (regexMapping != null)
                foreach (string pattern in regexMapping) {
                    if (!pattern.StartsWith("^")) {
                        continue;
                    }

                    Regex regex = new Regex(pattern);
                    MatchCollection collection = regex.Matches(contextPath);
                    bool found = false;
                    List<string?> arguments = new List<string?>();

                    foreach (Match match in collection) {
                        found = true;
                        for (int index = 2; index < match.Groups.Count + 1; index++) {
                            arguments.Add(match.Groups[index].Value);
                        }
                    }

                    if (found) {
                        return new RestAction(mapping[pattern], arguments);
                    }
                }

            return default;
        }


        private void RegisterEndpoint(Dictionary<string, Dictionary<string, Endpoint>> pathMapping, string path, object endpoint){
            string _base = path;
            foreach (MethodInfo method in endpoint.GetType().GetMethods()) {
                string httpMethod = "get";
                if (method.GetCustomAttributes(typeof(POST), false).Length > 0) {
                    httpMethod = "post";
                }

                if (method.GetCustomAttributes(typeof(PUT), false).Length > 0) {
                    httpMethod = "put";
                }

                if (method.GetCustomAttributes(typeof(DELETE), false).Length > 0) {
                    httpMethod = "delete";
                }

                Dictionary<string, Endpoint> httpMethods = pathMapping[httpMethod];
                if (method.GetCustomAttributes(typeof(Action), false).Length > 0) {
                    Action actionAnnotation = (Action) method.GetCustomAttributes(typeof(Action), false)[0];
                    string actionPath = actionAnnotation.Path;
                    if (!actionPath.StartsWith("/")) {
                        actionPath = actionPath + "/" + actionAnnotation.Path;
                    }

                    if (_base == "/") _base = "";
                    String urlPattern = CompileRegexFromPath(_base + actionPath);
                    httpMethods.Add(urlPattern, new Endpoint(_base + actionPath, method, endpoint));
                    httpMethods.Add(_base + actionPath, new Endpoint(_base + actionPath, method, endpoint));
                    pathMapping[httpMethod] = httpMethods;

                }
            }
        }

        public static string BuildContextPath(string[] oldPath){
            if (oldPath == null || oldPath.Length == 0) {
                return "";
            }

            string path = "";
            for (int index = 0; index < oldPath.Length - 1; index++) {
                path += oldPath[index] + "/";
            }

            if (path != "") {
                path = path.Substring(0, -1);
            }

            return path;
        }

        public static bool ContentTypeMatch(string accept, string contentType){
            string patternString = "^" + accept.Replace("\\*", "\\S+").Replace("/", "\\/");
            Regex regex = new Regex(patternString);
            MatchCollection collection = regex.Matches(contentType);
            return collection.Any();
        }

        private string CompileRegexFromPath(string path){
            string patternString = "(\\{\\w+\\})";
            string realPattern = path.Replace("/", "\\/");

            Regex regex = new Regex(patternString);
            MatchCollection collection = regex.Matches(path);
            if (collection.Count > 0) {
                for (int index = 0; index < collection.Count; index++)
                    realPattern = realPattern.Replace(collection[index].Groups[0].ToString(), "(\\S+)");
            }
            return "^(" + realPattern + ")$";
        }


        private void StartServer(){
            try {
                _running = true;
                _serverSocket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(_localEndPoint);
                _serverSocket.Listen(_maxConnections);

                while (_running) {
                    try {
                        Socket clientSocket = _serverSocket.Accept();
                        Thread thread = new Thread(HandleClientRequest);
                        thread.Start(clientSocket);
                    } catch (Exception e) {
                        Console.WriteLine(e.ToString());
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void Stop(){
            _running = false;
            _serverSocket?.Close();
            _serverThread?.Interrupt();
        }

        public void Listen(){
            _serverThread = new Thread(StartServer);
            _serverThread.Start();
        }
    }
}