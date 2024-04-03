using Godot;
using System;

namespace Editor
{
    public partial class FieldPosition : Control
    {
        [Signal] public delegate void UpdateEventHandler();
        [Signal] public delegate void MoveTimelineUpdateEventHandler();
        [Export] public Button PositionButton { get; private set; }
        public ModifierPosition ModifierPosition { get; private set; }
        public bool IsSelecting { get; set; }
        private SubViewportContainer _playerViewportContainer;
        private Control _midContainer;
        private readonly Vector2 VIEWPORT_SIZE = new Vector2(768, 1024); //CHANGE THIS WHEN CHANGING VIEWPORT SIZE

        public void Init(ModifierPosition modifierPosition)
        {
            ModifierPosition = modifierPosition;
        }

        public override void _Ready()
        {
            _playerViewportContainer = (SubViewportContainer)GetTree().GetNodesInGroup("play_viewport")[0];
            _midContainer = _playerViewportContainer.GetNode<Control>("../..");
            UpdateUI();
        }

        public override void _Process(double delta)
        {
            if (!IsSelecting) return;

            if (Input.IsActionJustPressed("mouse_click"))
            {
                var mousePos = GetViewport().GetMousePosition();
                var viewportRect = _playerViewportContainer.GetRect();
                viewportRect.Position = new Vector2(
                    x: viewportRect.Position.X + _midContainer.Position.X,
                    y: viewportRect.Position.Y + _midContainer.Position.Y
                );
                if (viewportRect.HasPoint(mousePos))
                {
                    Vector2 ratio = (mousePos - viewportRect.Position) / viewportRect.Size;
                    Vector2 position = ratio * VIEWPORT_SIZE;
                    ModifierPosition.Position = position;
                    EmitSignal(SignalName.MoveTimelineUpdate);
                    EmitSignal(SignalName.Update);
                    UpdateUI();
                }
                IsSelecting = false;
            }
        }

        public void OnButtonPressed()
        {
            IsSelecting = true;
        }

        private void UpdateUI()
        {
            PositionButton.Text = $"[{ModifierPosition.Position.X:F0}, {ModifierPosition.Position.Y:F0}]";
        }
    }
}
