namespace RestEasy.Web.HTML;

public abstract class Tag {
    private readonly Dictionary<string, string?> _properties = new Dictionary<string, string?>();
    private readonly Dictionary<string, object> _resources = new Dictionary<string, object>();

    public void AddProperties(Dictionary<string, string?> map){
        foreach (var property in map){
            _properties.Add(property.Key, property.Value);
        }
    }

    public void AddResource(Dictionary<string, object> map){
        foreach (var property in map){
            _resources.Add(property.Key, property.Value);
        }
    }

    public abstract string StartTag();
    public abstract string EndTag();

    protected T? GetResource<T>(){
        Type instanceType = typeof(T);
        if (_resources.ContainsKey(instanceType.Name)){
            return (T?)_resources[instanceType.Name];
        }

        return default;
    }

    protected string GetProperty(string key){
        if (_properties.ContainsKey(key)){
            return _properties[key] ?? string.Empty;
        }
        return "";
    }
}