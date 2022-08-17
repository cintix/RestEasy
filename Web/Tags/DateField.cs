using RestEasy.Web.HTML;
namespace RestEasy.Web.Tags {
    public class DateField: Tag {

        public override string startTag(){
            return DateTime.Now.ToLongDateString();
        }
        public override string endTag(){
            return "";
        }
    }
}