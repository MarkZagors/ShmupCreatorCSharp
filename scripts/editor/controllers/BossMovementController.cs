using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    public partial class BossMovementController : Node
    {
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public PlayController PlayController { get; private set; }
        private Range _xRange = null;

        public override void _Ready()
        {
            ComponentsController.MoveTimelineUpdate += UpdateTimeline;
            _xRange = new Range
            {
                Max = new RefDouble(1.0),
                Min = new RefDouble(0.0),
                Points = new List<Vector2> {
                    new Vector2(0.0f, 1.0f),
                    new Vector2(float.PositiveInfinity, 1.0f)
                }
            };
        }

        public void UpdateTimeline()
        {
            List<(double, ComponentMovement)> movementComponenetsStartList = new();
            foreach (Sequence sequence in PlayController.SequenceList)
            {
                foreach (IComponent component in sequence.Components)
                {
                    if (component.Type == ComponentType.MOVEMENT)
                    {
                        movementComponenetsStartList.Add((sequence.Time, (ComponentMovement)component));
                    }
                }
            }

            var sortedStartList = movementComponenetsStartList.OrderBy(tupl => tupl.Item1).ToList();

            List<Vector2> points = new();
            points.Add(new Vector2(0.0f, 1.0f));
            for (int i = 0; i < sortedStartList.Count; i++)
            {
                (double, ComponentMovement) compoenentTimeKeyPairCurrent = sortedStartList[i];
                double startTime = compoenentTimeKeyPairCurrent.Item1;
                ComponentMovement componentMovement = compoenentTimeKeyPairCurrent.Item2;
                Range range = (componentMovement.GetModifier(ModifierID.MOVEMENT_CURVE) as ModifierRange).Range;
                double componentTime = (componentMovement.GetModifier(ModifierID.MOVEMENT_TIME) as ModifierDouble).Value;

                double? nextStartTime = null;
                if (i + 1 < sortedStartList.Count)
                {
                    var compoenentTimeKeyPairNext = sortedStartList[i + 1];
                    nextStartTime = compoenentTimeKeyPairNext.Item1;
                }

                bool isOverlapping = false;
                if (nextStartTime != null && startTime + componentTime >= nextStartTime)
                {
                    //do something when overlap
                    GD.Print("overlap!");
                    isOverlapping = true;
                }

                if (i == 0)
                {
                    //add first separator point
                    points.Add(new Vector2((float)startTime, 1.0f));
                }

                foreach (Vector2 point in range.Points)
                {
                    Vector2 scaledPoint = new Vector2(
                        x: point.X * (float)componentTime + (float)startTime,
                        y: point.Y
                    );
                    points.Add(scaledPoint);
                }

                if (!isOverlapping && nextStartTime != null)
                {
                    //add middle separator points
                    points.Add(new Vector2((float)nextStartTime, range.Points.Last().Y));
                }

                if (i == sortedStartList.Count - 1)
                {
                    //add last point
                    points.Add(new Vector2(float.PositiveInfinity, range.Points.Last().Y));
                }
            }
            foreach (var point in points)
            {
                GD.Print(point);
            }
        }
    }
}
