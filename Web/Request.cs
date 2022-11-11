using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using RestEasy.IO;

namespace RestEasy.Web {
    public class Request {
        public readonly Dictionary<string, string> Headers = new Dictionary<string, string>();
        public readonly Dictionary<string, string> Post = new Dictionary<string, string>();
        public readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();
        public readonly Dictionary<string, string> Query = new Dictionary<string, string>();

        public string ContentType = "application/json";
        public string? RawRequest;
        public string Path { get; } = "";
        public int ClientPort { get; } = 0;
        public string ClientIP { get; } = "";
        public string Server { get; } = "";
        public string Uri { get; } = "";
        public string Method { get; } = "";

        public string ContextPath { get; set; } = "";

        public Request(Socket? clientSocket, string serverName, byte[] clientRequestData)
        {
            if (clientSocket == null) return;
            try {
                string[] clientInfo = clientSocket.RemoteEndPoint.ToString().Split(":");
                ClientIP = clientInfo[0];
                if (clientInfo.Length > 1) ClientPort = Int32.Parse(clientInfo[1]);
                
                string clientRequest = Encoding.UTF8.GetString(clientRequestData);
                string[] lines = clientRequest.Split("\r\n");
                int lineCount = lines.Length;
                int currentLineIndex = 1;

                int indexOfFromData = clientRequest.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
                if (indexOfFromData < clientRequest.Length) {
                    RawRequest = clientRequest.Substring(indexOfFromData);
                }

                if (indexOfFromData == -1) {
                    indexOfFromData = clientRequest.IndexOf("\n\n", StringComparison.Ordinal) + 2;
                    if (indexOfFromData < clientRequest.Length) {
                        RawRequest = clientRequest.Substring(indexOfFromData);
                    }
                }

                string[] requestLineParts = lines[0].Split(' ');
                if (requestLineParts.Length < 2) return;

                Method = requestLineParts[0].Trim().ToUpper();
                Path = requestLineParts[1].Trim();


                if (Path.IndexOf('?') > 0) {
                    int queryIndex = Path.IndexOf('?');

                    string queryString = Path.Substring(queryIndex + 1);
                    string[] queryFields = queryString.Split('&');

                    Path = Path.Substring(0, queryIndex);
                    foreach (string queryField in queryFields) {
                        string[] keyValue = queryField.Split('=');
                        if (keyValue.Length == 1) {
                            Query.Add(keyValue[0], "");
                        } else {
                            Query.Add(keyValue[0], HttpUtility.UrlDecode(keyValue[1]));
                        }
                    }
                }

                string temp = Path;
                for (int index = 2; index < requestLineParts.Length - 1; index++) {
                    temp = requestLineParts[index] + " ";
                }
                ContextPath = temp.Trim();

                Uri = serverName + requestLineParts[1].Trim();

                for (int lineIndex = 1; lineIndex < lineCount; lineIndex++) {
                    currentLineIndex = lineIndex;
                    string line = lines[lineIndex];
                    if (line.Equals("")) {
                        break;
                    } else {
                        string[] keyValue = line.Split(':');
                        if (keyValue[0].Trim().ToLower().Equals("server")) Server = keyValue[1];
                        if (keyValue[0].Trim().ToLower().Equals("content-type")) ContentType = keyValue[1];
                        Headers.Add(keyValue[0], keyValue[1]);
                        if (keyValue[0].Trim().ToLower().Equals("host")) {
                            if (keyValue[1].Trim().Contains(".")) {
                                Headers.Add("SubDomain", keyValue[1].Trim().Substring(0,keyValue[1].Trim().IndexOf(".", StringComparison.Ordinal) -1));
                            }
                        }
                    }
                }

                if (Headers.ContainsKey("Content-Type") && Headers["Content-Type"].Contains("multipart/form-data")) {
                    string[] boundaryKeys = Headers["Content-Type"].Split("boundary=");
                    
                    byte[] boundaryPattern = Encoding.UTF8.GetBytes(boundaryKeys[1]);
                    byte[] formOffset = Encoding.UTF8.GetBytes("\r\n\r\n");
                    byte[] lineOffset = Encoding.UTF8.GetBytes("\r\n");

                    int boundaryKeyLength = boundaryKeys[1].Length;
                    int[] dataBegin = clientRequestData.Locate(formOffset);
                    int dataBeginOffset = dataBegin[0];
                    int[] boundariesOffsets = clientRequestData.Locate(boundaryPattern, dataBeginOffset);

                    for (int index = 0; index < boundariesOffsets.Length; index++) {
                        int sectionOffset = boundariesOffsets[index] + boundaryKeyLength + 1;
                        int[] sectionEnd = clientRequestData.Locate(lineOffset, sectionOffset);
                        int sectionEndOffset = sectionEnd[0];

                        string sectionType = clientRequestData.RangeToString(sectionOffset, sectionEndOffset - sectionOffset);
                        int dataOffset = sectionEndOffset + 4;

                        if (sectionType.Contains("form-data")) {
                            int nameStart = sectionType.IndexOf("name=", StringComparison.Ordinal) + 6;
                            int nameLength = sectionType.IndexOf("\"", nameStart, StringComparison.Ordinal) - nameStart;
                            string name = sectionType.Substring(nameStart, nameLength);

                            if (sectionType.Contains("filename=")) {
                                int filenameStart = sectionType.IndexOf("filename=", StringComparison.Ordinal) + 10;
                                int filenameLength = sectionType.IndexOf("\"", filenameStart, StringComparison.Ordinal) - filenameStart;
                                string filename = sectionType.Substring(filenameStart, filenameLength);

                                int[] contentBegin = clientRequestData.Locate("\r\n\r\n", dataOffset);
                                dataOffset = contentBegin[0] + 4;
                                
                                Post.Add(name, filename);
                                Files.Add(filename, clientRequestData.GetRange(dataOffset, boundariesOffsets[index + 1] - dataOffset - 4));
                            } else {
                                string postValue = clientRequestData.RangeToString(dataOffset, boundariesOffsets[index + 1] - dataOffset - 4);
                                Post.Add(name, postValue);
                            }
                        }

                    }

                } else {
                    if (currentLineIndex < lineCount) {
                        currentLineIndex++;
                        string[] postData = lines[currentLineIndex].Split('&');
                        foreach (string postField in postData) {
                            if (postField.Trim() == "") break;
                            string[] keyValue = postField.Split('=');
                            if (keyValue.Length == 1) {
                                Post.Add(keyValue[0], "");
                            } else {
                                Post.Add(keyValue[0], HttpUtility.UrlDecode(keyValue[1]));
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private void PrintOutDictionary(Dictionary<string, string> dic){
            string output = "";
            foreach (KeyValuePair<string, string> item in dic) {
                output += string.Format("Key = {0}, Value = {1}", item.Key, item.Value);
                output += "\n";
            }

            Console.WriteLine(output);
        }

        public override string ToString(){
            return
                $"{nameof(Path)}: {Path}, {nameof(Server)}: {Server}, {nameof(Uri)}: {Uri}, {nameof(Method)}: {Method}";
        }
    }
}