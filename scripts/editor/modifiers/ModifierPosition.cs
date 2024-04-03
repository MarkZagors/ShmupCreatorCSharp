namespace Editor
{
    public class ModifierPosition : IModifier
    {
        required public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
    }
}
