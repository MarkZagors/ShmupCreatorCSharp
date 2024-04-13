using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class Sequence
    {
        required public double Time { get; set; }
        required public List<IComponent> Components { get; set; }
        public Control Node { get; set; }
    }
}
