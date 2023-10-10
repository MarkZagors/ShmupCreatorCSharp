using Godot;

namespace Editor
{
    public class BulletData
    {
        public Node2D Node { get; set; } = null;
        public Vector2 Position { get; set; } = new();
        public float Angle { get; set; } = 0;
        public float Speed { get; set; } = 0;
        public float Size { get; set; } = 1.0f;
    }
}