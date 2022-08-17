namespace RestEasy.Web.Generator;

public abstract class ModelGenerator {
    public abstract string FromModel(Object model);
    public abstract object? ToModel(string? content, Type type);
    public abstract T? ToModel<T>(string content);
}