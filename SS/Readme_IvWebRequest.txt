//==== IvWebRequest ====
//Performs web request and use an IvSign certificate as Client Certificate on Authentication is Required

//With the same instance, Cookies are stored and can be retrieved from:


//=== Instantiate ===
//To instantiate WebClient, you can do it WITH or WITHOUT IvSign Object 
//WebClient that requires Login in IvSign
var WebClient = new IvWebRequest(new IvSignInit() { server = "ivnosys.net", userid = "user1", pass = "password1" });
//or web client that use existing Token
var WebClient = new IvWebRequest(new IvSignInit() { server = "ivsign.net", token = "KpzkBdM92nujnG5fQLNfkYsbVuMG95R/NppD85v65XcCgl6xw51wRYzoJW1AJhQ5s" });
//or use basic WebClient without using IvSign
var WebClient = new IvWebRequest();

//After instanced, the "error" string can be set if some error occurs (!string.IsNullOrWhiteSpace(WebClient.error))

//The CertObject needs at least Certid and pin
var cert = new CertObj()  { certid = "8F8F8F8F8F8F", pin = "123456"};
//But its better performance if you call it with public cert
var cert = new CertObj()  { certid = "8F8F8F8F8F8F", pin = "123456", pubCer = Convert.FromBase64String("Get from base64 or directly from x509") };

//=== Disposing ===
//It is recommended to dispose the instance when not necesary

//=== Headers ===
//The headers Dictionary can be null or filled in this way:
var header = new Dictionary<string, string>() { { "Authentication", "Authentication value" }, { "User-Agent", "Your-User Agent" } };
//even, you can add values conditionally
if (x != y && !header.Contains("name") ) header.Add("name","value");

//=== WebForm POST parameters ===
//The formdata Dictionary can be null or filled in this way:
var data = new Dictionary<string, string>() { { "Key1", "Value1" }, { "Key2", "Value2" } };
//even, you can add values conditionally
if (x != y) data.Add("Conditionalkey","ConditionalValue");


//=== Methods ===
//For direct GET method, only 3 parameters are allowed: 
get(url [, CertObj] [, headers]); last two are optional

//There are 2 direct POST methods:
 //POST body or basic webform 
 byte[] post(string url, CertObj in_cert = null, Dictionary<string, string> headers = null, Dictionary<string, string> formdata = null, string body = null)
 
 //POST extended webform with file uploading
 public byte[] postwebform(string url, CertObj in_cert = null, Dictionary<string, string> headers = null, Dictionary<string, string> formdata = null, string FileField = "", string FileName = "", Stream FileStream = null, string ContentType = "")

 //There are a generic method to perform requests:
 public byte[] request(string url, string method, CertObj in_cert = null, Dictionary<string, string> headers = null, Dictionary<string, string> formdata = null, string body = null, string FileField = "", string FileName = "", Stream FileStream = null, string ContentType = "")

// === Cookies ===
// Cookies are stored for each domain in class IvWebRequest.
// To get list of domains with cookies use:
 List<string> Get_Cookies_Domains(); 
// This function returns a list of domains. To get cookies for each domain, use: 
List<Cookie> Get_Cookies(string url);
//This function returns a Cookie Object for specific Url

 /* C# EXAMPLES */
//WITHOUT CLIENT CERTIFICATE AUTHENTICATION
//Basic GET usage overriding header 
var web1 = new IvWebRequest();
if (!string.IsNullOrWhiteSpace(web1.error))
	Console.WriteLine(web1.error);
var resp1 = web1.get("https://ivsign.net/Keyman/rest/v2/cert/list?request.cert.userid=pruiz", null, new Dictionary<string, string>() { { "Authentication", "K9kYbxu8aiR697W1IZLq1CO9AXmYwr/UdLq1MaueeDdm0IJAUoj/vcHOWyVhgztqm" }, { "User-Agent", "Your-User Agent" } });
if (!string.IsNullOrWhiteSpace(web1.error))
	Console.WriteLine(web1.error);
if (resp1 != null)
	Console.WriteLine(Encoding.UTF8.GetString(resp1));
web1.close();

//Basic POST usage adding new header
var web11 = new IvWebRequest();
if (!string.IsNullOrWhiteSpace(web11.error))
	Console.WriteLine(web11.error);
var resp11 = web11.post("https://ivsign.net/Keyman/rest/v4/user/get", null, new Dictionary<string, string>() { { "Authentication", "K9kYbxu8aiR697W1IZLq1CO9AXmYwr/UdLq1MaueeDdm0IJAUoj/vcHOWyVhgztqm" }, { "Content-Type", "application/json" } }, null, "{\"user\":{\"userid\":\"ktest\"}}");
if (!string.IsNullOrWhiteSpace(web11.error))
	Console.WriteLine(web11.error);
if (resp11 != null)
	Console.WriteLine(Encoding.UTF8.GetString(resp11));
web11.close();

//POST page with form values
var web4 = new IvWebRequest();
if (!string.IsNullOrWhiteSpace(web4.error))
	Console.WriteLine(web4.error);
var resp4 = web4.post("http://127.0.0.1/prueba.php", null, new Dictionary<string, string>() { { "Authentication", "K9kYbxu8aiR697W1IZLq1CO9AXmYwr/UdLq1MaueeDdm0IJAUoj/vcHOWyVhgztqm" } }, new Dictionary<string, string>() { { "key1", "value1" }, { "key2", "value2" } });
if (!string.IsNullOrWhiteSpace(web4.error))
	Console.WriteLine(web4.error);
if (resp4 != null)
	Console.WriteLine(Encoding.UTF8.GetString(resp4));
web4.close();

			
//WITH CLIENT CERTIFICATE AUTHENTICATION
//GET webpage with IvSign Certificate and Application token
var ivsign2 = new IvSignInit() { server = "ivsign.net", token = "KpzkBdM92nujnG5fQLNfkYsbVuMG95R/NppD85v65XcCgl6xw51wRYzoJW1AJhQ5s" };
var web2 = new IvWebRequest(ivsign2);
if (!string.IsNullOrWhiteSpace(web2.error))
	Console.WriteLine(web2.error);
var resp2 = web2.get("https://tools.ivnosys.net/cert/check", new CertObj() { certid = "8F8F8F8F8F8F", pin = "123456", pubCer = Convert.FromBase64String("Get from base64 or directly from x509") });
if (!string.IsNullOrWhiteSpace(web2.error))
	Console.WriteLine(web2.error);
if (resp2 != null)
	Console.WriteLine(Encoding.UTF8.GetString(resp2));
web2.close();

//GET page with IvSign Certificate and Login with User/password
var ivsign3 = new IvSignInit() { server = "ivnosys.net", userid = "user1", pass = "pass1" };
var web3 = new IvWebRequest(ivsign3);
if (!string.IsNullOrWhiteSpace(web3.error))
	Console.WriteLine(web3.error);
var cert = new CertObj()  { certid = "8F8F8F8F8F8F", pin = "123456", pubCer = Convert.FromBase64String("Get from base64 or directly from x509") };
var resp3 = web3.get("https://tools.ivnosys.net/cert/check", cert);
if (!string.IsNullOrWhiteSpace(web3.error))
	Console.WriteLine(web3.error);
if (resp3 != null)
	Console.WriteLine(Encoding.UTF8.GetString(resp3));
//Al reutilizar la instancia, se puede hacer otra llamada sin volver a pasarle los datos del certificado
var resp31 = web3.get("https://apsc.accv.es/utiles/frontal/comprobacionaccv/comprobacionaccv.htm");
if (!string.IsNullOrWhiteSpace(web3.error))
	Console.WriteLine(web3.error);
if (resp31 != null)
	Console.WriteLine(Encoding.UTF8.GetString(resp31));
web3.close();


			