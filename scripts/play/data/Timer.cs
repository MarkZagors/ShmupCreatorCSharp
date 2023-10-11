namespace Editor
{
    public class Timer
    {
        public float WaitTime { get; private set; }
        public float ProcessTime { get; private set; }
        public int LoopCount { get; private set; }
        public float[] TiggerOffsets { get; private set; }

        public Timer(float waitTime, float processTime, int loopCount)
        {
            WaitTime = waitTime;
            ProcessTime = processTime;
            LoopCount = loopCount;

            CreateOffsets();
        }

        public void UpdateTimer(float waitTime, float processTime)
        {
            WaitTime = waitTime;
            ProcessTime = processTime;

            CreateOffsets();
        }

        private void CreateOffsets()
        {
            if (LoopCount == 0)
            {
                TiggerOffsets = new float[0];
            }
            else if (LoopCount == 1)
            {
                TiggerOffsets = new float[1];
                TiggerOffsets[0] = 0.0f;
            }
            else
            {
                TiggerOffsets = new float[LoopCount];
                TiggerOffsets[0] = 0.0f;
                for (int i = 1; i < TiggerOffsets.Length; i++)
                {
                    TiggerOffsets[i] = i * (ProcessTime + WaitTime);
                }
            }
        }
    }
}