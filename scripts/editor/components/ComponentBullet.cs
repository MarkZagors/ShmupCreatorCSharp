using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Editor
{
    public class ComponentBullet : BaseComponent, IComponent
    {
        public string Name { get; set; }
        public List<IModifier> Modifiers { get; set; }
        public ComponentType Type { get; set; } = ComponentType.BULLET;
        public Texture2D Icon { get; set; }

        public ComponentBullet(string name)
        {
            Name = name;
            Modifiers = new List<IModifier>
            {
                new ModifierOptions {
                    ID = ModifierID.BULLET_SPRITE,
                    IsStructureChanging = true,
                    Active = true,
                    Options = new() {
                        Option.SPRITE_RED,
                        Option.SPRITE_GREEN,
                        Option.SPRITE_BLUE
                    },
                    SelectedOption = Option.SPRITE_RED
                },
            };
            _lookupHelper = new LookupHelper(Modifiers);
        }
    }
}
