using System.Reflection;
using Godot;
using Chickensoft.GoDotTest;

public partial class EditorTest : Node
{
    public override async void _Ready()
    => await GoTest.RunTests(Assembly.GetExecutingAssembly(), this);
}
