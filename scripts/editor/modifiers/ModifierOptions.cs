using System.Collections.Generic;

namespace Editor
{
    public class ModifierOptions : IModifier
    {
        required public ModifierID ID { get; set; }
        public ModifierType Type { get; set; } = ModifierType.OPTIONS;
        required public List<Option> Options { get; set; }
        required public Option SelectedOption { get; set; }
        public bool Active { get; set; } = false;
    }
}
