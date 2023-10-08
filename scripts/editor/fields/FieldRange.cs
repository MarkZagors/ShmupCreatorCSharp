using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using Godot;

namespace Editor
{
    public partial class FieldRange : Control, IField
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] double DoubleClickThreshold = 0.35;
        [Export] public Control RangeContainerNode;
        [Export] public Control RangeControllerNode;
        [Export] public Control RangePointsGroupNode;
        [Export] public LineEdit MaxLineEditNode;
        [Export] public LineEdit MidLineEditNode;
        [Export] public LineEdit MinLineEditNode;
        [Export] public Line2D RangeLineNode;
        [Export] public PackedScene RangePointObj;
        public ModifierRange RangeModifier { get; private set; }
        private bool _rangeExpanded = false;
        private bool _rangeMouseDragging = false;
        private double _doubleClickCurrentTick = 999.0;
        private double _previousMaxLineEditValue = 0.0;
        private double _currentMaxLineEditValue = 0.0;
        private double _previousMinLineEditValue = 0.0;
        private double _currentMinLineEditValue = 0.0;
        private Control _currentHoldingPoint;
        private Control _selectedLineEdit = null;
        private double _selectedLineEditTick = 500.0;
        private List<Control> _hoveringRangePoints = new();
        private List<Control> _rangePointNodes = new();
        private List<Vector2> _expandedPointListCache = new();
        private readonly char[] _lineEditAllowedCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-' };

        public void Init(ModifierRange rangeModifier)
        {
            RangeModifier = rangeModifier;

            _previousMaxLineEditValue = RangeModifier.Range.Max.Value;
            _currentMaxLineEditValue = RangeModifier.Range.Max.Value;
            _previousMinLineEditValue = RangeModifier.Range.Min.Value;
            _currentMinLineEditValue = RangeModifier.Range.Min.Value;
            MaxLineEditNode.Text = RangeModifier.Range.Max.Value.ToString();
            MinLineEditNode.Text = RangeModifier.Range.Min.Value.ToString();
            UpdateMidLine(RangeModifier.Range.Max.Value, RangeModifier.Range.Min.Value);

            MaxLineEditNode.TextChanged += (newText) => OnLineEditTextChangeMax(newText);
            MidLineEditNode.TextChanged += (newText) => OnLineEditTextChangeMid(newText);
            MinLineEditNode.TextChanged += (newText) => OnLineEditTextChangeMin(newText);

            var modifierRangePointsPhysical = RangeModifier.Range.Points;
            modifierRangePointsPhysical.RemoveAt(0);
            modifierRangePointsPhysical.RemoveAt(modifierRangePointsPhysical.Count - 1);
            foreach (Vector2 point in modifierRangePointsPhysical)
            {
                CreatePoint(point);
            }

            GetNode<Label>("FieldName").Text = ModifierNamer.Get(rangeModifier.ID);

            UpdateLines();
        }

        public override void _Ready()
        {
            ResizeController resizeController = GetNode<ResizeController>("/root/Editor/Controllers/ResizeController");
            resizeController.WindowResized += OnWindowResize;
            GetViewport().GuiFocusChanged += OnFocusChanged;
        }

        public override void _Process(double delta)
        {
            if (_rangeExpanded)
            {
                ProcessDoubleClick(delta);
                ProcessPointDragging();
                ProcessPointRemoving();
                ProcessUnselectLineEdit(delta);
            }
        }

        //=================
        //RangeController
        //=================
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

            bool listsEqual = _expandedPointListCache.SequenceEqual(expandedPointList);

            _expandedPointListCache = expandedPointList;

            if (!listsEqual)
                UpdateRange(_currentMaxLineEditValue, _currentMinLineEditValue, expandedPointList);
        }

        public void ToggleRange()
        {
            if (_rangeExpanded)
            {
                CustomMinimumSize = new Vector2(0, 50);
                RangeContainerNode.Visible = false;
                _rangeExpanded = false;
            }
            else
            {
                CustomMinimumSize = new Vector2(0, 200);
                RangeContainerNode.Visible = true;
                _rangeExpanded = true;
                UpdateLines();
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

                Vector2 lastPointNonExpanded = pointList[^1];
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


        //=================
        //LineEdit Controllers
        //=================
        private void OnLineEditTextChange(string newText, LineEdit lineEdit)
        {
            if (newText.Length == 0) return;

            char lastCharacter = newText[^1];
            string modified = new(newText.Where(c => _lineEditAllowedCharacters.Contains(c)).ToArray());

            // Remove - except the first one
            if (modified.StartsWith('-'))
                modified = "-" + modified[1..].Replace("-", "");
            else
                modified = modified.Replace("-", "");

            // Remove . except the first one
            int index = modified.IndexOf('.');
            if (index >= 0)
            {
                for (int i = index + 1; i < modified.Length; i++)
                {
                    if (modified[i] == '.')
                    {
                        modified = modified.Remove(i, 1);
                        i--;
                    }
                }
            }

            lineEdit.Text = modified;
            lineEdit.CaretColumn = 999;
        }

        private double ParseLineEditValue(LineEdit lineEdit)
        {
            double value = 0.0;
            if (lineEdit.Text.Length != 0)
            {
                if (!Double.TryParse(lineEdit.Text, out value))
                {
                    // - in the first place
                    value = 0.0;
                }
            }
            return value;
        }

        private void UpdateMidLine(double maxValue, double minValue)
        {
            MidLineEditNode.Text = (minValue + ((maxValue - minValue) / 2)).ToString();
        }

        private void OnLineEditTextChangeMid(string newText)
        {
            OnLineEditTextChange(newText, MidLineEditNode);
            double value = ParseLineEditValue(MidLineEditNode);
            double valuesDiff = (_previousMaxLineEditValue - _previousMinLineEditValue) / 2;

            _currentMaxLineEditValue = value + valuesDiff;
            _currentMinLineEditValue = value - valuesDiff;

            MaxLineEditNode.Text = (value + valuesDiff).ToString();
            MinLineEditNode.Text = (value - valuesDiff).ToString();

            UpdateRange(_currentMaxLineEditValue, _currentMinLineEditValue, _expandedPointListCache);
        }

        private void OnLineEditTextChangeMax(string newText)
        {
            OnLineEditTextChange(newText, MaxLineEditNode);
            double value = ParseLineEditValue(MaxLineEditNode);

            _previousMaxLineEditValue = value;

            if (value < _previousMinLineEditValue)
            {
                MinLineEditNode.Text = value.ToString();
                _currentMaxLineEditValue = value;
                _currentMinLineEditValue = value;
                UpdateMidLine(value, value);
            }
            else
            {
                MinLineEditNode.Text = _previousMinLineEditValue.ToString();
                _currentMaxLineEditValue = value;
                _currentMinLineEditValue = _previousMinLineEditValue;
                UpdateMidLine(value, _previousMinLineEditValue);
            }

            UpdateRange(_currentMaxLineEditValue, _currentMinLineEditValue, _expandedPointListCache);
        }

        private void OnLineEditTextChangeMin(string newText)
        {
            OnLineEditTextChange(newText, MinLineEditNode);
            double value = ParseLineEditValue(MinLineEditNode);

            _previousMinLineEditValue = value;

            if (value > _previousMaxLineEditValue)
            {
                MaxLineEditNode.Text = value.ToString();
                _currentMaxLineEditValue = value;
                _currentMinLineEditValue = value;
                UpdateMidLine(value, value);
            }
            else
            {
                MaxLineEditNode.Text = _previousMaxLineEditValue.ToString();
                _currentMaxLineEditValue = _previousMaxLineEditValue;
                _currentMinLineEditValue = value;
                UpdateMidLine(_previousMaxLineEditValue, value);
            }

            UpdateRange(_currentMaxLineEditValue, _currentMinLineEditValue, _expandedPointListCache);
        }

        private void OnFocusChanged(Control control)
        {
            if (
                control.GetInstanceId() == MaxLineEditNode.GetInstanceId() ||
                control.GetInstanceId() == MidLineEditNode.GetInstanceId() ||
                control.GetInstanceId() == MinLineEditNode.GetInstanceId()
            )
            {
                _selectedLineEdit = (LineEdit)control;
                _selectedLineEditTick = 0.0;
            }
        }

        private void ProcessUnselectLineEdit(double delta)
        {
            _selectedLineEditTick += delta;

            if (Input.IsActionJustPressed("mouse_click"))
            {
                if (_selectedLineEdit != null && _selectedLineEditTick > 0.5)
                    _selectedLineEdit.ReleaseFocus();
            }
        }

        private void UpdateRange(double max, double min, List<Vector2> pointList)
        {
            RangeModifier.Range.Max.Value = max;
            RangeModifier.Range.Min.Value = min;
            RangeModifier.Range.Points = pointList;
            EmitSignal(SignalName.Update);
        }
    }
}