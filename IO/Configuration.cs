using System.IO.Enumeration;
using System.Text;
namespace RestEasy.IO {
    public class Configuration {
        private static Dictionary<string, string> _Values = new Dictionary<string, string>();

        public static void Load(string file){
            if (!File.Exists(file)) return;
            string data = File.ReadAllText(file);
            string[] fields = data.Split("\n");
            foreach (string field in fields) {
                if (field.Trim().Length > 0 && field.Contains("=")) {
                    string[] keyValue = field.Split("=");
                    _Values.Add(keyValue[0].Trim(), keyValue[1].Trim());
                }
            }
        }

        public static void Save(string file){
            using (TextWriter writer = new StreamWriter(file, append: false)) {
                foreach (var property in _Values) {
                    writer.WriteLine(property.Key + "=" + property.Value);
                }
            }
        }

        public static void Set(string key, string value){
            if (_Values.ContainsKey(key)) _Values[key] = value;
            else _Values.Add(key.Trim(), value.Trim());
        }
        
        public static bool Contains(string key){
            return _Values.ContainsKey(key);
        }

        public static string? Get(string key){
            if (_Values.ContainsKey(key)) return _Values[key].Trim();
            else return null;
        }

    }
}