using System.Text.RegularExpressions;
namespace RestEasy.Web; 

public static class HttpUtil {
    public static bool ContentTypeMatch(string accept, string contentType) {
        string patternString = "^" + accept.Replace("\\*", "\\\\S+").Replace("/", "\\/");
        Regex regex = new Regex(patternString);
        MatchCollection collection = regex.Matches(contentType);
        return collection.Count > 0;
    }

}