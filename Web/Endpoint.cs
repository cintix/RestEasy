using System.Reflection;
namespace RestEasy.Web {
    public class Endpoint {
        public readonly string Path;
        public readonly MethodInfo Method;
        public readonly object Object;
        public Endpoint(string path, MethodInfo method, object o){
            Path = path;
            Method = method;
            Object = o;
        }

        public void AddInjection(object obj){
            foreach (FieldInfo field in Object.GetType().GetFields()){
                if (field.GetType() == obj.GetType()){
                    field.SetValue(Object, obj);
                }
            }
        }
        
        public override string ToString(){
            return $"{nameof(Path)}: {Path}, {nameof(Method)}: {Method}, {nameof(Object)}: {Object}";
        }
    }
}