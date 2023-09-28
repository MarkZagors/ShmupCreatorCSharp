using Godot;
using System;
using System.Collections.Generic;
using ScrollDirection = Enums.ScrollDirection;

namespace Editor
{
    public partial class PlayController : Node
    {
        [Export] public Label TimelineTimeLabel;
        [Export] public VBoxContainer LanesNode;
        [Export] public PackedScene SequenceIconObj;

        public bool Playing { get; private set; } = false;
        public double Time { get; private set; } = 0.0;
        public List<Sequence> SequenceList { get; private set; } = new();

        private const double SCROLL_TICK = 0.25;

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

            UpdateUI();
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
                Time += SCROLL_TICK;
            }

            else if (scrollDirection == ScrollDirection.BACKWARD)
            {
                Time -= SCROLL_TICK;
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

        }
    }
}

