using Godot;

namespace Editor
{
    public partial class ComponentRange : Control, IComponent
    {
        private bool _rangeExpanded = false;
        public void ToggleRange()
        {
            if (_rangeExpanded)
            {
                CustomMinimumSize = new Vector2(0, 50);
                _rangeExpanded = false;
            }
            else
            {
                CustomMinimumSize = new Vector2(0, 200);
                _rangeExpanded = true;
            }
        }
    }
}