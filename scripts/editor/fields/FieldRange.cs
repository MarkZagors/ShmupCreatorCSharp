using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using Godot;

namespace Editor
{
    public partial class FieldRange : Control, IField
    {
        [Export] double DoubleClickThreshold = 0.35;
        [Export] public Control RangeControllerNode;
        [Export] public Control RangePointsGroupNode;
        [Export] public Line2D RangeLineNode;
        [Export] public PackedScene RangePointObj;
        private bool _rangeExpanded = true;
        private Control _currentHoldingPoint;
        private bool _rangeMouseDragging = false;
        private double _doubleClickCurrentTick = 999.0;
        private List<Control> _hoveringRangePoints = new();
        private List<Control> _rangePointNodes = new();

        public override void _Ready()
        {
            ResizeController resizeController = GetNode<ResizeController>("/root/Editor/Controllers/ResizeController");
            resizeController.WindowResized += OnWindowResize;

            CreatePoint(new Vector2(0.0f, 0.5f));
            CreatePoint(new Vector2(1.0f, 0.5f));

            UpdateLines();
        }

        public override void _Process(double delta)
        {
            if (_rangeExpanded)
            {
                ProcessDoubleClick(delta);
                ProcessPointDragging();
                ProcessPointRemoving();
            }
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
                SetRangeContextPosition(_currentHoldingPoint, mousePos);

                UpdateLines();
            }
        }

        private void ProcessDoubleClick(double delta)
        {
            _doubleClickCurrentTick += delta;

            if (Input.IsActionJustPressed("mouse_click"))
            {
                Vector2 mousePos = GetMousePositionInRangeContext();
                if (IsInsideRangeContext(mousePos))
                {
                    if (_doubleClickCurrentTick < DoubleClickThreshold)
                    {
                        CreatePoint(mousePos, isSelected: true);
                        _doubleClickCurrentTick = 9999.0;
                    }
                    else
                    {
                        _doubleClickCurrentTick = 0.0;
                    }
                }
            }
        }

        private void ProcessPointRemoving()
        {
            if (_hoveringRangePoints.Count > 0)
            {
                if (Input.IsActionJustPressed("mouse_right_click"))
                {
                    var removingNode = _hoveringRangePoints.Last();

                    _hoveringRangePoints.Remove(removingNode);
                    _hoveringRangePoints.Remove(removingNode); //Just in case when adding with CreatePoint() it has 2 copies
                    _rangePointNodes.Remove(removingNode);

                    removingNode.QueueFree();

                    _rangeMouseDragging = false;

                    UpdateLines();
                }
            }
        }

        private void CreatePoint(Vector2 pointPos, bool isSelected = false)
        {
            RangePoint pointNode = RangePointObj.Instantiate<RangePoint>();
            pointNode.MouseEntered += () => RangePointMouseEntered(pointNode);
            pointNode.MouseExited += () => RangePointMouseExited(pointNode);

            SetRangeContextPosition(pointNode, pointPos);

            RangePointsGroupNode.AddChild(pointNode);
            _rangePointNodes.Add(pointNode);

            if (isSelected)
            {
                _hoveringRangePoints.Add(pointNode);
            }

            UpdateLines();
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

            if (pointList.Count > 0)
            {
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
            }
            else
            {
                pointList.Insert(0, new Vector2
                {
                    X = 0.0f,
                    Y = 0.5f,
                });

                pointList.Add(new Vector2
                {
                    X = 1.0f,
                    Y = 0.5f,
                });
            }

            return pointList;
        }

        private void SetRangeContextPosition(Control controlNode, Vector2 pointPos)
        {
            controlNode.AnchorRight = pointPos.X;
            controlNode.AnchorLeft = pointPos.X;
            controlNode.AnchorBottom = pointPos.Y;
            controlNode.AnchorTop = pointPos.Y;
        }

        private bool IsInsideRangeContext(Vector2 pointPos)
        {
            if (
                pointPos.X > 0.0 && pointPos.X < 1.0 &&
                pointPos.Y > 0.0 && pointPos.Y < 1.0
                )
                return true;
            else
                return false;
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
            _hoveringRangePoints.Remove(node); //Remove again when adding points
        }
    }
}