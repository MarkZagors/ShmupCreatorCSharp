using Godot;
using System;
using System.Collections.Generic;

namespace Logic
{
    public partial class BulletController : Node
    {
        [Export] public Node2D BulletPoolNode { get; private set; }
        [Export] public PackedScene BulletNodeObj { get; private set; }
        [Export] public int BulletPoolSize { get; private set; } = 100;

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
        }


    }
}
