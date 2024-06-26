using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Editor
{
    public partial class PlayStateController : Node
    {
        [Signal] public delegate void UpdateEventHandler();
        [Signal] public delegate void UpdateTimelineEventHandler();
        [Signal] public delegate void PhaseChangeEventHandler();
        [Signal] public delegate void OnWinEventHandler();
        [Export] public SavingManager SavingManager { get; private set; }
        [Export] public Sprite2D BossSprite { get; private set; }
        [Export] public Node2D BulletPoolNode { get; private set; }
        [Export] public HealthController HealthController { get; private set; }
        [Export] public Control WinScreen { get; private set; }
        [Export] public Control LoseScreen { get; private set; }
        [Export] public AudioStreamPlayer MusicPlayer { get; private set; }
        public List<Phase> PhasesList { get; private set; } = new();
        public double Time { get; private set; } = 0.0;
        public PlayState PlayState { get; private set; } = PlayState.ENTERING;
        private Phase _selectedPhase = null;
        private int _selectedPhaseId = 0;
        private Vector2 _transitionStartingPos;
        private Vector2 _transitionEndPos;
        private double _transitionSpeed = 1.5;
        private List<BulletPlay> _protectedBulletList = new(); //used for continuin movements after transitions
        private List<BulletPlay> _protectedRemoveList = new(); //used for continuin movements after transitions
        private Rect _windowRect = new(-100, -100, 968, 1224);

        public override void _Ready()
        {
            LoadLevel();

            _transitionStartingPos = new Vector2(400, -200);
            _transitionEndPos = new Vector2(400, 200);

            HealthController.OnEnemyDeath += OnEnemyDeath;
            HealthController.OnPlayerDeath += OnPlayerDeath;
        }

        public override void _Process(double delta)
        {
            //Used for continuing bullet movements after transitions
            ContinueProtectedBulletListMovement(delta);

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
                    CreateProtectedBulletsList();
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
            else if (PlayState == PlayState.PHASE_TRANSITION)
            {
                LerpPosition();
                Time += delta;
                if (Time > _transitionSpeed)
                {
                    Time = 0.0;
                    PlayState = PlayState.MAIN;
                }
            }
            else if (PlayState == PlayState.END_WIN)
            {
            }
        }

        private void CreateProtectedBulletsList()
        {
            foreach (BulletPlay node in BulletPoolNode.GetChildren())
            {
                if (node.Visible && !_protectedBulletList.Contains(node))
                {
                    _protectedBulletList.Add(node);
                    node.IsClearProtected = true;
                    if (node.BulletData != null)
                    {
                        node.BulletData.Node = null;
                        node.BulletData = null;
                    }
                }
            }
        }

        private void ContinueProtectedBulletListMovement(double delta)
        {
            foreach (BulletPlay node in _protectedBulletList)
            {
                bool isBulletInBorder = BulletPool.BorderCheckPosition(node.Position, _windowRect);
                if (!isBulletInBorder)
                {
                    _protectedRemoveList.Add(node);
                }
                // GD.Print(node.Velocity.Length());
                node.Position += node.Velocity * (float)delta;
            }
            RemoveOutOfBorderBulletsFromProtectedList();
        }

        private void RemoveOutOfBorderBulletsFromProtectedList()
        {
            foreach (BulletPlay node in _protectedRemoveList)
            {
                node.Visible = false;
                Area2D hitboxArea = node.GetNode<Area2D>("Hitbox");
                hitboxArea.QueueFree();
                _protectedBulletList.Remove(node);
                // GD.Print("removed");
            }
            _protectedRemoveList.Clear();
        }

        private void OnEnemyDeath()
        {
            if (PlayState == PlayState.MAIN)
            {
                if (_selectedPhaseId + 1 >= PhasesList.Count)
                {
                    PlayState = PlayState.END_WIN;
                    WinScreen.Visible = true;
                    BossSprite.Visible = false;
                    EmitSignal(SignalName.OnWin);
                    return;
                }
                _transitionStartingPos = BossSprite.Position;

                ChangePhase();
                PlayState = PlayState.PHASE_TRANSITION;
            }
        }

        private void OnPlayerDeath()
        {
            LoseScreen.Visible = true;
        }

        private void ChangePhase()
        {
            _selectedPhaseId += 1;
            UpdateSelectedPhase(_selectedPhaseId);

            Time = 0.0;

            EmitSignal(SignalName.PhaseChange); //RESTRUCTURE BULLET LIST
            EmitSignal(SignalName.UpdateTimeline);
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

            // Dictionary loadedIndex = SavingManager.LoadLevelIndex(TransferLayer.LevelID);
            // bool isMusicAdded = (string)loadedIndex["isMusicAdded"] == "True";
            // if (isMusicAdded)
            // {
            //     GD.Print("playMusic");
            //     MusicPlayer.Stream = SavingManager.LoadMusic(TransferLayer.LevelID);
            //     MusicPlayer.Play();
            // }
        }

        public List<Sequence> GetSequenceList()
        {
            return _selectedPhase.SequenceList;
        }

        private void UpdateSelectedPhase(int index)
        {
            _selectedPhase = PhasesList[index];
        }

        private void GotoMainMenu()
        {
            GetTree().ChangeSceneToFile("res://scenes/pages/LevelPicker.tscn");
        }

        private void RestartScene()
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
