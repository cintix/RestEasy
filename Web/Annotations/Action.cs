namespace RestEasy.Web.Annotations {
    public class Action : Attribute {
        public readonly string Path;
        public readonly string Consume;

        public Action(string path = "/", string consume = "*/*"){
            Path = path;
            Consume = consume;
        }
    }
}