namespace RestEasy.Web.Injection;

public class HandlerCriteria {
    public Type[] Types { get; set; }

    public HandlerCriteria(params Type[] types) {
        Types = types;
    }
}