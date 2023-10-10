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
        private int _poolSize;
        private Node2D[] _bulletPool;
        private int _headIndex = 0;
        private readonly int _expansionCount = 1;
        private int _expansionIndex = 1;

        public BulletPool(Node2D poolGroupNode, PackedScene bulletObj, int poolSize, int expansionCount)
        {
            _poolGroupNode = poolGroupNode;
            _bulletObj = bulletObj;
            _poolSize = poolSize;
            _expansionCount = expansionCount;
            _bulletPool = new Node2D[_poolSize];

            CreatePool(_poolSize);
        }

        public Node2D GetBullet()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                int index = (_headIndex + i) % _poolSize;
                if (!_bulletPool[index].Visible)
                {
                    _bulletPool[index].Visible = true;
                    _headIndex = index + 1;
                    return _bulletPool[index];
                }
            }
            return ExpandPoolSize();
        }

        private void CreatePool(int n)
        {
            for (int i = 0; i < n; i++)
            {
                CreateBulletNode(i);
            }
        }

        private void CreateBulletNode(int index)
        {
            Node2D bullet = _bulletObj.Instantiate<Node2D>();
            bullet.Visible = false;
            _poolGroupNode.AddChild(bullet);

            _bulletPool[index] = bullet;
        }

        private Node2D ExpandPoolSize()
        {
            if (_expansionCount == _expansionIndex)
            {
                //Expansion folds limit reached
                throw new IndexOutOfRangeException("Bullet Pool is overflown!");
            }

            _expansionIndex += 1;
            _poolSize *= 2;

            //Double the capacity and copy current values
            Node2D[] _newBulletPool = new Node2D[_poolSize];
            for (int i = 0; i < _poolSize / 2; i++)
            {
                _newBulletPool[i] = _bulletPool[i];
            }
            _bulletPool = _newBulletPool;

            //Create new bullet nodes on the other half
            for (int i = _poolSize / 2; i < _poolSize; i++)
            {
                CreateBulletNode(i);
            }

            _headIndex = _poolSize / 2;

            GD.Print($"Bullet pool expanded, pool size:  {_poolSize}");

            return _bulletPool[_poolSize / 2];
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