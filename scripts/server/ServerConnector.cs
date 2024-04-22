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
        if (_requestChainIndex != 0) return;
        _requestLevelID = LevelPickController.SelectedLevelID;
        PublishStep();
    }

    private void PublishStep()
    {
        if (_requestChainIndex == 0)
            SendIndexJson(_requestLevelID);
        else if (_requestChainIndex == 1)
            SendPhasesJson(_requestLevelID);
        // else if (_requestChainIndex == 2)
        //     SendMusicJson(_requestLevelID);
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

    // private void SendMusicJson(string levelID)
    // {
    //     FileAccess musicFile = FileAccess.Open($"user://levels/{levelID}/music.mp3", FileAccess.ModeFlags.Read);

    //     var buffer = musicFile.GetBuffer((long)musicFile.GetLength());
    //     string req = Marshalls.RawToBase64(buffer);

    //     musicFile.Close();

    //     string[] headers = new string[] { "Content-Type: audio/mpeg" };
    //     _httpRequestNode.Request("http://localhost:3000/pub_music", customHeaders: headers, method: Godot.HttpClient.Method.Post, requestData: req);
    //     GD.Print("sent music");
    // }

    private void OnResponse(long result, long responseCode, string[] headers, byte[] body)
    {
        _requestChainIndex += 1;
        if (_requestChainIndex == 2)
        {
            GD.Print("end");
            _requestChainIndex = 0;
            return;
        }
        PublishStep();
    }
}
