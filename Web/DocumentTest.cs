using System.Text;
using Application.Web.Tags;
using RestEasy.Web.HTML;

namespace RestEasy.Web;

public class DocumentTest {

    public DocumentTest() {
        Engine.SetNamespace("demo");
        Engine.Add("value-as", typeof(ValueAs));
        string filedata = @"
                <html>
                   <body>
                       <span><demo:value-as data-tag='img' data-attribute='src' name='@image' style='border: solid 1px #000;' /></span>
                   </body>
                </html>";
        File.WriteAllText("imagedoc.htm", filedata.Trim());
        
        Request request = new Request(null, "cintix.dk", new byte[] { 0 });
        string response = Encoding.UTF8.GetString(new Response().OK()
            .Variable("image", "http://www.image.com/image.png")
            .Document(request, "imagedoc.htm").Build());
        File.Delete("imagedoc.htm");
        Console.WriteLine(("doc: " + response));


    }
    
    public static void Main(string[] args) {
        new DocumentTest();
    }
    
}