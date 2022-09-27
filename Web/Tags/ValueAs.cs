using RestEasy.Web.HTML;

namespace Application.Web.Tags;

public class ValueAs : Tag {
    public override string startTag() {
        Console.WriteLine("ValueAs running");
        string property = this.GetProperty("name");
        string value = (property != null && property.StartsWith("@")) ? this.GetProperty(property) ?? "" : this.GetProperty("name") ?? "";

        string? tag = GetProperty("data-tag").ToLower();
        string response = "<" + tag + " ";
        Console.WriteLine("response: " + response);

        string? style = GetProperty("style");
        if (!string.IsNullOrEmpty(style)) {
            response += "style=\"" + style + "\"";
        }
        Console.WriteLine("response: " + response);

        string? classAtt = GetProperty("class");
        if (!string.IsNullOrEmpty(classAtt)) {
            response += "class=\"" + classAtt + "\"";
        }
        Console.WriteLine("response: " + response);

        string? att = GetProperty("data-attribute");
        if (!string.IsNullOrEmpty(att)) {
             response += " " + att + "=\"" + value + "\"";
        }
        Console.WriteLine("response: " + response);
        
        if (!tag.Equals("img")) response += ">";
        Console.WriteLine("response: " + response);

        if (string.IsNullOrEmpty(att)) {
            response += value;
        }
        Console.WriteLine("response: " + response);
        return response;
    }

    public override string endTag() {
        string? tag = GetProperty("data-tag").ToLower();
        if (tag.Equals("img")) return "/>";
        else return "</" + tag + ">";
    }
}