using Editor;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Xml;
using FileAccess = Godot.FileAccess;

public partial class ServerConnector : Node
{
    [Export] public LevelPickController LevelPickController { get; private set; }
    [Export] public SavingManager SavingManager { get; private set; }
    [Export] public Control DownloadsContainer { get; private set; }
    [Export] public VBoxContainer DownloadsVboxContainer { get; private set; }
    [Export] public PackedScene DownloadLevelNodeObj { get; private set; }
    private HttpRequest _httpRequestNode;
    private string _requestLevelID = null;
    private HttpStatus _requestChainIndex = 0;
    private string _connectionIP = "127.0.0.1";
    private Godot.Collections.Array _cachedListArray = new();

    private enum HttpStatus
    {
        IDLE,
        POST_INDEX,
        POST_PHASES,
        POST_ERROR,
        GET_LEVELS,
        GET_INDEX,
        GET_PHASES,
        GET_ERROR,
    }

    public override void _Ready()
    {
        _httpRequestNode = GetNode<HttpRequest>("HTTPRequest");
        _httpRequestNode.RequestCompleted += OnResponse;
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

    public void OnDownloadButtonClick(string levelID)
    {
        _requestChainIndex = HttpStatus.GET_INDEX;
        _requestLevelID = levelID;
        DownloadStep();
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
            GD.Print("ERROR: POST Request ended early");
        }
        // else if (_requestChainIndex == 2)
        //     SendMusicJson(_requestLevelID);
    }

    private void DownloadStep()
    {
        if (_requestChainIndex == HttpStatus.GET_INDEX)
            GetLevelIndex(_requestLevelID);
        else if (_requestChainIndex == HttpStatus.GET_PHASES)
            GetLevelPhases(_requestLevelID);
        else if (_requestChainIndex == HttpStatus.IDLE)
        {
            GD.Print("Download ended");
            UpdateDownloadList();
        }
        else if (_requestChainIndex == HttpStatus.POST_ERROR)
        {
            _requestChainIndex = HttpStatus.IDLE;
            GD.Print("ERROR: GET Request ended early");
        }
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

    private void GetLevelIndex(string levelID)
    {
        string[] headers = new string[] {
            $"Level-ID: {levelID}"
        };
        _httpRequestNode.Request($"http://{_connectionIP}:3000/get_level_index", customHeaders: headers, method: Godot.HttpClient.Method.Get);
    }

    private void GetLevelPhases(string levelID)
    {
        string[] headers = new string[] {
            $"Level-ID: {levelID}"
        };
        _httpRequestNode.Request($"http://{_connectionIP}:3000/get_level_phases", customHeaders: headers, method: Godot.HttpClient.Method.Get);
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

    public void OnDownloadsButtonClick()
    {
        DownloadsContainer.Visible = true;
        GetLevels();
    }

    public void OnDonwloadCloseButtonCLick()
    {
        LevelPickController.UpdateLocalLevelsList();
        DownloadsContainer.Visible = false;
    }

    private void OnResponse(long result, long responseCode, string[] headers, byte[] body)
    {
        string message = System.Text.Encoding.UTF8.GetString(body);

        switch (_requestChainIndex)
        {
            case HttpStatus.GET_LEVELS:
                OnLevelListReceived(message);
                _requestChainIndex = HttpStatus.IDLE;
                break;
            case HttpStatus.GET_INDEX:
                if (responseCode == 200)
                {
                    DownloadIndex(_requestLevelID, message);
                    _requestChainIndex = HttpStatus.GET_PHASES;
                }
                else
                {
                    GD.Print(message);
                    _requestChainIndex = HttpStatus.GET_ERROR;
                }
                DownloadStep();
                break;
            case HttpStatus.GET_PHASES:
                if (responseCode == 200)
                {
                    DownloadPhases(_requestLevelID, message);
                    _requestChainIndex = HttpStatus.IDLE;
                }
                else
                {
                    GD.Print(message);
                    _requestChainIndex = HttpStatus.GET_ERROR;
                }
                DownloadStep();
                break;
            case HttpStatus.POST_INDEX:
                //END
                if (responseCode == 200)
                {
                    _requestChainIndex = HttpStatus.POST_PHASES;
                }
                else
                {
                    GD.Print(message);
                    _requestChainIndex = HttpStatus.POST_ERROR;
                }
                PublishStep();
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
                PublishStep();
                break;
            default:
                GD.PrintErr("No request chain supported");
                return;
        }
    }

    private void DownloadIndex(string levelID, string message)
    {
        DirAccess levelsDirectory = DirAccess.Open("user://levels");
        levelsDirectory.MakeDir(levelID);

        string cleanedIndex = message.Substr(1, message.Length - 2);

        FileAccess indexFile = FileAccess.Open($"user://levels/{levelID}/index.json", FileAccess.ModeFlags.Write);
        indexFile.StoreString(cleanedIndex);
        indexFile.Close();
    }

    private void DownloadPhases(string levelID, string message)
    {
        FileAccess phasesFile = FileAccess.Open($"user://levels/{levelID}/phases.json", FileAccess.ModeFlags.Write);
        phasesFile.StoreString(message);
        phasesFile.Close();
    }

    private void OnLevelListReceived(string message)
    {
        Godot.Collections.Array list = (Godot.Collections.Array)Json.ParseString(message);
        _cachedListArray = list;
        UpdateDownloadList();
    }

    private void UpdateDownloadList()
    {
        List<string> idList = SavingManager.GetLevelIDList();
        foreach (var child in DownloadsVboxContainer.GetChildren()) child.QueueFree();
        foreach (var listItem in _cachedListArray)
        {
            Godot.Collections.Dictionary<string, string> listItemDic = (Godot.Collections.Dictionary<string, string>)listItem;
            Control downloadLevelNode = DownloadLevelNodeObj.Instantiate<Control>();
            string levelServerID = (string)listItemDic["id"];
            string levelName = (string)listItemDic["levelName"];
            string levelAuthor = (string)listItemDic["levelAuthor"];
            downloadLevelNode.GetNode<Label>("LevelLabel").Text = $"{levelName} - By {levelAuthor}";
            DownloadsVboxContainer.AddChild(downloadLevelNode);

            bool foundDuplicateLocally = false;
            foreach (string localLevelId in idList)
            {
                if (localLevelId.Equals(levelServerID))
                {
                    foundDuplicateLocally = true;
                    downloadLevelNode.GetNode<Label>("AlreadyInstalledLabel").Visible = true;
                    break;
                }
            }

            if (foundDuplicateLocally == false)
            {
                downloadLevelNode.GetNode<Button>("DownloadButton").Visible = true;
                downloadLevelNode.GetNode<Button>("DownloadButton").Pressed += () => OnDownloadButtonClick(levelServerID);
            }
        }
    }
}
