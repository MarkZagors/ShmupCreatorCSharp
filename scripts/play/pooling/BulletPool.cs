using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class BulletPool
    {
        private readonly Node2D _poolGroupNode;
        private readonly PackedScene _bulletObj;
        private readonly int _poolSize;
        private readonly Node2D[] _bulletPool;

        public BulletPool(Node2D poolGroupNode, PackedScene bulletObj, int poolSize)
        {
            _poolGroupNode = poolGroupNode;
            _bulletObj = bulletObj;
            _poolSize = poolSize;
            _bulletPool = new Node2D[_poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                Node2D bullet = _bulletObj.Instantiate<Node2D>();
                bullet.Visible = false;
                _poolGroupNode.AddChild(bullet);

                _bulletPool[i] = bullet;
            }
        }

        public Node2D GetBullet()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                if (!_bulletPool[i].Visible)
                {
                    _bulletPool[i].Visible = true;
                    GD.Print("Bullet activated");
                    return _bulletPool[i];
                }
            }
            throw new IndexOutOfRangeException("Bullet Pool is overflown!");
        }

        public void ClearPool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                _bulletPool[i].Visible = false;
            }
        }

        public static void ClearSpawner(Spawner spawner)
        {
            for (int i = 0; i < spawner.Bullets.Length; i++)
            {
                if (spawner.Bullets[i].Node != null)
                {
                    spawner.Bullets[i].Node.Visible = false;
                    spawner.Bullets[i].Node = null;
                }
            }
        }

        public static bool BorderCheck(BulletData bulletData, Rect borderRect)
        {
            if (
                bulletData.Position.X > borderRect.X + borderRect.Width ||
                bulletData.Position.X < borderRect.X ||
                bulletData.Position.Y > borderRect.Y + borderRect.Height ||
                bulletData.Position.X < borderRect.Y
            )
            {
                if (bulletData.Node != null)
                {
                    bulletData.Node.Visible = false;
                    bulletData.Node = null;
                }
                return false;
            }
            return true;
        }
    }
}