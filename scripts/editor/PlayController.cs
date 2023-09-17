using Godot;
using System;

namespace Editor
{
    public partial class PlayController : Node
    {
        [Export] public Label TimelineTimeLabel;

        private const double SCROLL_TICK = 0.25;
        private enum ScrollDirection
        {
            FORWARD,
            BACKWARD
        }

        public bool Playing { get; set; } = false;
        public double Time { get; set; } = 0.0;

        public override void _Ready()
        {
            UpdateLabels();
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
                UpdateLabels();
            }

            if (Input.IsActionJustPressed("editor_scrolldown"))
            {
                ScrollTime(ScrollDirection.BACKWARD);
                UpdateLabels();
            }

            if (Playing)
            {
                ProcessPlay(delta);
                UpdateLabels();
            }
        }

        private void ProcessPlay(double delta)
        {
            Time += delta;
        }

        private void UpdateLabels()
        {
            TimelineTimeLabel.Text = $"Time: {Time:0.00}";
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
    }
}

