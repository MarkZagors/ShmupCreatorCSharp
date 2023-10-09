using System.Diagnostics;
using Godot;

namespace ExtensionMethods
{
    public static class GDE
    {
        public static void PrintHere()
        {
            StackTrace stackTrace = new();
            StackFrame[] stackFrames = stackTrace.GetFrames();

            GD.Print($"Ping! Called Method: {stackFrames[1].GetMethod()}");
        }

        public static void PrintRange(Editor.Range range)
        {
            GD.Print($"Max: {range.Max.Value}, Min: {range.Min.Value}, Points: {range.Points.ToStringMembers()}");
        }
    }
}