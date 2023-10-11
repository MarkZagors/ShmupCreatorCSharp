namespace Editor
{
    public class Spawner
    {
        public ComponentSpawner Component { get; private set; }
        public BulletData[,] Bullets { get; private set; }
        public int BulletCount { get; private set; }
        public Timer Timer { get; private set; }

        public Spawner(ComponentSpawner component, BulletData[,] bullets, int bulletCount, Timer timer)
        {
            Component = component;
            Bullets = bullets;
            BulletCount = bulletCount;
            Timer = timer;
        }
    }
}