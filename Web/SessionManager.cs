using System.Text;
using RestEasy.IO;
namespace RestEasy.Web {
    public class SessionManager {
        private static readonly byte[] _keys = Encoding.UTF8.GetBytes(" 4yregIcUNorupOvadIFYlUREDiTURURevyXULeNUNYqopajuVUKaCOBiHUMaReNoWENUlArImexulICIKIqIcikECyVAFylOBaJYgyGEwEGIcaGIjapevYXIfUFivyxixeRURUBOSehiLEnOvEHyxODuHyHoTARiDOpivaZUtedEHoCUHeNUJElasaHYWIwOsORiGIGuGYBoQaXIqAlIPulUluVUhygAtugysYhugosYpalIjAfUDyWaVYCuDIb]");
        private static readonly Dictionary<string, SessionItem> _session = new Dictionary<string, SessionItem>();
        private static readonly Random _random = new Random();

        public static long TimeOut { get; set; } = 1000 * 60 * 10;

        public static string register(string clientIP){
            string clientKey = RandomString(32);
            byte[] clientKeyData = Encoding.UTF8.GetBytes(clientKey);
            byte[] clientKeyDataEncrypted = Crypt(clientKeyData);
            string clientSessionKey = Convert.ToBase64String(clientKeyDataEncrypted);
            _session.Add(clientKey, new SessionItem(null));
            return clientSessionKey;
        }
        
        public static void Set(string clientIP, string sessionKey, object? obj){
            byte[] encryptedData = Convert.FromBase64String(sessionKey);
            byte[] clientKeyData = Crypt(encryptedData);
            string clientKey = Encoding.UTF8.GetString(clientKeyData);
            if (_session.ContainsKey(clientKey)) {
                SessionItem item = _session[clientKey];
                item.SessionObject = obj;
                _session[clientKey] = item;
            }
        }
        
        public static T? Get<T>(string clientIP, string sessionKey){
            byte[] encryptedData = Convert.FromBase64String(sessionKey);
            byte[] clientKeyData = Crypt(encryptedData);
            string clientKey = Encoding.UTF8.GetString(clientKeyData);
            if (_session.ContainsKey(clientKey)) {
                object? obj  = _session[clientKey].SessionObject;
                return (T) obj!;
            }
            return default;
        }

        private static string RandomString(int Length){
            byte[] stringData = new byte[Length];
            for (int index = 0; index < Length; index++) {
                stringData[index] = (byte) _random.Next(64, 90);
            }
            string key = Encoding.UTF8.GetString(stringData);
            if (_session.ContainsKey(key)) return RandomString(Length);
            else return key;
        }

        private static byte[] Crypt(byte[] data){
            byte[] cryptedData = new byte[data.Length];
            int keyIndex = 0;
            for (int index = 0; index < data.Length; index++) {
                if (keyIndex >=  _keys.Length) keyIndex = 0;
                cryptedData[index] = (byte) (_keys[keyIndex] ^ data[index]);
            }
            return cryptedData;
        }
    }

}