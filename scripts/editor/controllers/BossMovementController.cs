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

        public override void _Ready()
        {
            ComponentsController.MoveTimelineUpdate += UpdateTimeline;
        }

        public void UpdateTimeline()
        {
            GD.Print("Update timeline");
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

            foreach (var compoenentTimeKeyPair in sortedStartList)
            {
                double time = compoenentTimeKeyPair.Item1;
                ComponentMovement componentMovement = compoenentTimeKeyPair.Item2;
                GD.Print(time);
            }
        }
    }
}
