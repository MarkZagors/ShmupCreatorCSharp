namespace Editor
{
    public class ModifierRange : IModifier
    {
        public ModifierID ID { get; set; }
        public Range Range { get; set; }
        public bool Active { get; set; } = false;
    }
}
