namespace Editor
{
    public class ModifierRange : IModifier
    {
        required public ModifierID ID { get; set; }
        public ModifierType Type { get; set; } = ModifierType.RANGE;
        required public Range Range { get; set; }
        public bool Active { get; set; } = false;
        public bool IsMovementTimelineUpdating { get; set; } = false;
    }
}
