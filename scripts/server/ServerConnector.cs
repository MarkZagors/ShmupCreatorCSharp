using Editor;
using Godot;
using System;
using System.IO;
using System.Net.Http;
using FileAccess = Godot.FileAccess;

public partial class ServerConnector : Node
{
    [Export] public LevelPickController LevelPickController { get; private set; }
    private HttpRequest _httpRequestNode;
    private string _requestLevelID = null;
    private int _requestChainIndex = 0;

    public override void _Ready()
    {
        _httpRequestNode = GetNode<HttpRequest>("HTTPRequest");
        _httpRequestNode.RequestCompleted += OnResponse;
    }

    public void OnPublishButtonClick()
    {
        if (LevelPickController.SelectedLevelID == null) return;
        _requestLevelID = LevelPickController.SelectedLevelID;
        PublishStep();
    }

    private void PublishStep()
    {
        if (_requestChainIndex == 0)
            SendIndexJson(_requestLevelID);
        else if (_requestChainIndex == 1)
            SendPhasesJson(_requestLevelID);
    }

    private void SendIndexJson(string levelID)
    {
        FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Read);
        string req =
$@"{{
    ""index"": {indexFile.GetAsText()}
}}";

        indexFile.Close();

        string[] headers = new string[] { "Content-Type: application/json" };
        _httpRequestNode.Request("http://localhost:3000/pub_index", customHeaders: headers, method: Godot.HttpClient.Method.Post, requestData: req);
        GD.Print("sent index");
    }

    private void SendPhasesJson(string levelID)
    {
        FileAccess phasesFile = FileAccess.Open($"user://levels/{levelID}/phases.json", FileAccess.ModeFlags.Read);

        string req = phasesFile.GetAsText();

        phasesFile.Close();

        string[] headers = new string[] { "Content-Type: text/plain" };
        _httpRequestNode.Request("http://localhost:3000/pub_phases", customHeaders: headers, method: Godot.HttpClient.Method.Post, requestData: req);
        GD.Print("sent phases");
    }

    private void PublishNew(string levelID)
    {
        FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Read);
        FileAccess phasesFile = FileAccess.Open($"user://levels/{levelID}/phases.json", FileAccess.ModeFlags.Read);
        FileAccess musicFile = FileAccess.Open($"user://levels/{levelID}/music.mp3", FileAccess.ModeFlags.Read);

        string musicBuffer = "";
        if (musicFile != null)
        {
            // var buffer = musicFile.GetBuffer((long)musicFile.GetLength());
            // char[] chars = new char[(buffer.Length / sizeof(char) + 60)];
            // System.Buffer.BlockCopy(buffer, 0, chars, 0, buffer.Length);
            // musicBuffer = new string(chars);
            // // musicBuffer = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            // GD.Print("written");
            // musicFile.Close();
        }

        //         string req =
        // $@"{{
        //     ""index"": {indexFile.GetAsText()},
        //     ""phases"": ""{phasesFile.GetAsText()}"",
        // }}";
        string req =
$@"{{
    ""index"": {indexFile.GetAsText()}
}}";

        indexFile.Close();
        phasesFile.Close();

        string[] headers = new string[] { "Content-Type: application/json" };
        _httpRequestNode.Request("http://localhost:3000", customHeaders: headers, method: Godot.HttpClient.Method.Post, requestData: req);
        GD.Print("sent");
    }

    private void OnResponse(long result, long responseCode, string[] headers, byte[] body)
    {
        string str = System.Text.Encoding.UTF8.GetString(body);
        GD.Print(str);

        _requestChainIndex += 1;
        PublishStep();
    }
}
