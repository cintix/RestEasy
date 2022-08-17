namespace RestEasy.Web; 

public class CachedResponse : Response {
    private readonly byte[] _data;
    public CachedResponse(byte[]? data){
        if (data != null)
            _data = data;
        else _data = Array.Empty<byte>();
    }

    public new byte[]? Build() {
        return _data;
    }
}