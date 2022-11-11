namespace RestEasy.Web.HTML.Tags;

public class Value : Tag {
    public override string StartTag(){
        string? propertyName = GetProperty("name");
        if (propertyName !=null && propertyName.StartsWith("@")){
            return GetProperty(propertyName) ?? "";
        }
        return GetProperty("name") ?? "";
    }

    public override string EndTag(){
        return "";
    }
}