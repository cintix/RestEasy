using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Web;
using RestEasy.Cache;
using RestEasy.Web.Annotations;
using RestEasy.Web.Base;
using RestEasy.Web.Generator;
using Action = RestEasy.Web.Annotations.Action;

namespace RestEasy.Web;

public class RestAction {
    private static readonly Dictionary<string, Cache<string, string>> _CACHE_MAPS = new Dictionary<string, Cache<string, string>>();
    private readonly List<string?> arguments;
    private Endpoint _endpoint;
    private ModelGenerator? generator;

    public RestAction(Endpoint endpoint, List<string?> argument){
        this.arguments = argument;
        _endpoint = endpoint;
    }

    public void AddArgument(string? arg){
        arguments.Add(arg);
    }

    public List<string?> GetArguments(){
        return arguments;
    }

    public Endpoint GetEndpoint(){
        return _endpoint;
    }

    public Response process(Request request){
        try{
            foreach (FieldInfo field in _endpoint.Object.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)){
                if (field.GetCustomAttributes(typeof(Inject), false).Length > 0){
                    if (field.FieldType == typeof(Request)){
                        field.SetValue(_endpoint.Object, request);
                    }
                }
            }

            MethodInfo method = _endpoint.Method;
            ;
            ParameterInfo[] parameterTypes = method.GetParameters();
            object?[] methodArguments = new object?[parameterTypes.Length];

            string cacheBaseId = BaseId(method.ToString() ?? "");
            string requestId = BaseId(methodArguments.ToString() ?? "");

            CacheType cacheType = GetCacheStrategy(method);
            bool useCache = (cacheType != CacheType.NONE);
            Cache<string, string>? cache = null;

            if (_CACHE_MAPS.ContainsKey(cacheBaseId)) cache = _CACHE_MAPS[cacheBaseId];
            if (useCache && cache != null){
                if (cache.Contains(requestId)){
                    CachedResponse cachedResponse = new CachedResponse(Encoding.UTF8.GetBytes(cache.Get(requestId) ?? string.Empty));
                    return cachedResponse;
                }
            }

            object[] objectArguments = method.GetCustomAttributes(typeof(Action), false);
            Action action = (Action)objectArguments[0];
            string accept = action.Consume;

            if (!HttpUtil.ContentTypeMatch(accept, request.ContentType)){
                return new Response().NotFound();
            } else{
                Dictionary<string, ModelGenerator> contextGenerators = Response.Generators;
                if (contextGenerators.ContainsKey(accept)){
                    generator = contextGenerators[accept];
                } else{
                    generator = contextGenerators["default"];
                }
            }

            if (parameterTypes.Length == 1 && arguments.Count > 0 && (request.Method.ToUpper().Equals("POST") || request.Method.ToUpper().Equals("PUT"))){
                ParameterInfo parameter = parameterTypes[0];
                methodArguments[0] = ValueFromType(parameter, request.RawRequest);
            } else{
                for (int index = 0; index < parameterTypes.Length; index++){
                    ParameterInfo parameter = parameterTypes[index];
                    string? value = HttpUtility.UrlDecode(arguments[index]);
                    methodArguments[index] = ValueFromType(parameter, value);
                }
            }

            Response? response = (Response)method.Invoke(_endpoint.Object, methodArguments)!;
            if (useCache && response != null){
                if (cache == null){
                    CacheByStatus? cacheByStatus = (CacheByStatus)method.GetCustomAttributes(typeof(CacheByStatus), false)[0];
                    string cachedResponseString = Convert.ToBase64String(response.Build());

                    if (cacheByStatus == null){
                        cache = new Cache<string, string>(1);
                    } else{
                        int currentStatusCode = (int)response.GetStatus();
                        if (cacheByStatus.value != null)
                            foreach (Annotations.Cache cacheOptions in cacheByStatus.value){
                                if (IsStatusDefinedInCache(cacheOptions.Status, currentStatusCode)){
                                    cache = new Cache<string, string>(cacheOptions.TimeToLive, cacheOptions.Size);
                                    cache.Put(requestId, cachedResponseString, cacheType);
                                }
                            }
                    }
                    cache?.Put(requestId, cachedResponseString, cacheType);
                }

                if (cache != null){
                    if (_CACHE_MAPS.ContainsKey(cacheBaseId)) _CACHE_MAPS[cacheBaseId] = cache;
                    else _CACHE_MAPS.Add(cacheBaseId, cache);
                }
            }

            return response ?? new Response().InternalServerError().Data("Invalid response object");
        } catch (Exception exception){
            return new Response().InternalServerError().Data(exception.ToString());
        }
    }


    private bool IsStatusDefinedInCache(Status[] status, int value){
        for (int i = 0; i < status.Length; i++){
            if ((status[i] == Status.All)){
                return true;
            }

            if (value == ((int)status[i])){
                return true;
            }
        }

        return false;
    }

    private string BaseId(String name){
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
    }

    private object? ValueFromType(ParameterInfo param, string? value){
        Type valueFromType = param.ParameterType;
        if (ReferenceEquals(valueFromType, typeof(string)) ||
            ReferenceEquals(valueFromType, typeof(byte)) ||
            ReferenceEquals(valueFromType, typeof(double)) ||
            ReferenceEquals(valueFromType, typeof(float)) ||
            ReferenceEquals(valueFromType, typeof(long)) ||
            ReferenceEquals(valueFromType, typeof(short)) ||
            ReferenceEquals(valueFromType, typeof(bool)) ||
            ReferenceEquals(valueFromType, typeof(int))){
            return Convert.ChangeType(value, valueFromType);
        }

        if (value != null) return generator?.ToModel(value, param.GetType());
        return null;
    }

    private CacheType GetCacheStrategy(MethodInfo method){
        if (method.GetCustomAttributes(typeof(Static), false).Length > 0){
            return CacheType.STATIC;
        }

        if (method.GetCustomAttributes(typeof(Web.Annotations.Cache), false).Length > 0){
            return CacheType.DYNAMIC;
        }

        return CacheType.NONE;
    }
}