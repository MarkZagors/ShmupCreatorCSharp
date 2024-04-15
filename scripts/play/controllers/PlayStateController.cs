using System.Collections.Generic;
using Godot;

namespace Editor
{
    public partial class PlayStateController : Node
    {
        [Signal] public delegate void UpdateEventHandler();
        [Signal] public delegate void UpdateTimelineEventHandler();
        [Signal] public delegate void PhaseChangeEventHandler();
        [Export] public SavingManager SavingManager { get; private set; }
        [Export] public Sprite2D BossSprite { get; private set; }
        public List<Phase> PhasesList { get; private set; } = new();
        public double Time { get; private set; } = 0.0;
        public PlayState PlayState { get; private set; } = PlayState.ENTERING;
        private Phase _selectedPhase = null;
        private Vector2 _transitionStartingPos;
        private Vector2 _transitionEndPos;
        private double _transitionSpeed = 1.5;

        public override void _Ready()
        {
            LoadLevel();

            _transitionStartingPos = new Vector2(400, -200);
            _transitionEndPos = new Vector2(400, 200);
        }

        public override void _Process(double delta)
        {
            if (PlayState == PlayState.ENTERING)
            {
                LerpPosition();
                Time += delta;
                if (Time > _transitionSpeed)
                {
                    Time = 0.0;
                    PlayState = PlayState.MAIN;
                }
            }
            else if (PlayState == PlayState.MAIN)
            {
                Time += delta;
                EmitSignal(SignalName.Update);
                if (Time > _selectedPhase.LoopTime)
                {
                    Time = 0.0;
                    _transitionStartingPos = BossSprite.Position;
                    PlayState = PlayState.LOOP_TRANSITION;
                }
            }
            else if (PlayState == PlayState.LOOP_TRANSITION)
            {
                LerpPosition();
                Time += delta;
                if (Time > _transitionSpeed)
                {
                    Time = 0.0;
                    PlayState = PlayState.MAIN;
                }
            }
        }

        private void LerpPosition()
        {
            float t = (float)(Time / _transitionSpeed);
            BossSprite.Position = _transitionStartingPos.Lerp(_transitionEndPos, t);
        }

        private void LoadLevel()
        {
            List<Phase> loadedPhases = SavingManager.LoadLevelPhases(TransferLayer.LevelID);
            PhasesList = loadedPhases;
            UpdateSelectedPhase(0);
            EmitSignal(SignalName.PhaseChange);
            EmitSignal(SignalName.UpdateTimeline);
        }

        public List<Sequence> GetSequenceList()
        {
            return _selectedPhase.SequenceList;
        }

        private void UpdateSelectedPhase(int index)
        {
            _selectedPhase = PhasesList[index];
        }
    }
}
