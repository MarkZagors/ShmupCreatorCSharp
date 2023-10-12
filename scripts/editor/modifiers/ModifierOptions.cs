namespace Editor
{
    public class ModifierOptions : IModifier
    {
        public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
    }
}