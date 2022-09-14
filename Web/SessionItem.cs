namespace RestEasy.Web {
    public class SessionItem {
        private object? sessionObject;
        private DateTime createdAt = DateTime.Now;
        public SessionItem(object? sessionObject){
            this.sessionObject = sessionObject;
        }

        public object? SessionObject
        {
            get => sessionObject;
            set => sessionObject = value ?? throw new ArgumentNullException(nameof(value));
        }

        public DateTime CreatedAt
        {
            get => createdAt;
            set => createdAt = value;
        }

        public override string ToString(){
            return $"{nameof(SessionObject)}: {SessionObject}, {nameof(CreatedAt)}: {CreatedAt}";
        }
    }
}