using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public partial class ComponentRange : Control, IComponent
    {
        [Export] public Control RangeControllerNode;
        private bool _rangeExpanded = true;
        private Control _currentHoldingPoint;
        private bool _rangeMouseDragging = false;
        private List<Control> _hoveringRangePoints = new();

        public override void _Ready()
        {
            var point = GetNode<Control>("RangeContainer/RangeController/Points/Point1");
            point.MouseEntered += () => RangePointMouseEntered(point);
            point.MouseExited += () => RangePointMouseExited(point);
        }

        public override void _Process(double delta)
        {
            if (_rangeExpanded)
                ProcessPointDragging();
        }

        private void ProcessPointDragging()
        {
            if (_hoveringRangePoints.Count > 0)
            {
                if (Input.IsActionJustPressed("mouse_click"))
                {
                    _currentHoldingPoint = _hoveringRangePoints.Last();
                    _rangeMouseDragging = true;
                }
            }

            if (Input.IsActionJustReleased("mouse_click"))
            {
                _currentHoldingPoint = null;
                _rangeMouseDragging = false;
            }

            if (_rangeMouseDragging)
            {
                Vector2 mousePos = GetMousePositionInRangeContext();
                _currentHoldingPoint.AnchorRight = mousePos.X;
                _currentHoldingPoint.AnchorLeft = mousePos.X;
                _currentHoldingPoint.AnchorBottom = mousePos.Y;
                _currentHoldingPoint.AnchorTop = mousePos.Y;
            }
        }

        public void ToggleRange()
        {
            if (_rangeExpanded)
            {
                CustomMinimumSize = new Vector2(0, 50);
                _rangeExpanded = false;
            }
            else
            {
                CustomMinimumSize = new Vector2(0, 200);
                _rangeExpanded = true;
            }
        }

        private Vector2 GetMousePositionInRangeContext()
        {
            var mousePos = RangeControllerNode.GetLocalMousePosition();
            return new Vector2
            {
                X = (float)Mathf.Clamp(mousePos.X / RangeControllerNode.Size.X, 0.0, 1.0),
                Y = (float)Mathf.Clamp(mousePos.Y / RangeControllerNode.Size.Y, 0.0, 1.0),
            };
        }

        public void RangePointMouseEntered(Control node)
        {
            _hoveringRangePoints.Add(node);
        }

        public void RangePointMouseExited(Control node)
        {
            _hoveringRangePoints.Remove(node);
        }
    }
}