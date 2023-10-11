using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentTimer : IComponent
    {
        public string Name { get; set; }
        public TreeItem TreeItem { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public Enums.ComponentType Type { get; set; } = Enums.ComponentType.TIMER;
        private readonly Dictionary<ModifierID, IModifier> _modifiersLookup;

        public ComponentTimer(string name, TreeItem treeItem)
        {
            Name = name;
            TreeItem = treeItem;
            Modifiers = new List<IModifier>
            {
                //Modifiers here
            };
            _modifiersLookup = Modifiers.ToDictionary(modifier => modifier.ID);
        }

        public IModifier GetModifier(ModifierID modifierID)
        {
            if (_modifiersLookup.ContainsKey(modifierID))
            {
                return _modifiersLookup[modifierID];
            }
            else
            {
                return null;
            }
        }
    }
}
