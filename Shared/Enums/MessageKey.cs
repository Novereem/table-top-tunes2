using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums
{
    public enum MessageKey
    {
        Error_InvalidInput,
        Error_PasswordTooShort,
        Error_EmailTaken,
        Error_InvalidEmail,
        Error_InternalServerError,

        Success_OperationCompleted,
        Success_DataRetrieved
    }
}
