using System;
using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class Range
    {
        public RefDouble Max { get; set; }
        public RefDouble Min { get; set; }
        public List<Vector2> Points { get; set; }

        public static Range From(double max, double min)
        {
            return new Range
            {
                Max = new RefDouble(max),
                Min = new RefDouble(min),
                Points = new List<Vector2> {
                    new Vector2(0.0f, 0.5f),
                    new Vector2(0.0f, 0.5f),
                    new Vector2(1.0f, 0.5f),
                    new Vector2(1.0f, 0.5f),
                }
            };
        }

        public static Range TiltedUp(double max, double min)
        {
            return new Range
            {
                Max = new RefDouble(max),
                Min = new RefDouble(min),
                Points = new List<Vector2> {
                    new Vector2(0.0f, 1.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                    new Vector2(1.0f, 0.0f),
                }
            };
        }

        public double GetValueAt(double x)
        {
            if (x < Points[0].X || x > Points[^1].X)
                throw new ArgumentOutOfRangeException(nameof(x), "Given X value is out of range of available points.");

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (x >= Points[i].X && x <= Points[i + 1].X)
                {
                    // Linear interpolation
                    double l = 1.0 - (Points[i].Y + (x - Points[i].X) / (Points[i + 1].X - Points[i].X) * (Points[i + 1].Y - Points[i].Y));
                    return Min.Value + l * (Max.Value - Min.Value);
                }
            }

            throw new InvalidOperationException("Unable to find value for given X.");
        }
    }
}