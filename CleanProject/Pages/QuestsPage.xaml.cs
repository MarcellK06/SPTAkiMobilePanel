using ComponentAce.Compression.Libs.zlib;
using System.Net;
using Newtonsoft.Json;
namespace CleanProject.Pages;

public partial class QuestsPage : ContentPage
{
    struct questData
    {
        public int questtypeid { get; set; }
        public string questimage { get; set; }
        public string questname { get; set; }
        public string questcolor { get; set; }
    }

    struct questCondition
    {
        public string conditiontype { get; set; }
        public string conditiontarget { get; set; }
        public int curprogress { get; set; }
        public int goalprogress { get; set; }
        public bool conditiondone { get; set; }
    }

    class quest: List<questCondition>
    {
        public questData questdata { get; set; }
        public string questconditions { get; set; }
        public quest(questData questdata, List<questCondition> questconditions): base(questconditions)
        {
            this.questdata = questdata;
            var o = "";
            foreach(var condition in questconditions) {
                o += $"{condition.conditiontype} - {condition.conditiontarget} | {condition.curprogress}/{condition.goalprogress}\n";
            }
            this.questconditions = o;
        }
    }

    struct questType
    {
        public string questtype { get; set; }
    }

    List<quest> userquests = new List<quest>();
    List<int> selectedtypes = new List<int>();
    List<questType> qtypes = new List<questType>();
    Dictionary<int, string> questStatus = new Dictionary<int, string> {
            {0, "LOCKED"},
            {1, "AVAILABLE"},
            {2, "STARTED"},
            {3, "FINISH"},
            {4, "COMPLETED" }
        };
    private void onPageLoad(object sender, EventArgs e)
    {

        for (int k = 0; k < questStatus.Values.Count; k++)
        {
            questType type = new questType();
            type.questtype = questStatus[k];
            qtypes.Add(type);
        }
        TestCollectionView.ItemsSource = qtypes;
    }
    Dictionary<string, string> loginData;
    string sessionID = "";
    Page previousPage;
    public QuestsPage(Dictionary<string, string> loginData, string sessionID, Page previousPage)
	{
		InitializeComponent();
        Loaded += onPageLoad;
        this.sessionID = sessionID;
        this.loginData = loginData;
        this.previousPage = previousPage;
        getQuests();
    }
    private void changeSelection(object sender, EventArgs e)
    {
        selectedtypes.Clear();
        questType v_type;
        foreach (var item in TestCollectionView.SelectedItems)
        {
            v_type = (questType)item;
            selectedtypes.Add(questStatus.FirstOrDefault(i => i.Value == v_type.questtype).Key);
        }
        questview.ItemsSource = userquests.Where(i => selectedtypes.Contains(i.questdata.questtypeid));
    }


    private void getQuests()
    {
        byte[] bytes = SimpleZlib.CompressToBytes(System.Text.Json.JsonSerializer.Serialize(loginData), zlibConst.Z_BEST_SPEED);
        userquests.Clear();
        Dictionary<string, string> getQuests = new Dictionary<string, string> {
                { "url", loginData["backendUrl"]},
                { "info", null },
                { "sessionID", sessionID }
            };
        bytes = SimpleZlib.CompressToBytes(System.Text.Json.JsonSerializer.Serialize(getQuests), zlibConst.Z_BEST_SPEED);
        WebRequest client = WebRequest.Create(new Uri(loginData["backendUrl"] + "/client/quest/list"));
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
        dynamic obj = JsonConvert.DeserializeObject<dynamic>(contentdata);
        List<dynamic> quests = new List<dynamic>();

        for (int k = 0; k < obj.data.Count; k++)
        {
            var obj_data = obj.data[k];
            string queststatusdata = obj_data.sptStatus;
            int curQuestStatus = int.Parse(queststatusdata);
            if (questStatus.ContainsKey(curQuestStatus))
                quests.Add(obj_data);
        }
        var Onthquest = quests[0];
        DisplayAlert("info", JsonConvert.SerializeObject(Onthquest), "OK");
        for (int k = 0; k < quests.Count; k++)
        {
            var condtype = quests[k].conditions.AvailableForFinish[0].type;
            var condtarget = quests[k].conditions.AvailableForFinish[0].target;
            questData item = new questData();
            item.questname = quests[k].QuestName;
            item.questtypeid = quests[k].sptStatus;
            string sptstatus = quests[k].sptStatus;
            string questimage = quests[k].image;
            string questcolor = "";
            switch (int.Parse(sptstatus))
            {
                case 0:
                    questcolor = "gray";
                    break;
                case 1:
                    questcolor = "green";
                    break;
                case 2:
                    questcolor = "orange";
                    break;
                case 3:
                    questcolor = "cyan";
                    break;
                case 4:
                    questcolor = "gray";
                    break;
            }
            item.questcolor = questcolor;
            item.questimage = loginData["backendUrl"] + questimage;
            List<questCondition> _questconditions = new List<questCondition>();
            questCondition cond = new questCondition();
            cond.conditiontype = condtype;
            cond.curprogress = 0;
            cond.goalprogress = 0;
            cond.conditiondone = false;
            cond.conditiontarget = "Scav";
            _questconditions.Add(cond);

            userquests.Add(new quest(item, _questconditions));
        }

        questview.ItemsSource = userquests.Where(i => selectedtypes.Contains(i.questdata.questtypeid));
    }


    private void GoBack(object sender, EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(previousPage);
    }

}