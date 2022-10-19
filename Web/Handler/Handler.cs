namespace RestEasy.Web.Handler;

public interface Handler {
    public HandlerResponse OnRequested(Request request);
}