using ComponentAce.Compression.Libs.zlib;
using Newtonsoft.Json;
using System.Net;

namespace CleanProject.Pages;

public partial class ProfilePage : ContentPage
{
    struct skill {
        public string skillname { get; set; }
        public int skillevel { get; set; }
        public string skillxp { get; set; }
    }

	Dictionary<string, string> loginData;
    Dictionary<int, int> totalXpPerLevel = new Dictionary<int, int>
    {
        { 1, 0 },
        { 2, 1_000 },
        { 3, 4_017 },
        { 4, 8_432 },
        { 5, 14_245 },
        { 6, 21_477 },
        { 7, 30_023 },
        { 8, 39_936 },
        { 9, 51_204 },
        { 10, 63_723 },
        { 11, 77_563 },
        { 12, 92_713 },
        { 13, 111_881 },
        { 14, 134_674 },
        { 15, 161_139 },
        { 16, 191_417 },
        { 17, 225_194 },
        { 18, 262_366 },
        { 19, 302_484 },
        { 20, 345_751 },
        { 21, 391_649 },
        { 22, 440_444 },
        { 23, 492_366 },
        { 24, 547_896 },
        { 25, 609_066 },
        { 26, 679_255 },
        { 27, 755_444 },
        { 28, 837_672 },
        { 29, 925_976 },
        { 30, 1_020_396 },
        { 31, 1_120_969 },
        { 32, 1_227_735 },
        { 33, 1_344_260 },
        { 34, 1_470_605 },
        { 35, 1_606_833 },
        { 36, 1_759_965 },
        { 37, 1_923_579 },
        { 38, 2_097_740 },
        { 39, 2_282_513 },
        { 40, 2_477_961 },
        { 41, 2_684_149 },
        { 42, 2_901_143 },
        { 43, 3_123_824 },
        { 44, 3_379_281 },
        { 45, 3_640_603 },
        { 46, 3_929_436 },
        { 47, 4_233_995 },
        { 48, 4_554_372 },
        { 49, 4_890_662 },
        { 50, 5_242_956 },
        { 51, 5_611_348 },
        { 52, 5_995_931 },
        { 53, 6_402_287 },
        { 54, 6_830_542 },
        { 55, 7_280_825 },
        { 56, 7_753_260 },
        { 57, 8_247_975 },
        { 58, 8_765_097 },
        { 59, 9_304_752 },
        { 60, 9_876_880 },
        { 61, 10_512_365 },
        { 62, 11_193_911 },
        { 63, 11_929_835 },
        { 64, 12_727_177 },
        { 65, 13_615_989 },
        { 66, 14_626_588 },
        { 67, 15_864_243 },
        { 68, 17_555_001 },
        { 69, 19_926_895 },
        { 70, 22_926_895 },
        { 71, 26_526_895 },
        { 72, 30_726_895 },
        { 73, 35_526_895 },
        { 74, 40_926_895 },
        { 75, 46_926_895 },
        { 76, 53_526_895 },
        { 77, 60_726_895 },
        { 78, 69_126_895 },
        { 79, 81_126_895 },
    };
    List<skill> skillist = new List<skill>();
    string sessionID;
    Page cachedPage;

    public ProfilePage(Dictionary<string, string> loginData, string sessionID)
	{
		InitializeComponent();
		this.loginData = loginData;
		this.sessionID = sessionID;
        getProfileData();
	}

	async void getProfileData()
	{

        byte[] bytes = SimpleZlib.CompressToBytes(System.Text.Json.JsonSerializer.Serialize(loginData), zlibConst.Z_BEST_SPEED);

        Dictionary<string, string> getProfile = new Dictionary<string, string>
        {
                { "url", loginData["backendUrl"]},
                { "info", null },
                { "sessionID", sessionID }
        };

        WebRequest client = WebRequest.Create(new Uri(loginData["backendUrl"] + "/client/game/profile/list"));
        client.Headers.Add("Cookie", $"PHPSESSID={sessionID}");
        client.Headers.Add("sessionID", sessionID);
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
        dynamic deserialized = JsonConvert.DeserializeObject(contentdata);
        dynamic username = deserialized.data[0].Info.Nickname;
        string level = deserialized.data[0].Info.Level;
        dynamic side = deserialized.data[0].Info.Side;
        string experience = deserialized.data[0].Info.Experience;
        dynamic skills = deserialized.data[0].Skills.Common;
        foreach(var v_skill in skills)
        {
            string skillname = v_skill.Id;
            if (skillname != "BotReload" && skillname != "BotSound")
            {

                skill newskill = new skill();
                string skillxp = v_skill.Progress;
                int skillxpinteger = int.Parse(skillxp.Split('.')[0]);
                newskill.skillname = skillname;
                newskill.skillevel = (int)Math.Floor((float)skillxpinteger / 100);
                newskill.skillxp = $"{(int)Math.Floor((float)skillxpinteger % 100)}/100";
                skillist.Add(newskill);
            }
        }
        Username.Text = username;
        Side.Text = side;
        Level.Text = level;
        Experience.Text = $"{experience} / {totalXpPerLevel[int.Parse(level)+1]}";
        skillsview.ItemsSource = skillist;
    }

    private void GoBack(object sender, EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(new MainPage());
    }
    private void openQuests(object sender, EventArgs e)
    {

        App.Current.MainPage = new NavigationPage(new QuestsPage(loginData, sessionID, this));
    }
}