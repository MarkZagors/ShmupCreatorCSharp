namespace Editor
{
    public class ModifierDouble : IModifier
    {
        required public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public double Value { get; set; } = 0;
        public double MaxValue { get; set; } = 5000;
        public bool IsStructureChanging { get; set; } = false;
    }
}