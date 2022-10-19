namespace RestEasy.Web.Injection;

public class HandlerResponse {
    private readonly Request _request;
    private bool _status;

    public Request Request {
        get => _request;
    }

    public bool GetStatus() {
        return _status;
    }

    public HandlerResponse(Request request) {
        _request = request;
    }

    public HandlerResponse Status(bool status) {
        _status = status;
        return this;
    }

}