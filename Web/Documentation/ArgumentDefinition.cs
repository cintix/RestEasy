namespace RestEasy.Web.Documentation; 

public class ArgumentDefinition {
    private string name="";
    private string type="";
    private ModelDefinition? model = null;
    
    public ArgumentDefinition() {}
    public ArgumentDefinition(string name, string type){
        this.name = name;
        this.type = type;
    }

    public string Name{
        get => name;
        set => name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Type{
        get => type;
        set => type = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ModelDefinition? Model{
        get => model;
        set => model = value ?? throw new ArgumentNullException(nameof(value));
    }
}