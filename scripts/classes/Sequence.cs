using System.Collections.Generic;
using Godot;

public class Sequence
{
    public double Time { get; set; }
    public Control Node { get; set; }
    public List<IComponent> Components { get; set; }
}