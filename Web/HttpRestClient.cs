using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestEasy.Web; 

public class HttpRestClient {
    private  readonly HttpClient _client = new HttpClient();
    private int Status = -1;
    public void AddHeader(string key, string value){
        _client.DefaultRequestHeaders.Add(key,value);
    }

    public string Get(string uri){
        HttpResponseMessage result = _client.Send(new HttpRequestMessage(HttpMethod.Get,uri));
        Status = (int) result.StatusCode;
        Task<string> readAsStringAsync = result.Content.ReadAsStringAsync();
        return readAsStringAsync.Result;
    }

    public T? Get<T>(string uri){
        string json = Get(uri);
        DataContractJsonSerializer contractJsonSerializer = new DataContractJsonSerializer(typeof(T));
        using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return (T?)contractJsonSerializer.ReadObject(ms);
    }
}