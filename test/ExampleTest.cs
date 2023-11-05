using Godot;
using Chickensoft.GoDotTest;
using Chickensoft.GoDotLog;
using Editor;

public class ExampleTest : TestClass
{
    private readonly Chickensoft.GoDotLog.ILog _log = new GDLog(nameof(ExampleTest));
    private readonly Node _testScene;

    public ExampleTest(Node testScene) : base(testScene)
    {
        _testScene = testScene;
    }

    [SetupAll]
    public void SetupAll() => _log.Print("Setup everything");

    [Setup]
    public void Setup() => _log.Print("Setup");

    [Test]
    public void TestCreateBox()
    {
        _log.Print("Test CreateBox");
        CreateBoxController createBoxController = _testScene.GetNode<CreateBoxController>("Controllers/CreateBoxController");
        createBoxController.OnClickNewComponent();
    }

    [Cleanup]
    public void Cleanup() => _log.Print("Cleanup");

    [CleanupAll]
    public void CleanupAll() => _log.Print("Cleanup everything");

    [Failure]
    public void Failure() =>
      _log.Print("Runs whenever any of the tests in this suite fail.");
}