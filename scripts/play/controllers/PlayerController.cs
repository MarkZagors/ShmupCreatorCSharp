using System;
using Godot;

namespace Editor
{
    public partial class PlayerController : Node
    {
        [Signal] public delegate void OnPlayerHitEventHandler();
        [Export] public Node2D PlayerNode { get; private set; }
        [Export] public float Speed { get; private set; } = 150;
        [Export] public float FocusSpeedAmmount { get; private set; } = 0.4f;
        private Vector2 _velocity;
        private bool _focused = false;
        private Rect _windowRect = new Rect(0, 0, 768, 1024);
        private bool _isInvincible = false;
        private float _invincibleTimer = 1.0f;
        private AnimationPlayer _playerNodeAnimationPlayer;
        private const float INVINCIBLE_TIMER_THRESHOLD = 2.0f;

        public override void _Process(double delta)
        {
            _playerNodeAnimationPlayer = PlayerNode.GetNode<AnimationPlayer>("AnimationPlayer");
            ProcessMovement();
            ProcessPosition((float)delta);
            ProcessInvincible();
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
            _invincibleTimer += (float)delta;
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

        private void ProcessInvincible()
        {
            if (_invincibleTimer < INVINCIBLE_TIMER_THRESHOLD)
            {
                _isInvincible = true;
                _playerNodeAnimationPlayer.Play("blinking");
            }
            else
            {
                _isInvincible = false;
                _playerNodeAnimationPlayer.Play("idle");
            }
        }

        public void OnHitboxEnter(Area2D area)
        {
            if (!_isInvincible)
            {
                EmitSignal(SignalName.OnPlayerHit);
                _invincibleTimer = 0.0f;
            }
        }
    }
}
