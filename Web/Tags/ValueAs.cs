using RestEasy.Web.HTML;

namespace Application.Web.Tags;

public class ValueAs : Tag {
    public override string startTag() {
        string property = this.GetProperty("name");
        string value = (property != null && property.StartsWith("@")) ? this.GetProperty(property) ?? "" : this.GetProperty("name") ?? "";

        string? tag = GetProperty("data-tag").ToLower();
        string response = "<" + tag + " ";

        string? style = GetProperty("style");
        if (!string.IsNullOrEmpty(style)) {
            response += "style=\"" + style + "\"";
        }

        string? classAtt = GetProperty("class");
        if (!string.IsNullOrEmpty(classAtt)) {
            response += "class=\"" + classAtt + "\"";
        }

        string? att = GetProperty("data-attribute");
        if (!string.IsNullOrEmpty(att)) {
             response += " " + att + "=\"" + value + "\"";
        }
        
        if (!tag.Equals("img")) response += ">";

        if (string.IsNullOrEmpty(att)) {
            response += value;
        }
        return response;
    }

    public override string endTag() {
        string? tag = GetProperty("data-tag").ToLower();
        if (tag.Equals("img")) return "/>";
        else return "</" + tag + ">";
    }
}