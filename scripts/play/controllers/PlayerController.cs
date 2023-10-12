using System;
using Godot;

namespace Editor
{
    public partial class PlayerController : Node
    {
        [Export] public Node2D PlayerNode { get; private set; }
        [Export] public float Speed { get; private set; } = 150;
        [Export] public float FocusSpeedAmmount { get; private set; } = 0.4f;
        private Vector2 _velocity;
        private bool _focused = false;
        private Rect _windowRect = new Rect(0, 0, 768, 1024);

        public override void _Process(double delta)
        {
            ProcessMovement();
            ProcessPosition((float)delta);
        }

        private void ProcessPosition(float delta)
        {
            Vector2 newPos = new();

            if (!_focused)
            {
                newPos = new Vector2(
                    PlayerNode.Position.X + _velocity.X * delta,
                    PlayerNode.Position.Y + _velocity.Y * delta
                );
            }
            else
            {
                newPos = new Vector2(
                    PlayerNode.Position.X + _velocity.X * delta * FocusSpeedAmmount,
                    PlayerNode.Position.Y + _velocity.Y * delta * FocusSpeedAmmount
                );
            }

            newPos.X = Math.Clamp(newPos.X, _windowRect.X, _windowRect.Width);
            newPos.Y = Math.Clamp(newPos.Y, _windowRect.Y, _windowRect.Height);

            PlayerNode.Position = newPos;
        }

        private void ProcessMovement()
        {
            Vector2 inpuxAxis = new();

            if (Input.IsActionPressed("player_move_up"))
            {
                inpuxAxis.Y = -1;
            }
            if (Input.IsActionPressed("player_move_down"))
            {
                inpuxAxis.Y = 1;
            }
            if (Input.IsActionPressed("player_move_right"))
            {
                inpuxAxis.X = 1;
            }
            if (Input.IsActionPressed("player_move_left"))
            {
                inpuxAxis.X = -1;
            }

            _velocity = inpuxAxis.Normalized() * Speed;

            if (Input.IsActionPressed("player_focus"))
            {
                _focused = true;
            }
            else
            {
                _focused = false;
            }
        }
    }
}