using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class Range
    {
        public double Max { get; set; }
        public double Min { get; set; }
        public List<Vector2> Points { get; set; }

        public static Range From(double max, double min)
        {
            return new Range
            {
                Max = max,
                Min = min,
                Points = new List<Vector2> {
                    new Vector2(0.0f, 0.5f),
                    new Vector2(0.0f, 0.5f),
                    new Vector2(1.0f, 0.5f),
                    new Vector2(1.0f, 0.5f),
                }
            };
        }
    }
}