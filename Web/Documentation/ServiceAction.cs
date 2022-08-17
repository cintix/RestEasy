namespace RestEasy.Web.Documentation; 

public class ServiceAction {
    public enum ActionType {
        GET, POST, PUT, DELETE
    };

    private string action = "GET";
    private string uri="";
    private string accepts = "*/*";

    private readonly List<Cache> caching = new List<Cache>();
    private readonly List<ArgumentDefinition> arguments = new List<ArgumentDefinition>();
    
    public void AddArgument(ArgumentDefinition action) {
        arguments.Add(action);
    }
    
    public void AddCache(Cache cache) {
        caching.Add(cache);
    }

    public string Action => action;

    public void SetAction(ActionType type){
        switch (type){
            case ActionType.GET : action = "GET";
                break;
            case ActionType.PUT : action = "PUT";
                break;
            case ActionType.POST : action = "POST";
                break;
            case ActionType.DELETE : action = "DELETE";
                break;
        }
    }
    
    public string Uri{
        get => uri;
        set => uri = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Accepts{
        get => accepts;
        set => accepts = value ?? throw new ArgumentNullException(nameof(value));
    }

    public List<Cache> Caching => caching;

    public List<ArgumentDefinition> Arguments => arguments;
}