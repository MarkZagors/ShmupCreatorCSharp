using Godot;

namespace Editor
{
    public class ComponentBullet : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
    }
}
