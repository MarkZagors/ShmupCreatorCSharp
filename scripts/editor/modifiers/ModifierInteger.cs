namespace Editor
{
    public class ModifierInteger : IModifier
    {
        required public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public int Value { get; set; } = 0;
        public int MaxValue { get; set; } = 5000;
        public bool IsStructureChanging { get; set; } = false; //Can this field change how many bullets are in the system?
    }
}