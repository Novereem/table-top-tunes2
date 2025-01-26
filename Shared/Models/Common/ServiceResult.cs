using Shared.Enums;
using Shared.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Common
{
    public class ServiceResult<T>
    {
        public T? Data { get; private set; }
        public MessageInfo MessageInfo { get; private set; }

        protected ServiceResult(T? data, MessageInfo messageInfo)
        {
            Data = data;
            MessageInfo = messageInfo;
        }

        public static ServiceResult<T> SuccessResult(T? data = default, MessageKey messageKey = MessageKey.Success_OperationCompleted)
        {
            var message = MessageRepository.GetMessage(messageKey);
            if (message.Type != MessageType.Success)
            {
                throw new InvalidOperationException("The specified message key does not correspond to a success message.");
            }
            return new ServiceResult<T>(data, message);
        }

        public static ServiceResult<T> Failure(MessageKey messageKey)
        {
            var message = MessageRepository.GetMessage(messageKey);
            if (message.Type != MessageType.Error)
            {
                throw new InvalidOperationException("The specified message key does not correspond to an error message.");
            }
            return new ServiceResult<T>(default, message);
        }

        public ServiceResult<TNew> ToFailureResult<TNew>()
        {
            if (IsSuccess)
            {
                throw new InvalidOperationException("Cannot convert a successful result to a failure result.");
            }

            return new ServiceResult<TNew>(
                default,
                MessageInfo
            );
        }

        public bool IsSuccess => MessageInfo.Type == MessageType.Success;
        public bool IsFailure => MessageInfo.Type == MessageType.Error;
    }
}
