using System;
using System.Collections.Generic;
using Godot;

namespace Editor
{
    public class BulletPool
    {
        private Node2D _poolGroupNode;
        private PackedScene _bulletObj;
        private int _poolSize;
        private Node2D[] _bulletPool;

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
                    GD.Print("Bullet created");
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
    }
}