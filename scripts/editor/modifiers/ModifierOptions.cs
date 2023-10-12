using System.Collections.Generic;

namespace Editor
{
    public class ModifierOptions : IModifier
    {
        public ModifierID ID { get; set; }
        public bool Active { get; set; } = false;
        public List<Option> Options { get; set; }
        public Option SelectedOption { get; set; }
    }
}