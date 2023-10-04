using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public partial class ComponentRange : Control, IComponent
    {
        [Export] public Control RangeControllerNode;
        [Export] public Line2D RangeLineNode;
        private bool _rangeExpanded = true;
        private Control _currentHoldingPoint;
        private bool _rangeMouseDragging = false;
        private List<Control> _hoveringRangePoints = new();
        private List<Control> _rangePointNodes = new();

        public override void _Ready()
        {
            ResizeController resizeController = GetNode<ResizeController>("/root/Editor/Controllers/ResizeController");
            resizeController.WindowResized += OnWindowResize;

            var point1 = GetNode<Control>("RangeContainer/RangeController/Points/Point1");
            point1.MouseEntered += () => RangePointMouseEntered(point1);
            point1.MouseExited += () => RangePointMouseExited(point1);

            var point2 = GetNode<Control>("RangeContainer/RangeController/Points/Point2");
            point2.MouseEntered += () => RangePointMouseEntered(point2);
            point2.MouseExited += () => RangePointMouseExited(point2);

            _rangePointNodes.Add(point1);
            _rangePointNodes.Add(point2);

            UpdateLines();
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

                UpdateLines();
            }
        }

        private void UpdateLines()
        {
            RangeLineNode.ClearPoints();

            List<Vector2> expandedPointList = GetExpandedPointList();
            foreach (Vector2 point in expandedPointList)
            {
                Vector2 worldSpacePosition = RangeSpaceToWorldSpace(point);
                RangeLineNode.AddPoint(worldSpacePosition);
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

        private Vector2 RangeSpaceToWorldSpace(Vector2 pos)
        {
            return new Vector2
            {
                X = pos.X * RangeControllerNode.Size.X,
                Y = pos.Y * RangeControllerNode.Size.Y,
            };
        }

        private List<Vector2> GetExpandedPointList()
        {
            List<Vector2> pointList = new();
            foreach (Control rangePointNode in _rangePointNodes)
            {
                pointList.Add(new Vector2
                {
                    X = rangePointNode.AnchorRight,
                    Y = rangePointNode.AnchorTop,
                });
            }
            pointList.Sort((vecA, vecB) => vecA.X.CompareTo(vecB.X));

            Vector2 firstPointNonExpanded = pointList[0];
            pointList.Insert(0, new Vector2
            {
                X = 0.0f,
                Y = firstPointNonExpanded.Y,
            });

            Vector2 lastPointNonExpanded = pointList[pointList.Count - 1];
            pointList.Add(new Vector2
            {
                X = 1.0f,
                Y = lastPointNonExpanded.Y,
            });

            return pointList;
        }

        private void OnWindowResize()
        {
            UpdateLines();
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