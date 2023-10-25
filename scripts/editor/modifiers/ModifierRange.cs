namespace Editor
{
    public class ModifierRange : IModifier
    {
        required public ModifierID ID { get; set; }
        required public Range Range { get; set; }
        public bool Active { get; set; } = false;
    }
}
