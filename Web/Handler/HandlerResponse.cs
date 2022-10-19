namespace RestEasy.Web.Handler;

public class HandlerResponse {
    private bool _status;

    public HandlerResponse(Request request) {
        Request = request;
    }

    public Request Request { get; }

    public bool GetStatus() {
        return _status;
    }

    public HandlerResponse Status(bool status) {
        _status = status;
        return this;
    }
}