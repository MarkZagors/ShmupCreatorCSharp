using Godot;
using System;

namespace Editor
{
    public partial class ResizeController : Node
    {
        [Signal] public delegate void WindowResizedEventHandler();

        [ExportCategory("Containers")]
        [Export] public Control BottomContainer;
        [Export] public Control TopContainer;
        [Export] public Control LeftContainer;
        [Export] public Control MidContainer;
        [Export] public Control RightContainer;
        [Export] public ColorRect BottomContainerResizeBar;
        [Export] public ColorRect LeftContainerResizeBar;
        [Export] public ColorRect RightContainerResizeBar;

        private const float LIMIT_BOTTOM_MAX = 0.95f;
        private const float LIMIT_BOTTOM_MIN = 0.2f;
        private const float LIMIT_LEFT_MAX = 0.45f;
        private const float LIMIT_LEFT_MIN = 0.1f;
        private const float LIMIT_RIGHT_MAX = 0.9f;
        private const float LIMIT_RIGHT_MIN = 0.55f;

        private bool _isBottomContainerMouseEntered = false;
        private bool _isRightContainerMouseEntered = false;
        private bool _isLeftContainerMouseEntered = false;
        private bool _isBottomContainerDragging = false;
        private bool _isRightContainerDragging = false;
        private bool _isLeftContainerDragging = false;

        public override void _Ready()
        {
            BottomContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);
            LeftContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);
            RightContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);

            GetViewport().SizeChanged += OnWindowChangedExternally;
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
                YPoint = Math.Clamp(YPoint, LIMIT_BOTTOM_MIN, LIMIT_BOTTOM_MAX);

                BottomContainer.AnchorTop = YPoint;
                TopContainer.AnchorBottom = YPoint;

                EmitSignal(SignalName.WindowResized);
            }

            if (_isLeftContainerDragging)
            {
                float XPoint = PosMouse.X / ViewportSize.X;
                XPoint = Math.Clamp(XPoint, LIMIT_LEFT_MIN, LIMIT_LEFT_MAX);

                LeftContainer.AnchorRight = XPoint;
                MidContainer.AnchorLeft = XPoint;

                EmitSignal(SignalName.WindowResized);
            }

            if (_isRightContainerDragging)
            {
                float XPoint = PosMouse.X / ViewportSize.X;
                XPoint = Math.Clamp(XPoint, LIMIT_RIGHT_MIN, LIMIT_RIGHT_MAX);

                RightContainer.AnchorLeft = XPoint;
                MidContainer.AnchorRight = XPoint;

                EmitSignal(SignalName.WindowResized);
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

        public void OnLeftContainerMouseEntered() //connection
        {
            _isLeftContainerMouseEntered = true;
            LeftContainerResizeBar.SelfModulate = Colors.White;
        }

        public void OnLeftContainerMouseExited() //connection
        {
            _isLeftContainerMouseEntered = false;
            LeftContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);
        }

        public void OnRightContainerMouseEntered() //connection
        {
            _isRightContainerMouseEntered = true;
            RightContainerResizeBar.SelfModulate = Colors.White;
        }

        public void OnRightContainerMouseExited() //connection
        {
            _isRightContainerMouseEntered = false;
            RightContainerResizeBar.SelfModulate = Color.Color8(0, 0, 0, 0);
        }

        private async void OnWindowChangedExternally()
        {
            await ToSignal(GetTree(), "process_frame");
            EmitSignal(SignalName.WindowResized);
        }
    }
}
