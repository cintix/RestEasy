namespace RestEasy.Web; 
using Documentation;
public class API {
    private readonly List<Service> services = new List<Service>();
    public void AddService(Service service) { services.Add(service);}

    public List<Service> Services => services;
}