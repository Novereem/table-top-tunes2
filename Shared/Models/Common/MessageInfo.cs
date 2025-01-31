using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Common
{
    public class MessageInfo
    {
        public string InternalMessage { get; }
        public string UserMessage { get; }
        public MessageType Type { get; }
        public HttpStatusCode HttpStatusCode { get; }

        public MessageInfo(
            string internalMessage,
            string userMessage,
            MessageType type,
            HttpStatusCode httpStatusCode)
        {
            InternalMessage = internalMessage;
            UserMessage = userMessage;
            Type = type;
            HttpStatusCode = httpStatusCode;
        }
    }

    public enum MessageType
    {
        Success,
        Error,
        Warning
    }
}
