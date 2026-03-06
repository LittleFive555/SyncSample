using System;

namespace SyncSample.Common
{
    [Serializable]
    public class EchoMessage
    {
        public string content;

        public EchoMessage() { }

        public EchoMessage(string content)
        {
            this.content = content ?? string.Empty;
        }
    }
}
