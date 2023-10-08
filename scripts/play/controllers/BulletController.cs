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
        private List<BulletData> _bulletDataList = new();
        private List<Sequence> _sequenceList;

        private List<Node> _bulletPool = new();

        public override void _Ready()
        {
            for (int i = 0; i < BulletPoolSize; i++)
            {
                Node2D bullet = BulletNodeObj.Instantiate<Node2D>();
                bullet.Visible = false;
                BulletPoolNode.AddChild(bullet);

                _bulletPool.Add(bullet);
            }

            _sequenceList = PlayController.SequenceList;

            PlayController.Update += Update;
            ComponentsController.Update += Update;
            SequenceController.Update += Update;
        }

        public void Update()
        {
            GDE.PrintHere();
            GD.Print(PlayController.Time);
        }
    }
}
