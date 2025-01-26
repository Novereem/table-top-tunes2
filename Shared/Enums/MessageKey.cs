using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums
{
    public enum MessageKey
    {
        //Standard
        Error_InternalServerError,
        Error_InvalidInput,

        //Authentication
        Error_PasswordTooShort,
        Error_EmailTaken,
        Error_InvalidEmail,
        Error_JWTNullOrEmpty,
        Error_InvalidCredentials,

        Success_OperationCompleted,
        Success_DataRetrieved
    }
}
