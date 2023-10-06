using Godot;

namespace Editor
{
    public interface IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
    }
}
