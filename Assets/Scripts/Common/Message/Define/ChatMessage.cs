using System;

namespace SyncSample.Common
{
    [Serializable]
    public class ChatMessage
    {
        public string sender;
        public string text;

        public ChatMessage() { }

        public ChatMessage(string sender, string text)
        {
            this.sender = sender ?? string.Empty;
            this.text = text ?? string.Empty;
        }
    }
}
