using RestEasy.Web.Base;

namespace RestEasy.Web.Annotations {
    public class Cache : Attribute {
        public readonly long TimeToLive;
        public readonly int Size;
        public RestEasy.Web.Base.Status[] Status;

        public Cache(long timeToLive = -1, int size = 1000, Status[]? status = null){
            TimeToLive = timeToLive;
            Size = size;
            Status = status ?? new[]{ Base.Status.All };
        }
    }
}