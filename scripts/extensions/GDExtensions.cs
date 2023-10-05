using System.Diagnostics;
using Godot;

namespace ExtensionMethods
{
    public static class GDE
    {
        public static void PrintHere()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();

            GD.Print($"Ping! Called Method: {stackFrames[1].GetMethod()}");
        }
    }
}