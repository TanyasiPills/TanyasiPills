using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly_CSharp
{
    internal class JitterBuffer
    {
        private Queue<float[]> queue = new Queue<float[]>();

        private int frameSize;
        private int targetFrames;
        private int maxFrames;

        public JitterBuffer(int frameSize, int targetFrames = 5, int maxFrames = 10)
        {
            this.frameSize = frameSize;
            this.targetFrames = targetFrames;
            this.maxFrames = maxFrames;
        }

        public int Count => queue.Count;

        // Push decoded frame
        public void Push(float[] frame)
        {
            queue.Enqueue(frame);

            // HARD CAP → drop old audio (prevents latency growth)
            while (queue.Count > maxFrames)
            {
                queue.Dequeue();
            }
        }

        // Pop frame for playback
        public bool TryPop(out float[] frame)
        {
            if (queue.Count >= targetFrames)
            {
                frame = queue.Dequeue();
                return true;
            }

            frame = null;
            return false;
        }
    }
}
