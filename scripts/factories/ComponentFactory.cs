using Godot;

namespace Editor
{
    public static class ComponentFactory
    {
        public static IComponent CreateComponent(ComponentType componentType, string name, Sequence sequence)
        {
            IComponent component = null;
            switch (componentType)
            {
                case ComponentType.BULLET:
                    component = new ComponentBullet
                    (
                        name: name
                    );
                    break;
                case ComponentType.BUNDLE:
                    component = new ComponentBundle
                    (
                        name: name
                    );
                    break;
                case ComponentType.SPAWNER:
                    component = new ComponentSpawner
                    (
                        name: name,
                        sequence: sequence
                    );
                    break;
                case ComponentType.TIMER:
                    component = new ComponentTimer
                    (
                        name: name
                    );
                    break;
                case ComponentType.MOVEMENT:
                    component = new ComponentMovement
                    (
                        name: name,
                        sequence: sequence
                    );
                    break;
                default:
                    GD.PushError($"Component type not implemented in Create Component!: {componentType}");
                    break;
            }
            return component;
        }
    }
}
