using System;

namespace SyncSample.Common
{
    [Serializable]
    public class JoinMessage
    {
        public string name;

        public JoinMessage() { }

        public JoinMessage(string name)
        {
            this.name = name ?? string.Empty;
        }
    }
}
