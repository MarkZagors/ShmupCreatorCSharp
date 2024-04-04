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
        private Range _yRange = null;
        private readonly Vector2 VIEWPORT_SIZE = new Vector2(768, 1024); //CHANGE THIS WHEN CHANGING VIEWPORT SIZE
        private readonly Vector2 START_POS = new Vector2(400, 200); //CHANGE THIS WHEN CHANGING VIEWPORT SIZE

        public override void _Ready()
        {
            ComponentsController.MoveTimelineUpdate += UpdateTimeline;
            Vector2 startingWorldToPoint = WorldToPoint(START_POS);
            _xRange = new Range
            {
                Max = new RefDouble(VIEWPORT_SIZE.X),
                Min = new RefDouble(0.0),
                Points = new List<Vector2> {
                    new Vector2(0.0f, startingWorldToPoint.X),
                    new Vector2(float.PositiveInfinity, startingWorldToPoint.X)
                }
            };
            _yRange = new Range
            {
                Max = new RefDouble(VIEWPORT_SIZE.Y),
                Min = new RefDouble(0.0),
                Points = new List<Vector2> {
                    new Vector2(0.0f, startingWorldToPoint.Y),
                    new Vector2(float.PositiveInfinity, startingWorldToPoint.Y)
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

            List<Vector2> pointsX = new();
            // List<Vector2> componentWorldPositions = new();
            pointsX.Add(new Vector2(0.0f, WorldToPoint(START_POS).X));
            // componentWorldPositions.Add(START_POS);
            for (int i = 0; i < sortedStartList.Count; i++)
            {
                (double, ComponentMovement) compoenentTimeKeyPairCurrent = sortedStartList[i];
                double startTime = compoenentTimeKeyPairCurrent.Item1;
                ComponentMovement componentMovement = compoenentTimeKeyPairCurrent.Item2;
                Range range = (componentMovement.GetModifier(ModifierID.MOVEMENT_CURVE) as ModifierRange).Range;
                double componentTime = (componentMovement.GetModifier(ModifierID.MOVEMENT_TIME) as ModifierDouble).Value;
                Vector2 componentPosition = (componentMovement.GetModifier(ModifierID.MOVEMENT_POSITION) as ModifierPosition).Position;

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
                    // pointsX.Add(new Vector2((float)startTime, 1.0f));
                    pointsX.Add(new Vector2((float)startTime, WorldToPoint(START_POS).X));
                    // componentWorldPositions.Add(START_POS);
                }

                Vector2 lastXPointPos = pointsX.Last();
                foreach (Vector2 point in range.Points)
                {
                    Vector2 scaledPoint = new Vector2(
                        x: point.X * (float)componentTime + (float)startTime,
                        y: (1.0f - point.Y) * (WorldToPoint(componentPosition).X - lastXPointPos.Y) + lastXPointPos.Y
                    );
                    if (isOverlapping && scaledPoint.X > nextStartTime)
                    {
                        double x = (double)((nextStartTime - startTime) / componentTime);
                        double yIntercept = range.GetYIntercept(x);
                        pointsX.Add(new Vector2(
                            x: (float)nextStartTime,
                            y: (1.0f - (float)yIntercept) * (WorldToPoint(componentPosition).X - lastXPointPos.Y) + lastXPointPos.Y
                        ));
                        // componentWorldPositions.Add(componentPosition);
                        break;
                    }
                    pointsX.Add(scaledPoint);
                    // componentWorldPositions.Add(componentPosition);
                }

                if (!isOverlapping && nextStartTime != null)
                {
                    //add middle separator points
                    pointsX.Add(new Vector2((float)nextStartTime, pointsX.Last().Y));
                    // componentWorldPositions.Add(componentPosition);
                }

                if (i == sortedStartList.Count - 1)
                {
                    //add last point
                    pointsX.Add(new Vector2(float.PositiveInfinity, pointsX.Last().Y));
                    // componentWorldPositions.Add(componentPosition);
                }
            }

            _xRange = Range.EmptyPointList(VIEWPORT_SIZE.X, 0);
            _yRange = Range.EmptyPointList(VIEWPORT_SIZE.Y, 0);
            _xRange.Points = pointsX;
            for (int i = 0; i < pointsX.Count; i++)
            {
                Vector2 point = pointsX[i];
                GD.Print(point);
            }
        }

        public float GetXValueAt(float time)
        {
            return (float)_xRange.GetValueAtNonInverted(time);
        }

        private Vector2 WorldToPoint(Vector2 worldPos)
        {
            return new Vector2(
                x: worldPos.X / VIEWPORT_SIZE.X,
                y: worldPos.Y / VIEWPORT_SIZE.Y
            );
        }
    }
}