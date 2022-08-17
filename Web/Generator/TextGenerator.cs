namespace RestEasy.Web.Generator;

public class TextGenerator : ModelGenerator {
    public override string FromModel(object model){
        return model.ToString() ?? string.Empty;
    }

    public override T? ToModel<T>(string content) where T : default{
        return (T)(object)content;
    }

    public override object? ToModel(string? content, Type type){
        return content;
    }
}