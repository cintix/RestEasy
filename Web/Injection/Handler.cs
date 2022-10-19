namespace RestEasy.Web.Injection;

public interface Handler {
    public HandlerResponse OnRequested(Request request);
}