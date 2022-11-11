using RestEasy.Web.HTML;
namespace RestEasy.Web.Tags {
    public class DateField: Tag {

        public override string StartTag(){
            return DateTime.Now.ToLongDateString();
        }
        public override string EndTag(){
            return "";
        }
    }
}