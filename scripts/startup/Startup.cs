namespace Editor;

using Godot;

#if DEBUG
using System.Reflection;
using Chickensoft.GoDotTest;
#endif

public partial class Startup : Node2D
{
    public TestEnvironment Environment = default!;

    public override void _Ready()
    {
#if DEBUG
        // If this is a debug build, use GoDotTest to examine the
        // command line arguments and determine if we should run tests.
        Environment = TestEnvironment.From(OS.GetCmdlineArgs());
        if (Environment.ShouldRunTests)
        {
            CallDeferred("RunTests");
            return;
        }
#endif
        // If we don't need to run tests, we can just switch to the game scene.
        GetTree().ChangeSceneToFile("res://scenes/pages/Editor.tscn");
    }

    private void RunTests()
    {
        GetTree().ChangeSceneToFile("res://test/EditorTest.tscn");
        // _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
    }
}
