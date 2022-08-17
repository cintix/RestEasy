namespace RestEasy.Web.HTML;

public class Value : Tag {
    public override string startTag(){
        string? propertyName = GetProperty("name");
        if (propertyName !=null && propertyName.StartsWith("@")){
            return GetProperty(propertyName) ?? "";
        }
        return GetProperty("name") ?? "";
    }

    public override string endTag(){
        return "";
    }
}