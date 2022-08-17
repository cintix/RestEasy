using RestEasy.Web.HTML;
namespace RestEasy.Web.Tags {
    public class Layout : Tag {
        public override string startTag(){
            string templateStart = File.ReadAllText("templates/layout-start.html");
            return templateStart;
        }
        public override string endTag(){
            string templateEnd = File.ReadAllText("templates/layout-end.html");
            return templateEnd;
        }
    }
}