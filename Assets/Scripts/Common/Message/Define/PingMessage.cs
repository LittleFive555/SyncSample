using System;

namespace SyncSample.Common
{
    [Serializable]
    public class PingMessage
    {
        public long timestamp;

        public PingMessage() { }

        public PingMessage(long timestamp)
        {
            this.timestamp = timestamp;
        }
    }
}
