namespace Editor
{
    public class BaseComponent
    {
        protected LookupHelper _lookupHelper;

        public IModifier GetModifier(ModifierID modifierID)
        {
            return _lookupHelper.GetModifier(modifierID);
        }
    }
}