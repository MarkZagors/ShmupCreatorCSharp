namespace Editor
{
    public interface IModifier
    {
        public ModifierID ID { get; set; }
        public ModifierType Type { get; set; }
        public bool Active { get; set; }
    }
}
