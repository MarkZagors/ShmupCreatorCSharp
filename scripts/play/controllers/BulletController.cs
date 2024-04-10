using ExtensionMethods;
using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Editor
{
    public partial class BulletController : Node
    {
        [Export] public PlayController PlayController { get; private set; }
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public SequenceController SequenceController { get; private set; }
        [Export] public BossMovementController BossMovementController { get; private set; }
        [Export] public Node2D BulletPoolNode { get; private set; }
        [Export] public PackedScene BulletNodeObj { get; private set; }
        [Export] public Sprite2D BossSprite { get; private set; }
        [Export] public int BulletPoolSize { get; private set; } = 100;
        private readonly List<Spawner> _spawnerList = new();
        private Vector2 _bossPosition = new(400, 200);
        private BulletPool _bulletPool;
        private Rect _windowRect = new(-100, -100, 968, 1224);

        public override void _Ready()
        {
            _bulletPool = new BulletPool(
                poolGroupNode: BulletPoolNode,
                bulletObj: BulletNodeObj,
                poolSize: 4096,
                expansionCount: 5
            );

            PlayController.Update += Update;
            PlayController.PhaseChange += RestructureBulletList;
            ComponentsController.Update += Update;
            SequenceController.Update += Update;

            ComponentsController.OnValidRestructure += RestructureBulletList;
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
            BulletPool.ClearSpawner(spawner);

            double sequenceSpawnTime = PlayController.Time - spawner.Component.Sequence.Time;
            if (sequenceSpawnTime < 0.0)
            {
                return;
            }

            ComponentBundle bundle = spawner.Component.GetBundleComponent();
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
                    bulletIndex: i,
                    sequenceSpawnTime: sequenceSpawnTime,
                    spawner: spawner,
                    pointArray: pointArray,
                    timerProcessCurve: timerProcessCurve,
                    angleRange: angleRange,
                    speedRange: speedRange,
                    sizeRange: sizeRange
                );
            }
        }

        private void UpdateBullet(
            int bulletIndex,
            double sequenceSpawnTime,
            Spawner spawner,
            double[] pointArray,
            Range timerProcessCurve,
            Range angleRange,
            Range speedRange,
            Range sizeRange
        )
        {
            var bulletSpawnTimeOffset = sequenceSpawnTime - spawner.Timer.TiggerOffsets[bulletIndex];
            var absoluteBulletSpawnTime = spawner.Component.Sequence.Time + spawner.Timer.TiggerOffsets[bulletIndex];
            if (bulletSpawnTimeOffset < 0.0)
            {
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

                BulletData bulletData = spawner.Bullets[bulletIndex, j];
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
                    bulletData.Node ??= _bulletPool.GetBullet();

                    bulletData.Node.Position = bulletData.Position;
                    bulletData.Node.RotationDegrees = bulletData.Angle;
                    bulletData.Node.Scale = new Vector2(bulletData.Size, bulletData.Size);
                }
            }
        }

        private void UpdateBossPosition()
        {
            Vector2 position = BossMovementController.GetPosition((float)PlayController.Time);
            _bossPosition = position;
            BossSprite.Position = _bossPosition;
        }

        private void RestructureBulletList()
        {
            _spawnerList.Clear();

            foreach (Sequence sequence in PlayController.GetSequenceList())
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
