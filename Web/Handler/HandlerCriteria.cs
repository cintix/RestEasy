namespace RestEasy.Web.Handler;

public class HandlerCriteria {

    public HandlerCriteria(params Type[] types) {
        Types = types;
    }
    public Type[] Types { get; set; }
}