using System.Reflection;
using RestEasy.Web.Annotations;
using RestEasy.Web.Base;
using Action = RestEasy.Web.Annotations.Action;

namespace RestEasy.Web.Documentation;

public static class JsonServiceDescriptionEngine {
    public static Service GenerateServiceDefinition(string uri, string? name, object endpointObject){
        Type endpoint = endpointObject.GetType();

        Service service = new Service();
        service.Name = (name != null) ? name : endpoint.Name;
        service.Uri = uri;

        MethodInfo[] methods = endpoint.GetMethods();
        foreach (MethodInfo methodOriginal in methods){
            MethodInfo method = methodOriginal;
            if (method.GetCustomAttributes(typeof(Action), false).Length > 0){
                object[] classAttributes = method.GetCustomAttributes(typeof(Action), false);
                Action action = (Action)classAttributes[0];

                ServiceAction serviceAction = new ServiceAction();
                serviceAction.Uri = (action.Path);
                serviceAction.SetAction (ServiceAction.ActionType.GET);
                serviceAction.Accepts = action.Consume;

                if (method.GetCustomAttributes(typeof(PUT), false).Length > 0){
                    serviceAction.SetAction(ServiceAction.ActionType.PUT);
                } else if (method.GetCustomAttributes(typeof(POST), false).Length > 0){
                    serviceAction.SetAction(ServiceAction.ActionType.POST);
                } else if (method.GetCustomAttributes(typeof(DELETE), false).Length > 0){
                    serviceAction.SetAction(ServiceAction.ActionType.DELETE);
                }

                foreach (ParameterInfo parameter in method.GetParameters()){
                    if (parameter.Name != null){
                        ArgumentDefinition? definition = GetArgumentDefinition(parameter.Name, parameter.ParameterType);
                        if (definition != null)
                            serviceAction.AddArgument(definition);
                    }
                }

                if (method.GetCustomAttributes(typeof(Static), false).Length > 0){
                    Cache cache = new Cache();
                    cache.Description = ("Static");
                    cache.TimeToLive = (-1);
                    cache.Status = (Status.All);
                    serviceAction.AddCache(cache);
                }

                if (method.GetCustomAttributes(typeof(CacheByStatus), false).Length > 0){
                    object[] classArguments = method.GetCustomAttributes(typeof(CacheByStatus), false);
                    Console.WriteLine(classArguments);
                    RestEasy.Web.Annotations.Cache[] caches = (Annotations.Cache[])classArguments[0];
                    foreach (RestEasy.Web.Annotations.Cache cache in caches){
                        foreach (Status stat in cache.Status){
                            Cache c = new Cache();
                            c.Description = ("Cache for " + ((stat == Status.All) ? " all statuses" : stat.ToString()));
                            c.TimeToLive = (cache.TimeToLive);
                            c.Status = (stat);
                            serviceAction.AddCache(c);
                        }
                    }
                }

                service.AddMethod(serviceAction);
            }
        }

        return service;
    }

    private static ArgumentDefinition? GetArgumentDefinition(string name, Type obj){
        ArgumentDefinition definition = new ArgumentDefinition();
        definition.Name = (name);

        if (ReferenceEquals(obj, typeof(string)) ||
            ReferenceEquals(obj, typeof(byte)) ||
            ReferenceEquals(obj, typeof(double)) ||
            ReferenceEquals(obj, typeof(float)) ||
            ReferenceEquals(obj, typeof(long)) ||
            ReferenceEquals(obj, typeof(short)) ||
            ReferenceEquals(obj, typeof(bool)) ||
            ReferenceEquals(obj, typeof(int))){
            definition.Type = (obj.Name.ToLower());
            return definition;
        }

        try{
            FieldInfo[] declaredFields = obj.GetFields();
            definition.Type = ("object");
            ModelDefinition? modelDefinition = new ModelDefinition();
            foreach (FieldInfo field in declaredFields){
                ArgumentDefinition? argumentDefinition = GetArgumentDefinition(field.Name, field.FieldType);
                if (argumentDefinition != null)
                    modelDefinition.fields.Add(argumentDefinition);
            }

            definition.Model = (modelDefinition);
            return definition;
        } catch (Exception ex){
            Console.WriteLine(ex);
        }

        return default;
    }
}