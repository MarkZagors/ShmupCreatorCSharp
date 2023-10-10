using ExtensionMethods;
using Godot;
using System;
using System.Collections.Generic;

namespace Editor
{
    public partial class BulletController : Node
    {
        [Export] public PlayController PlayController { get; private set; }
        [Export] public ComponentsController ComponentsController { get; private set; }
        [Export] public SequenceController SequenceController { get; private set; }
        [Export] public Node2D BulletPoolNode { get; private set; }
        [Export] public PackedScene BulletNodeObj { get; private set; }
        [Export] public int BulletPoolSize { get; private set; } = 100;
        private readonly List<Spawner> _spawnerList = new();
        private List<Sequence> _sequenceList;
        private Vector2 _bossPosition = new(400, 200);
        private BulletPool _bulletPool;
        private Rect _windowRect = new(0, 0, 768, 1024);

        public override void _Ready()
        {
            _bulletPool = new BulletPool(
                poolGroupNode: BulletPoolNode,
                bulletObj: BulletNodeObj,
                poolSize: 512
            );

            _sequenceList = PlayController.SequenceList;

            PlayController.Update += Update;
            ComponentsController.Update += Update;
            SequenceController.Update += Update;

            ComponentsController.OnValidRestructure += RestructureBulletList;
        }

        private void Update()
        {
            foreach (Spawner spawner in _spawnerList)
            {
                double spawnerTime = PlayController.Time - spawner.Component.Sequence.Time;
                if (spawnerTime < 0.0)
                {
                    BulletPool.ClearSpawner(spawner);
                    continue;
                }

                ComponentBundle bundle = spawner.Component.GetBundleComponent();

                Range angleRange =
                    (bundle.GetModifier(ModifierID.BUNDLE_ANGLE) as ModifierRange)
                    .Range;

                Range speedRange =
                    (bundle.GetModifier(ModifierID.BUNDLE_SPEED) as ModifierRange)
                    .Range;

                Range sizeRange =
                    (bundle.GetModifier(ModifierID.BUNDLE_SIZE) as ModifierRange)
                    .Range;

                double[] pointArray = GetEqualSizePoints(spawner.BulletCount);

                for (int i = 0; i < spawner.BulletCount; i++)
                {
                    var pointX = pointArray[i];

                    BulletData bulletData = spawner.Bullets[i];
                    bulletData.Angle = (float)angleRange.GetValueAt(pointX);
                    bulletData.Speed = (float)speedRange.GetValueAt(pointX);
                    bulletData.Size = (float)sizeRange.GetValueAt(pointX);

                    bulletData.Position = new Vector2(
                        (bulletData.Speed * MathF.Cos(bulletData.Angle * Calc.Deg2Rad) * (float)spawnerTime) + _bossPosition.X,
                        (bulletData.Speed * MathF.Sin(bulletData.Angle * Calc.Deg2Rad) * (float)spawnerTime) + _bossPosition.Y
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
        }

        private void RestructureBulletList()
        {
            _spawnerList.Clear();

            foreach (Sequence sequence in _sequenceList)
            {
                foreach (IComponent component in sequence.Components)
                {
                    if (component.Type == Enums.ComponentType.SPAWNER)
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
            int bulletCount =
                (bundle.GetModifier(ModifierID.BUNDLE_COUNT) as ModifierInteger)
                .Value;

            BulletData[] bulletDataArray = new BulletData[bulletCount];
            for (int i = 0; i < bulletCount; i++)
            {
                var bulletData = new BulletData { };
                bulletDataArray[i] = bulletData;
            }

            Spawner spawner = new(
                component: spawnerComponent,
                bullets: bulletDataArray,
                bulletCount: bulletCount
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
