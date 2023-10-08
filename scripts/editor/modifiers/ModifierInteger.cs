namespace Editor
{
    public class ModifierInteger : IModifier
    {
        public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public int Value { get; set; } = 0;
        public int MaxValue { get; set; } = 5000;
    }
}