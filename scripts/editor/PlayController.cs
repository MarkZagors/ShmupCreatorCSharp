using Godot;
using System;
using System.Collections.Generic;
using ScrollDirection = Enums.ScrollDirection;

namespace Editor
{
    public partial class PlayController : Node
    {
        [Export] public double ScrollTick { get; private set; } = 0.25;
        [Export] public Label TimelineTimeLabel { get; private set; }
        [Export] public VBoxContainer LanesNode { get; private set; }
        [Export] public PackedScene SequenceIconObj { get; private set; }

        public bool Playing { get; private set; } = false;
        public double Time { get; private set; } = 0.0;
        public List<Sequence> SequenceList { get; private set; } = new();
        public double LanesWidth { get; private set; } = 5.0;

        public override void _Ready()
        {
            UpdateUI();
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("editor_play"))
            {
                TogglePlaying();
            }

            if (Input.IsActionJustPressed("editor_scrollup"))
            {
                ScrollTime(ScrollDirection.FORWARD);
                UpdateUI();
            }

            if (Input.IsActionJustPressed("editor_scrolldown"))
            {
                ScrollTime(ScrollDirection.BACKWARD);
                UpdateUI();
            }

            if (Playing)
            {
                ProcessPlay(delta);
                UpdateUI();
            }
        }

        public void AddSequence()
        {
            if (CheckIfSequenceOverlap()) return;

            var sequenceNode = SequenceIconObj.Instantiate<Control>();
            var laneOne = LanesNode.GetChild<Control>(0);

            SequenceList.Add(
                new Sequence
                {
                    Time = this.Time,
                    Node = sequenceNode,
                }
            );

            laneOne.AddChild(sequenceNode);
            GD.Print($"Sequence added at time {Time:0.0}");

            UpdateUI();
        }

        private bool CheckIfSequenceOverlap()
        {
            foreach (Sequence sequence in SequenceList)
            {
                if (Math.Abs(sequence.Time - Time) < 0.1)
                {
                    GD.Print($"Sequence overlaps at time {Time:0.0}! Not adding");
                    return true;
                }
            }
            return false;
        }

        //=================
        //TIME SEQUENCE
        //=================
        private void ProcessPlay(double delta)
        {
            Time += delta;
        }

        private void TogglePlaying()
        {
            if (Playing)
            {
                Playing = false;
            }
            else
            {
                Playing = true;
            }
        }

        private void ScrollTime(ScrollDirection scrollDirection)
        {
            if (scrollDirection == ScrollDirection.FORWARD)
            {
                Time += ScrollTick;
            }

            else if (scrollDirection == ScrollDirection.BACKWARD)
            {
                Time -= ScrollTick;
                Time = Math.Max(Time, 0.0);
            }
        }

        //=================
        //UPDATING UI
        //=================
        private void UpdateUI()
        {
            UpdateLabels();
            UpdateSequenceUI();
        }

        private void UpdateLabels()
        {
            TimelineTimeLabel.Text = $"Time: {Time:0.00}";
        }

        private void UpdateSequenceUI()
        {
            foreach (Sequence sequence in SequenceList)
            {
                double anchor = 0.5 + (sequence.Time - Time) * 0.5 / LanesWidth;

                sequence.Node.AnchorLeft = (float)anchor;
                sequence.Node.AnchorRight = (float)anchor;
            }
        }
    }
}

