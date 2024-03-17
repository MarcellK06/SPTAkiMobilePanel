using System.Net;
using CleanProject.Pages;
using ComponentAce.Compression.Libs.zlib;
namespace CleanProject
{
    public partial class MainPage : ContentPage
    {

        public Dictionary<string, string> loginData = new Dictionary<string, string> { { "username", "" }, { "email", "" }, { "edition", "Edge Of Darkness" }, { "password", "" }, { "backendUrl", "" } };

        public MainPage()
        {
            InitializeComponent();
        }

        public string sessionID;
        private void Login()
        {

            loginData["username"] = username.Text;
            loginData["email"] = username.Text;
            loginData["password"] = password.Text;
            loginData["backendUrl"] = address.Text;

            byte[] bytes = SimpleZlib.CompressToBytes(System.Text.Json.JsonSerializer.Serialize(loginData), zlibConst.Z_BEST_SPEED);
            WebRequest client = WebRequest.Create(new Uri(loginData["backendUrl"] + "/launcher/profile/login"));
            client.Headers.Add("Cookie", $"PHPSESSID={null}");
            client.Headers.Add("SessionId", null);
            client.Headers.Add("Accept-Encoding", "deflate, gzip");
            client.ContentType = "application/json";
            client.ContentLength = bytes.Length;
            client.Method = "POST";
            client.Timeout = 5000;
            client.Headers.Add("content-encoding", "deflate");
            using (Stream stream = client.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            string contentdata = "";
            WebResponse res = client.GetResponse();
            using (MemoryStream ms = new MemoryStream())
            {
                res.GetResponseStream().CopyTo(ms);
                contentdata = SimpleZlib.Decompress(ms.ToArray(), null);
            }
            if (contentdata == "FAILED" || contentdata == "INVALID_PASSWORD")
            {
                DisplayAlert("FAILED", $"Login Failed: {contentdata}", "OK");
                return;
            }
            sessionID = contentdata;
            App.Current.MainPage = new NavigationPage(new ProfilePage(loginData, sessionID));
        }
        private void onLoginPressed(object sender, EventArgs e)
        {
            Login();
        }
    }

}
