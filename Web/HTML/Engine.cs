using System.Text;

namespace RestEasy.Web.HTML;

public class Engine {
    private static Dictionary<string, Type> _registeredClasses = new Dictionary<string, Type>();
    private static string _namespace = "server";

    public static void SetNamespace(string name){
        _namespace = name;
    }

    public static void Add(string name, Type _type){
        if (_namespace.Length == 0) throw new InvalidOperationException("can't register type without namespace");
        string key = _namespace + ":" + name;
        if (!_registeredClasses.ContainsKey(key)) _registeredClasses.Add(key, _type);
    }

    private static string ReadFile(string file){
        if (File.Exists((file))) return File.ReadAllText(file, Encoding.UTF8);
        return "";
    }

    private static List<string> ReadParamters(string line){
        List<string> parameters = new List<string>();
        int offset = 0;
        line = line.Replace("'", "\"");

        while (offset != -1){
            offset = line.IndexOf("=", offset);
            if (offset == -1){
                break;
            }

            int start = 0; // line.IndexOf(" ");
            if (start == -1 || start > offset){
                start = 0;
            }

            int firstMark = line.IndexOf("\"", offset);
            int lastMark = line.IndexOf("\"", firstMark + 1);

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

    private static Tag? ProccessTag(string code, Dictionary<string, string?> predefinedValues){
        int keyOffset = code.IndexOf(":");
        Tag? htmlTag = null;
        int classEndIndex = code.IndexOf(" ", keyOffset);
        if (classEndIndex == -1){
            classEndIndex = code.IndexOf(">", keyOffset);
        }

        string clsKey = code.Substring(1, classEndIndex-1);
        try{
            string propertiesString = code.Substring(1 + clsKey.Length, code.Length - (clsKey.Length+2)).Trim();
            if (propertiesString.EndsWith("/")){
                propertiesString = propertiesString.Substring(0, propertiesString.Length - 1);
            }

            Dictionary<string, string?> properties = new Dictionary<string, string?>();
            List<string> propertyKeys = ReadParamters(propertiesString);
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
                     
            if (_registeredClasses.ContainsKey(clsKey)){
                htmlTag = (Tag)Activator.CreateInstance(_registeredClasses[clsKey])!;
                if (htmlTag != null){
                    htmlTag.addProperties(properties);
                    htmlTag.addProperties(predefinedValues);
                }
            }
        } catch (Exception e){
            Console.WriteLine("ProcessTag Exception: " + e);
        }

        return htmlTag;
    }

    private static string ProcessHTML(string code, Dictionary<string, string?> properties,
        Dictionary<string, object> resources){
        int offset = 0;
        int lastOffset = 0;
        string prefix = "<" + _namespace + ":";
        string prefixEnd = "</" + _namespace + ":";
        string parseHTML = "";

        while (offset != -1){
            lastOffset = offset;
            offset = code.IndexOf(prefix, offset);
            if (offset == -1 && lastOffset > 0){
                if (lastOffset < code.Length){
                    parseHTML += code.Substring(lastOffset);
                }
            }

            if (offset >= 0){
                if (lastOffset != offset){
                    parseHTML += code.Substring(lastOffset, offset-lastOffset);
                }

                int tagEndIndex = code.IndexOf(">", offset);
                string tagData = code.Substring(offset,  (tagEndIndex + 1) - offset);
                if (tagData.Contains("/")){
                    Tag? proccessTag = ProccessTag(tagData, properties);
                    if (proccessTag != null){
                        proccessTag.addResource(resources);
                        parseHTML += proccessTag.startTag() + proccessTag.endTag();
                    }

                    offset = tagEndIndex + 1;
                } else{
                    int endingTagOffset = code.IndexOf(prefixEnd, tagEndIndex);
                    if (endingTagOffset != -1){
                        int endTagEndIndex = code.IndexOf(">", endingTagOffset);
                        
                        string innerHTML = code.Substring(tagEndIndex + 1, endingTagOffset -(tagEndIndex +1));

                        Tag? proccessTag = ProccessTag(tagData, properties);
                        if (proccessTag != null){
                            proccessTag.addResource(resources);
                            parseHTML += proccessTag.startTag();
                        }

                        parseHTML += ProcessHTML(innerHTML, properties, resources);
                        if (proccessTag != null){
                            parseHTML += proccessTag.endTag();
                        }

                        offset = endTagEndIndex + 1;
                    } else{
                        throw new IOException("Invalid tags missing end for " + prefix);
                    }
                }
            }
        }

        if (parseHTML.Trim() == ""){
            return code;
        }
        return parseHTML;
    }

    public static string Process(string file, Dictionary<string, string?> properties,
        Dictionary<string, object> resources){
        string filedata = ReadFile(file);
        string parseHTML = ProcessHTML(filedata, properties, resources);
        return parseHTML.Trim();
    }

    public static string Process(string file){
        return Process(file, new Dictionary<string, string?>(), new Dictionary<string, object>());
    }

    private static string ProcessHTML(string code){
        return ProcessHTML(code, new Dictionary<string, string?>(), new Dictionary<string, object>());
    }
}