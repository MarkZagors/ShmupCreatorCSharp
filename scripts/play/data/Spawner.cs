namespace Editor
{
    public class Spawner
    {
        public ComponentSpawner Component { get; set; }
        public BulletData[] Bullets { get; set; }
        public int BulletCount { get; set; }

        public Spawner(ComponentSpawner component, BulletData[] bullets, int bulletCount)
        {
            Component = component;
            Bullets = bullets;
            BulletCount = bulletCount;
        }
    }
}