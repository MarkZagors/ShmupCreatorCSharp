using System.Collections.Generic;
using Godot;

namespace Editor
{
    public partial class PlayStateController : Node
    {
        [Signal] public delegate void UpdateEventHandler();
        [Export] public SavingManager SavingManager { get; private set; }
        public List<Phase> PhasesList { get; private set; } = new();
        public double Time { get; private set; } = 0.0;
        private Phase _selectedPhase = null;

        public override void _Ready()
        {
            LoadLevel();
        }

        public override void _Process(double delta)
        {
            Time += delta;
            EmitSignal(SignalName.Update);
        }

        private void LoadLevel()
        {
            List<Phase> loadedPhases = SavingManager.LoadLevelPhases(TransferLayer.LevelID);
            PhasesList = loadedPhases;
            UpdateSelectedPhase(0);
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
