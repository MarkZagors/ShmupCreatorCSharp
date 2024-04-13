using System.Collections.Generic;

namespace Editor
{
    public static class SpawnerVerifier
    {
        public static void Verify(List<IComponent> components)
        {
            foreach (IComponent component in components)
            {
                var spawnRef = (ModifierRef)component.GetModifier(ModifierID.SPAWNER_REF_BUNDLE);
                if (spawnRef != null && spawnRef.Ref != null)
                {
                    var bundleRefNested = (ModifierRef)spawnRef.Ref.GetModifier(ModifierID.BUNDLE_REF_BULLET);
                    if (bundleRefNested != null && bundleRefNested.Ref != null)
                    {
                        ((ComponentSpawner)component).Valid = true;
                    }
                    else
                    {
                        ((ComponentSpawner)component).Valid = false;
                    }
                    return;
                }
            }
        }
    }
}
