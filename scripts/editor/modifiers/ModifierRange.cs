namespace Editor
{
    public class ModifierRange : IModifier
    {
        public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public Range Range { get; set; }
    }
}
