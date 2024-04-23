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
    private HTTPChain _requestChainIndex = 0;
    private string _connectionIP = "234.0.0.1";

    private enum HTTPChain
    {
        IDLE,
        INDEX,
        PHASES,
        ERROR,
    }

    public override void _Ready()
    {
        _httpRequestNode = GetNode<HttpRequest>("HTTPRequest");
        _httpRequestNode.RequestCompleted += OnResponse;
    }

    public void OnPublishButtonClick()
    {
        if (LevelPickController.SelectedLevelID == null) return;
        if (_requestChainIndex != HTTPChain.IDLE)
        {
            GD.Print("http processing");
            return;
        }
        _requestChainIndex = HTTPChain.INDEX;
        _requestLevelID = LevelPickController.SelectedLevelID;
        PublishStep();
    }

    private void PublishStep()
    {
        if (_requestChainIndex == HTTPChain.INDEX)
            SendIndexJson(_requestLevelID);
        else if (_requestChainIndex == HTTPChain.PHASES)
            SendPhasesJson(_requestLevelID);
        else if (_requestChainIndex == HTTPChain.IDLE)
            GD.Print("Request ended");
        else if (_requestChainIndex == HTTPChain.ERROR)
        {
            _requestChainIndex = HTTPChain.IDLE;
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
        switch (_requestChainIndex)
        {
            case HTTPChain.INDEX:
                if (responseCode == 200)
                {
                    _requestChainIndex = HTTPChain.PHASES;
                }
                else
                {
                    // GD.Print(responseCode);
                    GD.Print(message);
                    _requestChainIndex = HTTPChain.ERROR;
                }
                break;
            case HTTPChain.PHASES:
                //END
                if (responseCode == 200)
                {
                    _requestChainIndex = HTTPChain.IDLE;
                }
                if (responseCode == 500)
                {
                    GD.PrintErr(message);
                    _requestChainIndex = HTTPChain.ERROR;
                }
                break;
            default:
                GD.PrintErr("No request chain supported");
                return;
        }
        PublishStep();
    }
}
