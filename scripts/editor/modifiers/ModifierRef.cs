namespace Editor
{
    public class ModifierRef : IModifier
    {
        public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
    }
}