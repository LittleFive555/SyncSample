using System;

namespace SyncSample.Common
{
    [Serializable]
    public class ErrorMessage
    {
        public string code;
        public string message;

        public ErrorMessage() { }

        public ErrorMessage(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }
    }
}
