using System.Text;

namespace RestEasy.Web.HTML;

public static class Engine {
    private static readonly Dictionary<string, Type> RegisteredClasses = new Dictionary<string, Type>();
    private static string _namespace = "server";

    public static void SetNamespace(string name){
        _namespace = name;
    }

    public static void Add(string name, Type type){
        if (_namespace.Length == 0) throw new InvalidOperationException("can't register type without namespace");
        string key = _namespace + ":" + name;
        if (!RegisteredClasses.ContainsKey(key)) RegisteredClasses.Add(key, type);
    }

    private static string ReadFile(string file){
        if (File.Exists((file))) return File.ReadAllText(file, Encoding.UTF8);
        return "";
    }

    private static List<string> ReadParameters(string line){
        List<string> parameters = new List<string>();
        int offset = 0;
        line = line.Replace("'", "\"");

        while (true){
            offset = line.IndexOf("=", offset, StringComparison.Ordinal);
            if (offset == -1){
                break;
            }

            int start = 0; // line.IndexOf(" ");
            if (start > offset){
                start = 0;
            }

            int firstMark = line.IndexOf("\"", offset, StringComparison.Ordinal);
            int lastMark = line.IndexOf("\"", firstMark + 1, StringComparison.Ordinal);

            if (firstMark == -1 || lastMark == -1){
                break;
            }

            lastMark++;
            parameters.Add(line.Substring(start, lastMark).Trim());
            line = line.Substring(lastMark + 1);
            offset = 0;
        }

        return parameters;
    }

    private static Tag? ProcessTag(string code, Dictionary<string, string?> predefinedValues){
        int keyOffset = code.IndexOf(":", StringComparison.Ordinal);
        Tag? htmlTag = null;
        int classEndIndex = code.IndexOf(" ", keyOffset, StringComparison.Ordinal);
        if (classEndIndex == -1){
            classEndIndex = code.IndexOf(">", keyOffset, StringComparison.Ordinal);
        }

        string clsKey = code.Substring(1, classEndIndex-1);
        try{
            string propertiesString = code.Substring(1 + clsKey.Length, code.Length - (clsKey.Length+2)).Trim();
            if (propertiesString.EndsWith("/")){
                propertiesString = propertiesString.Substring(0, propertiesString.Length - 1);
            }

            Dictionary<string, string?> properties = new Dictionary<string, string?>();
            List<string> propertyKeys = ReadParameters(propertiesString);
            foreach (string property in propertyKeys){
                if (!property.Contains("=")){
                    continue;
                }

                string[] keyValue = property.Split("=");
                string? key = keyValue[0].Trim();
                string? value = keyValue[1].Trim();

                if (value.Equals("\"\"", StringComparison.OrdinalIgnoreCase)){
                    value = "";
                }

                if (value != null && value != ""){
                    value = value.Substring(1);
                    value = value.Substring(0, value.Length - 1);
                }

                properties.Add(key, value);
            }
                     
            if (RegisteredClasses.ContainsKey(clsKey)){
                htmlTag = (Tag)Activator.CreateInstance(RegisteredClasses[clsKey])!;
                htmlTag.AddProperties(properties);
                htmlTag.AddProperties(predefinedValues);
            }
        } catch (Exception e){
            Console.WriteLine("ProcessTag Exception: " + e);
        }

        return htmlTag;
    }

    private static string ProcessHtml(string code, Dictionary<string, string?> properties,
        Dictionary<string, object> resources){
        int offset = 0;
        int lastOffset;
        string prefix = "<" + _namespace + ":";
        string prefixEnd = "</" + _namespace + ":";
        string parseHtml = "";

        while (offset != -1){
            lastOffset = offset;
            offset = code.IndexOf(prefix, offset, StringComparison.Ordinal);
            if (offset == -1 && lastOffset > 0){
                if (lastOffset < code.Length){
                    parseHtml += code.Substring(lastOffset);
                }
            }

            if (offset >= 0){
                if (lastOffset != offset){
                    parseHtml += code.Substring(lastOffset, offset-lastOffset);
                }

                int tagEndIndex = code.IndexOf(">", offset, StringComparison.Ordinal);
                string tagData = code.Substring(offset,  (tagEndIndex + 1) - offset);
                if (tagData.Contains("/")){
                    Tag? proccessTag = ProcessTag(tagData, properties);
                    if (proccessTag != null){
                        proccessTag.AddResource(resources);
                        parseHtml += proccessTag.StartTag() + proccessTag.EndTag();
                    }

                    offset = tagEndIndex + 1;
                } else{
                    int endingTagOffset = code.IndexOf(prefixEnd, tagEndIndex, StringComparison.Ordinal);
                    if (endingTagOffset != -1){
                        int endTagEndIndex = code.IndexOf(">", endingTagOffset, StringComparison.Ordinal);
                        
                        string innerHtml = code.Substring(tagEndIndex + 1, endingTagOffset -(tagEndIndex +1));

                        Tag? proccessTag = ProcessTag(tagData, properties);
                        if (proccessTag != null){
                            proccessTag.AddResource(resources);
                            parseHtml += proccessTag.StartTag();
                        }

                        parseHtml += ProcessHtml(innerHtml, properties, resources);
                        if (proccessTag != null){
                            parseHtml += proccessTag.EndTag();
                        }

                        offset = endTagEndIndex + 1;
                    } else{
                        throw new IOException("Invalid tags missing end for " + prefix);
                    }
                }
            }
        }

        if (parseHtml.Trim() == ""){
            return code;
        }
        return parseHtml;
    }

    public static string Process(string file, Dictionary<string, string?> properties,
        Dictionary<string, object> resources){
        string fileData = ReadFile(file);
        string parseHtml = ProcessHtml(fileData, properties, resources);
        return parseHtml.Trim();
    }

    public static string Process(string file){
        return Process(file, new Dictionary<string, string?>(), new Dictionary<string, object>());
    }

    private static string ProcessHTML(string code){
        return ProcessHtml(code, new Dictionary<string, string?>(), new Dictionary<string, object>());
    }
}