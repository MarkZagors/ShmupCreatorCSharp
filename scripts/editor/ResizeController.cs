using Godot;
using System;

namespace Editor
{
    public partial class ResizeController : Node
    {
        [ExportCategory("Containers")]
        [Export] public Control BottomContainer;
        [Export] public Control TopContainer;
        [Export] public Control LeftContainer;
        [Export] public Control MidContainer;
        [Export] public Control RightContainer;
        [Export] public ColorRect BottomContainerResizeBar;

        [ExportCategory("Limits")]
        [Export(PropertyHint.Range, "0,1,")] public float LimitBottomMax = 0.5f;
        [Export(PropertyHint.Range, "0,1,")] public float LimitBottomMin = 0.2f;

        private bool _isBottomContainerMouseEntered = false;
        private bool _isRightContainerMouseEntered = false;
        private bool _isLeftContainerMouseEntered = false;
        private bool _isBottomContainerDragging = false;
        private bool _isRightContainerDragging = false;
        private bool _isLeftContainerDragging = false;

        public override void _Ready()
        {
            BottomContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("mouse_click"))
            {
                ProcessMouseClick();
            }

            if (Input.IsActionJustReleased("mouse_click"))
            {
                ProcessMouseRelease();
            }

            ProcessDragging();
        }

        private void ProcessDragging()
        {
            Vector2 PosMouse = GetViewport().GetMousePosition();
            Vector2 ViewportSize = GetViewport().GetVisibleRect().Size;
            if (_isBottomContainerDragging)
            {
                float YPoint = PosMouse.Y / ViewportSize.Y;

                YPoint = Math.Clamp(YPoint, LimitBottomMin, LimitBottomMax);
                GD.Print(YPoint);

                BottomContainer.AnchorTop = YPoint;
                TopContainer.AnchorBottom = YPoint;
            }
        }

        private void ProcessMouseClick()
        {
            if (_isBottomContainerMouseEntered) _isBottomContainerDragging = true;
            if (_isRightContainerMouseEntered) _isRightContainerDragging = true;
            if (_isLeftContainerMouseEntered) _isLeftContainerDragging = true;
        }

        private void ProcessMouseRelease()
        {
            _isBottomContainerDragging = false;
            _isRightContainerDragging = false;
            _isLeftContainerDragging = false;
        }

        public void OnBottomContainerMouseEntered() //connection
        {
            _isBottomContainerMouseEntered = true;
            BottomContainerResizeBar.SelfModulate = Colors.White;
        }

        public void OnBottomContainerMouseExited() //connection
        {
            _isBottomContainerMouseEntered = false;
            BottomContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);
        }
    }
}
