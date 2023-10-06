using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class Sequence
    {
        public Sequence(double time, Control node)
        {
            Time = time;
            Node = node;
            Components = new();
        }

        public double Time { get; set; }
        public Control Node { get; set; }
        public List<IComponent> Components { get; set; }
    }
}
