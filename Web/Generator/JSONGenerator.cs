using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

namespace RestEasy.Web.Generator;

public class JSONGenerator : ModelGenerator {
    public override string FromModel(object model){
        return JsonSerializer.Serialize(model);
    }

    public override T? ToModel<T>(string content) where T : default{
        DataContractJsonSerializer contractJsonSerializer = new DataContractJsonSerializer(typeof(T));
        using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return (T?)contractJsonSerializer.ReadObject(ms);
    }

    public override object? ToModel(string? content, Type type){
        if (content == null) return content;
        DataContractJsonSerializer contractJsonSerializer = new DataContractJsonSerializer(type);
        using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return contractJsonSerializer.ReadObject(ms);
    }
    
}