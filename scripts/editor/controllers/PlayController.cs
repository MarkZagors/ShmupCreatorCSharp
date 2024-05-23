using ExtensionMethods;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Editor
{
    public partial class PlayController : Node
    {
        [Signal] public delegate void UpdateEventHandler();
        [Signal] public delegate void PhaseChangeEventHandler();
        [Signal] public delegate void UpdateTimelineEventHandler();
        [Export] public double ScrollTick { get; private set; } = 0.25;
        [Export] public SequenceController SequenceController { get; private set; }
        [Export] public SavingManager SavingManager { get; private set; }
        [Export] public Label TimelineTimeLabel { get; private set; }
        [Export] public VBoxContainer LanesNode { get; private set; }
        [Export] public PackedScene SequenceIconObj { get; private set; }
        [Export] public PackedScene PhasesButtonObj { get; private set; }
        [Export] public Control ComponentBodyContainer { get; private set; }
        [Export] public Control StartingLanesBorderNode { get; private set; }
        [Export] public Control LanesMainControllerContainer { get; private set; }
        [Export] public Control LanesClickZone { get; private set; }
        [Export] public Control LaneClickLine { get; private set; }
        [Export] public Control LoopSeparatorNode { get; private set; }
        [Export] public Control PhasesContainer { get; private set; }
        public bool Playing { get; private set; } = false;
        public double Time { get; private set; } = 0.0;
        public List<Phase> PhasesList { get; private set; } = new();
        public double LanesWidth { get; private set; } = 5.0;
        private Phase _selectedPhase = null;
        private int selectedPhaseIndex = -1;
        private bool _mouseOverContainerScroll = false;
        private bool _mouseOverSequence = false;
        private bool _mouseOverLoopNode = false;
        private bool _loopNodeDragging = false;

        public override void _Ready()
        {
            LoadLevel();
            UpdateUI();

            SavingManager.OnSave += SaveLevel;
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("editor_play"))
            {
                TogglePlaying();
            }

            ProcessLaneHoverAndClick();
            ProcessLoopHoverDragging();

            if (!IsMouseOverBodyContainer())
            {

                if (Input.IsActionJustPressed("editor_scrollup"))
                {
                    ScrollTime(ScrollDirection.FORWARD);
                    UpdateUI();
                }

                if (Input.IsActionJustPressed("editor_scrolldown"))
                {
                    ScrollTime(ScrollDirection.BACKWARD);
                    UpdateUI();
                }
            }

            if (Playing)
            {
                ProcessPlay(delta);
                UpdateUI();
            }
        }

        public void AddSequence()
        {
            if (CheckIfSequenceOverlap()) return;

            var sequenceNode = SequenceIconObj.Instantiate<Control>();
            var laneOne = LanesNode.GetChild<Control>(0);
            var newSequence = new Sequence
            {
                Time = this.Time,
                Node = sequenceNode,
                Components = new List<IComponent>(),
            };

            var sequenceList = GetSequenceList();
            sequenceList.Add(newSequence);
            AddSequenceButtonInLane(newSequence, sequenceNode, laneOne);
            GD.Print($"Sequence added at time {Time:0.0}");

            UpdateUI();
            EmitSignal(SignalName.Update);
        }

        private void AddSequenceButtonInLane(Sequence sequence, Control sequenceNode, Control lane)
        {
            sequenceNode.GetNode<Button>("Button").Pressed += () => ClickSequence(sequence);
            sequenceNode.GetNode<Button>("Button").MouseEntered += () => OnSequenceHover();
            sequenceNode.GetNode<Button>("Button").MouseExited += () => OnSequenceUnHover();

            lane.AddChild(sequenceNode);
        }

        private bool CheckIfSequenceOverlap()
        {
            foreach (Sequence sequence in GetSequenceList())
            {
                if (Math.Abs(sequence.Time - Time) < 0.1)
                {
                    GD.Print($"Sequence overlaps at time {Time:0.0}! Not adding");
                    return true;
                }
            }
            return false;
        }

        private void ClickSequence(Sequence sequence)
        {
            SequenceController.OpenSequence(sequence);
        }

        private bool IsMouseOverBodyContainer()
        {
            var mousePos = GetViewport().GetMousePosition();
            var componentBodyRect = ComponentBodyContainer.GetGlobalRect();
            return componentBodyRect.HasPoint(mousePos);
        }

        private bool IsMouseOverLanesConroller()
        {
            var mousePos = GetViewport().GetMousePosition();
            var lanesClickZoneRect = LanesClickZone.GetGlobalRect();
            return lanesClickZoneRect.HasPoint(mousePos);
        }

        private Vector2 GetMousePositionInLanesContext()
        {
            //TODO: refactor into static method
            var mousePos = LanesMainControllerContainer.GetLocalMousePosition();
            return new Vector2
            {
                X = (float)Mathf.Clamp(mousePos.X / LanesMainControllerContainer.Size.X, 0.0, 1.0),
                Y = (float)Mathf.Clamp(mousePos.Y / LanesMainControllerContainer.Size.Y, 0.0, 1.0),
            };
        }

        private void ProcessLaneHoverAndClick()
        {
            if (IsMouseOverLanesConroller() && !_mouseOverSequence && !_mouseOverLoopNode)
            {
                Vector2 mousePos = GetMousePositionInLanesContext();
                LaneClickLine.Visible = true;
                LaneClickLine.AnchorLeft = mousePos.X;
                LaneClickLine.AnchorRight = mousePos.X;

                double timePos = ((mousePos.X - 0.5) * 2 * LanesWidth) + Time;
                LaneClickLine.GetNode<Label>("TimeLabel").Text = $"{timePos:0.00}";

                if (Input.IsActionJustPressed("mouse_click"))
                {
                    Time = timePos;
                    UpdateUI();
                    EmitSignal(SignalName.Update);
                }
            }
            else
            {
                LaneClickLine.Visible = false;
            }
        }

        private void ProcessLoopHoverDragging()
        {
            if (_mouseOverLoopNode && Input.IsActionJustPressed("mouse_click"))
            {
                _loopNodeDragging = true;
            }

            if (Input.IsActionJustReleased("mouse_click"))
            {
                _loopNodeDragging = false;
            }

            if (_loopNodeDragging)
            {
                Vector2 mousePos = GetMousePositionInLanesContext();
                double timePos = ((mousePos.X - 0.5) * 2 * LanesWidth) + Time;
                if (timePos < 0.0f) timePos = 0.0;
                _selectedPhase.LoopTime = (double)timePos;
                UpdateUI();
            }
        }

        private void OnSequenceHover()
        {
            _mouseOverSequence = true;
        }

        private void OnSequenceUnHover()
        {
            _mouseOverSequence = false;
        }

        public void OnLoopSeparatorHover()
        {
            _mouseOverLoopNode = true;
            LoopSeparatorNode.GetNode<ColorRect>("SelectionRectVisible").Visible = true;
        }

        public void OnLoopSeparatorUnhover()
        {
            _mouseOverLoopNode = false;
            LoopSeparatorNode.GetNode<ColorRect>("SelectionRectVisible").Visible = false;
        }

        //=================
        //TIME SEQUENCE
        //=================
        private void ProcessPlay(double delta)
        {
            Time += delta;
            EmitSignal(SignalName.Update);
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
                Time += ScrollTick;
            }

            else if (scrollDirection == ScrollDirection.BACKWARD)
            {
                Time -= ScrollTick;
                Time = Math.Max(Time, 0.0);
            }
            EmitSignal(SignalName.Update);
        }
        //=================
        //PHASES
        //=================
        public void OnPhasesButtonClick()
        {
            if (PhasesContainer.Visible)
                PhasesContainer.Visible = false;
            else
                ShowPhaseButtons();
        }

        private void SetupPhases()
        {
            PhasesList.Add(new Phase(
                name: "New Phase",
                id: 0,
                health: 100,
                loopTime: 1.0,
                sequenceList: new List<Sequence>()
            ));
            selectedPhaseIndex = 0;
            UpdateSelectedPhase(0);
        }

        private void ShowPhaseButtons()
        {
            PhasesContainer.Visible = true;
            VBoxContainer phasesVBox = PhasesContainer.GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");

            foreach (var child in phasesVBox.GetChildren())
            {
                child.QueueFree();
            }

            foreach (Phase phase in PhasesList)
            {
                Control phasesButton = PhasesButtonObj.Instantiate<Control>();
                Button phasesSelectButton = phasesButton.GetNode<Button>("SelectButton");
                phasesSelectButton.Text = $"Phase {phase.ID}: {phase.Name}";
                phasesSelectButton.Pressed += () => OnPhasesSelectButtonPressed(phase);
                phasesVBox.AddChild(phasesButton);
            }

            Button addButton = new Button();
            addButton.Text = "+";
            addButton.Pressed += OnPhasesAddButtonPressed;
            phasesVBox.AddChild(addButton);
        }

        private void OnPhasesAddButtonPressed()
        {
            PhasesList.Add(new Phase(
                name: "New Phase",
                id: PhasesList.Count,
                health: 100,
                loopTime: 1.0,
                sequenceList: new List<Sequence>()
            ));
            ShowPhaseButtons();
        }

        private void OnPhasesSelectButtonPressed(Phase phase)
        {
            PhasesContainer.Visible = false;
            ChangePhase(phase);
        }

        private void ChangePhase(Phase phase)
        {
            UpdateSelectedPhase(phase.ID);

            var laneOne = LanesNode.GetChild<Control>(0);
            foreach (var child in laneOne.GetChildren()) child.QueueFree();

            foreach (Sequence sequence in GetSequenceList())
            {
                var sequenceNode = SequenceIconObj.Instantiate<Control>();
                sequence.Node = sequenceNode;
                AddSequenceButtonInLane(sequence, sequenceNode, laneOne);
            }

            Time = 0.0;

            EmitSignal(SignalName.PhaseChange); //RESTRUCTURE BULLET LIST
            EmitSignal(SignalName.UpdateTimeline);
            UpdateUI();
        }

        private void UpdateSelectedPhase(int index)
        {
            _selectedPhase = PhasesList[index];
        }

        public List<Sequence> GetSequenceList()
        {
            return _selectedPhase.SequenceList;
        }

        //=================
        //UPDATING UI
        //=================
        private void UpdateUI()
        {
            UpdateLabels();
            UpdateSequenceUI();
        }

        private void UpdateLabels()
        {
            TimelineTimeLabel.Text = $"Time: {Time:0.00}";
        }

        private void UpdateSequenceUI()
        {
            foreach (Sequence sequence in GetSequenceList())
            {
                double anchor = 0.5 + (sequence.Time - Time) * 0.5 / LanesWidth;

                sequence.Node.AnchorLeft = (float)anchor;
                sequence.Node.AnchorRight = (float)anchor;
            }
            StartingLanesBorderNode.AnchorRight = (float)(0.5 - Time * 0.5 / LanesWidth);
            LanesClickZone.AnchorLeft = StartingLanesBorderNode.AnchorRight;

            double loopSeparatorAnchor = 0.5 + (_selectedPhase.LoopTime - Time) * 0.5 / LanesWidth;
            LoopSeparatorNode.AnchorRight = (float)loopSeparatorAnchor;
            LoopSeparatorNode.AnchorLeft = (float)loopSeparatorAnchor;
        }

        public void DeleteSequence(Sequence sequence)
        {
            sequence.Node.QueueFree();
            _selectedPhase.SequenceList.Remove(sequence);
            UpdateUI();
            EmitSignal(SignalName.PhaseChange);
            EmitSignal(SignalName.UpdateTimeline);
            EmitSignal(SignalName.Update);
        }

        //=================
        //LOADING SAVING
        //=================
        private void LoadLevel()
        {
            List<Phase> loadedPhases = SavingManager.LoadLevelPhases(TransferLayer.LevelID);
            PhasesList = loadedPhases;

            selectedPhaseIndex = 0;
            UpdateSelectedPhase(0);
            ChangePhase(_selectedPhase);
        }

        private void SaveLevel()
        {
            SavingManager.SaveLevelPhases(PhasesList, TransferLayer.LevelID);
        }
    }

}

