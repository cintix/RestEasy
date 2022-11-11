using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace RestEasy.Web;

public class WebSocket
{
    private readonly IPAddress _ipAddress;
    private readonly IPEndPoint _localEndPoint;
    private readonly int _maxConnections = 100;

    private Thread? _serverThread;
    private Socket? _serverSocket;

    private volatile bool _running;
    private static string _serverName = "localhost";

    
    public WebSocket(int port, IPEndPoint localEndPoint) {
        _serverName = Dns.GetHostName();
        _ipAddress = IPAddress.Any;
        _localEndPoint = new IPEndPoint(_ipAddress, port);
    }

    public WebSocket(string hostOrIp, int port, IPEndPoint localEndPoint) {
        _localEndPoint = localEndPoint;
        IPHostEntry host;
        if (hostOrIp == "0.0.0.0") {
            _serverName = Dns.GetHostName();
            _ipAddress = IPAddress.Any;
        } else {
            _serverName = hostOrIp;
            host = Dns.GetHostEntry(hostOrIp);
            _ipAddress = host.AddressList[0];
        }        
    }

    public WebSocket(string hostOrIP, int port, int connections, IPEndPoint localEndPoint) {
        IPHostEntry host;
        if (hostOrIP == "0.0.0.0") {
            _serverName = Dns.GetHostName();
            _ipAddress = IPAddress.Any;
        } else {
            _serverName = hostOrIP;
            host = Dns.GetHostEntry(hostOrIP);
            _ipAddress = host.AddressList[0];
        }

        this._maxConnections = connections;
        _localEndPoint = localEndPoint;
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
     
                Response response = new Response();

                clientSocket.Send(response.Build());
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            clientSocket.Close();
        }
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