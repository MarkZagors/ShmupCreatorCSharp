using System.Collections.Generic;
using System.Linq;

namespace Editor
{
    public class LookupHelper
    {
        private readonly Dictionary<ModifierID, IModifier> _modifiersLookup;

        public LookupHelper(List<IModifier> modifiers)
        {
            _modifiersLookup = modifiers.ToDictionary(modifier => modifier.ID);
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

        public T GetRefComponent<T>(ModifierID modifierID)
        {
            return (T)((ModifierRef)GetModifier(modifierID)).Ref;
        }
    }
}