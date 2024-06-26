using ExtensionMethods;
using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Editor
{
    public partial class BulletController : Node
    {
        [Export] public PageType PageType { get; private set; } = PageType.EDITOR;
        [Export] public PlayController PlayController { get; private set; }
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public SequenceController SequenceController { get; private set; }
        [Export] public BossMovementController BossMovementController { get; private set; }
        [Export] public PlayStateController PlayStateController { get; private set; }
        [Export] public Node2D BulletPoolNode { get; private set; }
        [Export] public PackedScene BulletNodeObj { get; private set; }
        [Export] public Sprite2D BossSprite { get; private set; }
        [Export] public int BulletPoolSize { get; private set; } = 100;
        [Export] public Texture2D BulletSpriteRed;
        [Export] public Texture2D BulletSpriteGreen;
        [Export] public Texture2D BulletSpriteBlue;
        private readonly List<Spawner> _spawnerList = new();
        private Vector2 _bossPosition = new(400, 200);
        private BulletPool _bulletPool;
        private Rect _windowRect = new(-100, -100, 968, 1224);

        //THIS USES ENTER TREE -----
        //THIS MIGHT CAUSE SOME CONNECTION OR ON READY ERRORS
        public override void _EnterTree()
        {
            _bulletPool = new BulletPool(
                poolGroupNode: BulletPoolNode,
                bulletObj: BulletNodeObj,
                poolSize: 4096,
                expansionCount: 5
            );
            if (PageType == PageType.EDITOR)
            {
                PlayController.PhaseChange += RestructureBulletList;
            }
            else if (PageType == PageType.PLAY)
            {
                PlayStateController.Update += Update;
                PlayStateController.PhaseChange += RestructureBulletList;
            }
        }

        public override void _Ready()
        {
            if (PageType == PageType.EDITOR)
            {
                PlayController.Update += Update;
                ComponentsController.Update += Update;
                SequenceController.Update += Update;

                ComponentsController.OnValidRestructure += RestructureBulletList;
            }
        }

        private void Update()
        {
            UpdateBossPosition();
            foreach (Spawner spawner in _spawnerList)
            {
                UpdateSpawner(spawner);
            }
        }

        private void UpdateSpawner(Spawner spawner)
        {
            // BulletPool.ClearSpawner(spawner);
            double controllerTime = PageType == PageType.EDITOR ? PlayController.Time : PlayStateController.Time;
            if (controllerTime < spawner.Component.Sequence.Time)
            {
                BulletPool.ClearSpawner(spawner);
            }

            double sequenceSpawnTime = controllerTime - spawner.Component.Sequence.Time;
            if (sequenceSpawnTime < 0.0)
            {
                return;
            }

            ComponentBundle bundle = spawner.Component.GetBundleComponent();
            ComponentBullet bullet = spawner.Component.GetBulletComponent();
            ComponentTimer spawnTimerComponent = spawner.Component.GetSpawnTimerComponent();

            Range angleRange =
                (bundle.GetModifier(ModifierID.BUNDLE_ANGLE) as ModifierRange)
                .Range;

            Range speedRange =
                (bundle.GetModifier(ModifierID.BUNDLE_SPEED) as ModifierRange)
                .Range;

            Range sizeRange =
                (bundle.GetModifier(ModifierID.BUNDLE_SIZE) as ModifierRange)
                .Range;

            Option bulletSpriteOption =
                (bullet.GetModifier(ModifierID.BULLET_SPRITE) as ModifierOptions)
                .SelectedOption;

            Range timerProcessCurve = null;
            if (spawnTimerComponent != null)
            {
                timerProcessCurve =
                    (spawnTimerComponent.GetModifier(ModifierID.TIMER_PROCESS_CURVE) as ModifierRange)
                    .Range;

                spawner.Timer.UpdateTimer(
                    waitTime:
                        (float)(spawnTimerComponent.GetModifier(ModifierID.TIMER_WAIT_TIME) as ModifierDouble)
                        .Value,
                    processTime:
                        (float)(spawnTimerComponent.GetModifier(ModifierID.TIMER_PROCESS_TIME) as ModifierDouble)
                        .Value
                );
            }

            double[] pointArray = GetEqualSizePoints(spawner.BulletCount);

            for (int i = 0; i < spawner.Timer.LoopCount; i++)
            {
                UpdateBullet(
                    waveIndex: i,
                    sequenceSpawnTime: sequenceSpawnTime,
                    spawner: spawner,
                    pointArray: pointArray,
                    timerProcessCurve: timerProcessCurve,
                    angleRange: angleRange,
                    speedRange: speedRange,
                    sizeRange: sizeRange,
                    bulletSpriteOption: bulletSpriteOption
                );
            }
        }

        private void UpdateBullet(
            int waveIndex,
            double sequenceSpawnTime,
            Spawner spawner,
            double[] pointArray,
            Range timerProcessCurve,
            Range angleRange,
            Range speedRange,
            Range sizeRange,
            Option bulletSpriteOption
        )
        {
            var bulletSpawnTimeOffset = sequenceSpawnTime - spawner.Timer.TiggerOffsets[waveIndex];
            var absoluteBulletSpawnTime = spawner.Component.Sequence.Time + spawner.Timer.TiggerOffsets[waveIndex];
            if (bulletSpawnTimeOffset < 0.0)
            {
                BulletPool.ClearSpawnerWave(spawner, waveIndex);
                return;
            }

            for (int j = 0; j < spawner.BulletCount; j++)
            {
                var pointX = pointArray[j];
                var bulletSpawnTime = bulletSpawnTimeOffset;

                if (timerProcessCurve != null)
                {
                    bulletSpawnTime -= timerProcessCurve.GetValueAt(pointX) * spawner.Timer.ProcessTime;
                }

                if (bulletSpawnTime < 0.0)
                {
                    //skip individual bullets if not in time frame
                    continue;
                }

                Vector2 bossPositionAtFireTime = BossMovementController.GetPosition((float)absoluteBulletSpawnTime);

                BulletData bulletData = spawner.Bullets[waveIndex, j];
                bulletData.Angle = (float)angleRange.GetValueAt(pointX);
                bulletData.Speed = (float)speedRange.GetValueAt(pointX);
                bulletData.Size = (float)sizeRange.GetValueAt(pointX);

                bulletData.Position = new Vector2(
                    (bulletData.Speed * MathF.Cos(bulletData.Angle * Calc.Deg2Rad) * (float)bulletSpawnTime) + bossPositionAtFireTime.X,
                    (bulletData.Speed * MathF.Sin(bulletData.Angle * Calc.Deg2Rad) * (float)bulletSpawnTime) + bossPositionAtFireTime.Y
                );

                bool isBulletInBorder = BulletPool.BorderCheck(bulletData, _windowRect);
                if (isBulletInBorder)
                {
                    // if (bulletData.Node != null && bulletData.Node.Visible == false)
                    // {
                    //     // reset bullet data if made invisible by PlayStateController
                    //     bulletData.Node = null;
                    // }

                    if (bulletData.Node == null)
                    {
                        bulletData.Node = _bulletPool.GetBullet();

                        switch (bulletSpriteOption)
                        {
                            case Option.SPRITE_RED:
                                bulletData.Node.GetNode<Sprite2D>("Sprite").Texture = BulletSpriteRed;
                                break;
                            case Option.SPRITE_BLUE:
                                bulletData.Node.GetNode<Sprite2D>("Sprite").Texture = BulletSpriteBlue;
                                break;
                            case Option.SPRITE_GREEN:
                                bulletData.Node.GetNode<Sprite2D>("Sprite").Texture = BulletSpriteGreen;
                                break;
                        }

                        if (bulletData.Node is BulletPlay bulletPlayNode)
                        {
                            bulletPlayNode.Velocity = new Vector2(
                                bulletData.Speed * MathF.Cos(bulletData.Angle * Calc.Deg2Rad),
                                bulletData.Speed * MathF.Sin(bulletData.Angle * Calc.Deg2Rad)
                            );
                            bulletPlayNode.BulletData = bulletData;

                            Area2D bulletHitboxArea = new Area2D();
                            CollisionShape2D bulletCollisionShape = new CollisionShape2D();
                            CircleShape2D circleShape2D = new CircleShape2D();
                            circleShape2D.Radius = 10;
                            bulletHitboxArea.Name = "Hitbox";
                            bulletCollisionShape.Shape = circleShape2D;
                            bulletHitboxArea.AddChild(bulletCollisionShape);
                            bulletPlayNode.AddChild(bulletHitboxArea);
                        }
                    }

                    bulletData.Node.Position = bulletData.Position;
                    bulletData.Node.RotationDegrees = bulletData.Angle;
                    bulletData.Node.Scale = new Vector2(bulletData.Size, bulletData.Size);
                }
            }
        }

        private void UpdateBossPosition()
        {
            double controllerTime = PageType == PageType.EDITOR ? PlayController.Time : PlayStateController.Time;
            Vector2 position = BossMovementController.GetPosition((float)controllerTime);
            _bossPosition = position;
            BossSprite.Position = _bossPosition;
        }

        private void RestructureBulletList()
        {
            GD.Print("restructure");
            _spawnerList.Clear();

            List<Sequence> sequenceList = PageType == PageType.EDITOR ? PlayController.GetSequenceList() : PlayStateController.GetSequenceList();
            foreach (Sequence sequence in sequenceList)
            {
                foreach (IComponent component in sequence.Components)
                {
                    if (component.Type == ComponentType.SPAWNER)
                    {
                        var spawnerComponent = (ComponentSpawner)component;
                        if (spawnerComponent.Valid)
                        {
                            GenerateSpawners(spawnerComponent);
                        }
                    }
                }
            }

            _bulletPool.ClearPool();

            Update();
        }

        private void GenerateSpawners(ComponentSpawner spawnerComponent)
        {
            ComponentBundle bundle = spawnerComponent.GetBundleComponent();
            ComponentTimer spawnTimerComponent = spawnerComponent.GetSpawnTimerComponent();

            int bulletCount =
                (bundle.GetModifier(ModifierID.BUNDLE_COUNT) as ModifierInteger)
                .Value;

            Timer spawnTimer;
            if (spawnTimerComponent == null)
            {
                spawnTimer = new Timer(
                    waitTime: 1.0f,
                    processTime: 0.0f,
                    loopCount: 1
                );
            }
            else
            {
                spawnTimer = new Timer(
                    waitTime:
                        (float)(spawnTimerComponent.GetModifier(ModifierID.TIMER_WAIT_TIME) as ModifierDouble)
                        .Value,
                    processTime:
                        (float)(spawnTimerComponent.GetModifier(ModifierID.TIMER_PROCESS_TIME) as ModifierDouble)
                        .Value,
                    loopCount:
                        (spawnTimerComponent.GetModifier(ModifierID.TIMER_LOOP_COUNT) as ModifierInteger)
                        .Value
                );
            }

            //Bullet data[][] = x = loop index, y = bullet index
            BulletData[,] bulletDataArray = new BulletData[spawnTimer.LoopCount, bulletCount];
            for (int i = 0; i < spawnTimer.LoopCount; i++)
            {
                for (int j = 0; j < bulletCount; j++)
                {
                    var bulletData = new BulletData { };
                    bulletDataArray[i, j] = bulletData;
                }
            }

            Spawner spawner = new(
                component: spawnerComponent,
                bullets: bulletDataArray,
                bulletCount: bulletCount,
                timer: spawnTimer
            );

            _spawnerList.Add(spawner);
        }

        private static double[] GetEqualSizePoints(int n)
        {
            double f = 0.0f;
            double distanceInterval = 1.0 / n;
            double[] pointArray = new double[n];

            for (int i = 0; i < n; i++)
            {
                if (i == 0)
                    f += distanceInterval / 2;
                else
                    f += distanceInterval;

                pointArray[i] = f;
            }
            return pointArray;
        }
    }
}
