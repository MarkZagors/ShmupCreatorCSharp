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

            List<Vector2> points = new();
            List<Vector2> componentWorldPositions = new();
            points.Add(new Vector2(0.0f, 1.0f));
            componentWorldPositions.Add(START_POS);
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
                    points.Add(new Vector2((float)startTime, 1.0f));
                    componentWorldPositions.Add(componentPosition);
                }

                foreach (Vector2 point in range.Points)
                {
                    Vector2 scaledPoint = new Vector2(
                        x: point.X * (float)componentTime + (float)startTime,
                        y: point.Y
                    );
                    if (isOverlapping && scaledPoint.X > nextStartTime)
                    {
                        double x = (double)((nextStartTime - startTime) / componentTime);
                        double yIntercept = range.GetYIntercept(x);
                        points.Add(new Vector2((float)nextStartTime, (float)yIntercept));
                        componentWorldPositions.Add(componentPosition);
                        break;
                    }
                    points.Add(scaledPoint);
                    componentWorldPositions.Add(componentPosition);
                }

                if (!isOverlapping && nextStartTime != null)
                {
                    //add middle separator points
                    points.Add(new Vector2((float)nextStartTime, range.Points.Last().Y));
                    componentWorldPositions.Add(componentPosition);
                }

                if (i == sortedStartList.Count - 1)
                {
                    //add last point
                    points.Add(new Vector2(float.PositiveInfinity, range.Points.Last().Y));
                    componentWorldPositions.Add(componentPosition);
                }
            }

            _xRange = Range.EmptyPointList(VIEWPORT_SIZE.X, 0);
            _yRange = Range.EmptyPointList(VIEWPORT_SIZE.Y, 0);
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];
                Vector2 worldPosition = componentWorldPositions[i];
                Vector2 worldToPoint = WorldToPoint(worldPosition);
                Vector2 xRangePoint = new Vector2(point.X, worldToPoint.X);
                Vector2 yRangePoint = new Vector2(point.X, worldToPoint.Y);
                GD.Print(yRangePoint);
            }
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
