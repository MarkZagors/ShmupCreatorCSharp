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
    private HttpStatus _requestChainIndex = 0;
    private string _connectionIP = "127.0.0.1";

    private enum HttpStatus
    {
        IDLE,
        POST_INDEX,
        POST_PHASES,
        POST_ERROR,
        GET_LEVELS,
    }

    public override void _Ready()
    {
        _httpRequestNode = GetNode<HttpRequest>("HTTPRequest");
        _httpRequestNode.RequestCompleted += OnResponse;

        GetLevels();
    }

    public void OnPublishButtonClick()
    {
        if (LevelPickController.SelectedLevelID == null) return;
        if (_requestChainIndex != HttpStatus.IDLE)
        {
            GD.Print("http processing");
            return;
        }
        _requestChainIndex = HttpStatus.POST_INDEX;
        _requestLevelID = LevelPickController.SelectedLevelID;
        PublishStep();
    }

    private void PublishStep()
    {
        if (_requestChainIndex == HttpStatus.POST_INDEX)
            SendIndexJson(_requestLevelID);
        else if (_requestChainIndex == HttpStatus.POST_PHASES)
            SendPhasesJson(_requestLevelID);
        else if (_requestChainIndex == HttpStatus.IDLE)
            GD.Print("Request ended");
        else if (_requestChainIndex == HttpStatus.POST_ERROR)
        {
            _requestChainIndex = HttpStatus.IDLE;
            GD.Print("ERROR: Request ended early");
        }
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

        string[] headers = new string[] {
            "Content-Type: application/json",
            $"Level-ID: {levelID}"
        };
        _httpRequestNode.Request($"http://{_connectionIP}:3000/pub_index", customHeaders: headers, method: Godot.HttpClient.Method.Post, requestData: req);
        GD.Print("sent index");
    }

    private void SendPhasesJson(string levelID)
    {
        FileAccess phasesFile = FileAccess.Open($"user://levels/{levelID}/phases.json", FileAccess.ModeFlags.Read);

        string req = phasesFile.GetAsText();

        phasesFile.Close();

        string[] headers = new string[] {
            "Content-Type: text/plain",
            $"Level-ID: {levelID}"
        };
        _httpRequestNode.Request($"http://{_connectionIP}:3000/pub_phases", customHeaders: headers, method: Godot.HttpClient.Method.Post, requestData: req);
        GD.Print("sent phases");
    }

    private void GetLevels()
    {
        _requestChainIndex = HttpStatus.GET_LEVELS;
        _httpRequestNode.Request($"http://{_connectionIP}:3000/get_levels", method: Godot.HttpClient.Method.Get);
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
        string message = System.Text.Encoding.UTF8.GetString(body);

        if (_requestChainIndex == HttpStatus.GET_LEVELS)
        {
            GD.Print("getting");
            GD.Print(message);
            _requestChainIndex = HttpStatus.IDLE;
            return;
        }

        switch (_requestChainIndex)
        {
            case HttpStatus.POST_INDEX:
                if (responseCode == 200)
                {
                    _requestChainIndex = HttpStatus.POST_PHASES;
                }
                else
                {
                    // GD.Print(responseCode);
                    GD.Print(message);
                    _requestChainIndex = HttpStatus.POST_ERROR;
                }
                break;
            case HttpStatus.POST_PHASES:
                //END
                if (responseCode == 200)
                {
                    _requestChainIndex = HttpStatus.IDLE;
                }
                if (responseCode == 500)
                {
                    GD.PrintErr(message);
                    _requestChainIndex = HttpStatus.POST_ERROR;
                }
                break;
            default:
                GD.PrintErr("No request chain supported");
                return;
        }
        PublishStep();
    }
}
