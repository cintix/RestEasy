namespace RestEasy.Web.Documentation; 

public class Service {
    private string name = "";
    private string uri = "";

    private readonly List<ServiceAction> methods = new List<ServiceAction>();
    
    public void AddMethod(ServiceAction action) {
        methods.Add(action);
    }

    public string Name{
        get => name;
        set => name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Uri{
        get => uri;
        set => uri = value ?? throw new ArgumentNullException(nameof(value));
    }

    public List<ServiceAction> Methods => methods;
}