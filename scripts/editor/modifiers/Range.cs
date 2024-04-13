using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;

namespace Editor
{
    public class Range
    {
        required public RefDouble Max { get; set; }
        required public RefDouble Min { get; set; }
        required public List<Vector2> Points { get; set; }

        // Y = 0.0 is top!
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

        public static Range EmptyPointList(double max, double min)
        {
            return new Range
            {
                Max = new RefDouble(max),
                Min = new RefDouble(min),
                Points = new List<Vector2>()
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

        public static Range ManualRange(double max, double min, List<Vector2> points)
        {
            return new Range
            {
                Max = new RefDouble(max),
                Min = new RefDouble(min),
                Points = points
            };
        }

        // public List<Vector2> GetTruncatedPoints()
        // {
        //     List<Vector2> trucatedPoints = Points.GetRange(1, Points.Count - 2);
        //     return trucatedPoints;
        // }

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

        public double GetValueAtNonInverted(double x)
        {
            if (x < Points[0].X || x > Points[^1].X)
                throw new ArgumentOutOfRangeException(nameof(x), "Given X value is out of range of available points.");

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (x >= Points[i].X && x <= Points[i + 1].X)
                {
                    // Linear interpolation
                    double l = (Points[i].Y + (x - Points[i].X) / (Points[i + 1].X - Points[i].X) * (Points[i + 1].Y - Points[i].Y));
                    return Min.Value + l * (Max.Value - Min.Value);
                }
            }

            throw new InvalidOperationException("Unable to find value for given X.");
        }

        public double GetYIntercept(double x)
        {
            if (x < Points[0].X || x > Points[^1].X)
                throw new ArgumentOutOfRangeException(nameof(x), "Given X value is out of range of available points.");

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (x >= Points[i].X && x <= Points[i + 1].X)
                {
                    // Linear interpolation
                    double l = (Points[i].Y + (x - Points[i].X) / (Points[i + 1].X - Points[i].X) * (Points[i + 1].Y - Points[i].Y));
                    return l;
                }
            }

            throw new InvalidOperationException("Unable to find value for given X.");
        }
    }
}
