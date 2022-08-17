namespace RestEasy.Web.Annotations {
    public class CacheByStatus : Attribute {
        public readonly Cache[]? value;

        public CacheByStatus(Cache[]? value = null){
            this.value = value;
        }
    }
}