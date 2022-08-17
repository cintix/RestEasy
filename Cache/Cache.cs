namespace RestEasy.Cache {
    public class Cache<TKey, TValue> where TKey : notnull {
        private readonly object _cacheLock = new object();

        private readonly long _timeToLive;
        private readonly int _maxItems;
        private readonly Dictionary<TKey, CacheObject> _cacheMap;

        public Cache(int maxItems){
            _timeToLive = -1;
            this._maxItems = maxItems;
            _cacheMap = new Dictionary<TKey, CacheObject>();
        }

        public Cache(long timeToLive){
            if (timeToLive != -1){
                this._timeToLive = timeToLive;
            } else{
                this._timeToLive = -1;
            }

            _maxItems = 10000;
            _cacheMap = new Dictionary<TKey, CacheObject>();
        }

        public Cache(long timeToLive, int maxItems){
            if (timeToLive != -1){
                this._timeToLive = timeToLive;
            } else{
                this._timeToLive = -1;
            }

            this._maxItems = maxItems;
            _cacheMap = new Dictionary<TKey, CacheObject>();
        }

        public List<TValue> All(){
            lock (_cacheLock){
                List<TValue> list = new List<TValue>();
                foreach (CacheObject instance in _cacheMap.Values){
                    list.Add(instance.GetObject());
                }

                return list;
            }
        }

        public void Put(TKey key, TValue value, CacheType type){
            lock (_cacheLock){
                if (_cacheMap.Count == _maxItems){
                    TKey[]? keys = _cacheMap.Keys as TKey[];
                    if (keys != null) _cacheMap.Remove(keys[0]);
                }

                _cacheMap.Add(key, new CacheObject(value, type));
            }
        }

        public TValue? Get(TKey key){
            CacheObject? c = null;
            lock (_cacheLock){
                if (_cacheMap.ContainsKey(key))
                    c = _cacheMap[key];
            }

            CleanUp();
            if (c == null){
                return default;
            } else{
                return c.GetObject();
            }
        }

        public bool Contains(TKey key){
            CleanUp();
            lock (_cacheLock){
                return _cacheMap.ContainsKey(key);
            }
        }

        public void Renew(TKey key){
            lock (_cacheLock){
                CacheObject cacheObject = _cacheMap[key];
                cacheObject.SetCreatedAt(new DateTimeOffset(new DateTime()).ToUnixTimeMilliseconds());
                _cacheMap.Add(key, cacheObject);
            }
        }

        public void Remove(TKey key){
            lock (_cacheLock){
                _cacheMap.Remove(key);
            }
        }

        public int Count(){
            lock (_cacheLock){
                return _cacheMap.Count;
            }
        }

        public long GetCacheTimeInSeconds(TKey key){
            lock (_cacheLock){
                long time = new DateTimeOffset(new DateTime()).ToUnixTimeMilliseconds() - _cacheMap[key].GetCreatedAt();
                return time / 1000;
            }
        }

        public void CleanUp(){
            long now = new DateTimeOffset(new DateTime()).ToUnixTimeMilliseconds();
            List<TKey> deleteKey = new List<TKey>();

            lock (_cacheLock){
                foreach (var _cacheItem in _cacheMap){
                    CacheObject cacheObject = _cacheItem.Value;
                    if (cacheObject.GetCacheType() != CacheType.STATIC && _timeToLive != -1 &&
                        (now > (_timeToLive + cacheObject.GetCreatedAt()))){
                        deleteKey.Add(_cacheItem.Key);
                    }
                }
            }

            lock (_cacheLock){
                foreach (var key in deleteKey){
                    _cacheMap.Remove(key);
                }
            }
        }

        public void Clear(){
            lock (_cacheLock){
                _cacheMap.Clear();
            }
        }

        private class CacheObject {
            private long _createdAt = new DateTimeOffset(new DateTime()).ToUnixTimeMilliseconds();
            private readonly TValue _object;
            private readonly CacheType _type;

            public CacheObject(TValue @object, CacheType type){
                this._object = @object;
                this._type = type;
            }

            public long GetCreatedAt(){
                return _createdAt;
            }

            public void SetCreatedAt(long l){
                _createdAt = l;
            }

            public CacheType GetCacheType(){
                return _type;
            }

            public TValue GetObject(){
                return _object;
            }
        }
    }
}