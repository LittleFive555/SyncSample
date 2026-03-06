using System;

namespace SyncSample.Common
{
    [Serializable]
    public class PongMessage
    {
        public long timestamp;
        public long serverTime;

        public PongMessage() { }

        public PongMessage(long timestamp, long serverTime)
        {
            this.timestamp = timestamp;
            this.serverTime = serverTime;
        }
    }
}
