using Godot;

namespace Editor
{
    public class ModifierPosition : IModifier
    {
        required public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public Vector2 Position { get; set; } = new(400, 200);
    }
}
