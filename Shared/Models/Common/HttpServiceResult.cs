using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Common
{
    public class HttpServiceResult<T> : ServiceResult<T>
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        private HttpServiceResult(ServiceResult<T> baseResult)
            : base(baseResult.Data, baseResult.MessageInfo)
        {
            HttpStatusCode = baseResult.MessageInfo.HttpStatusCode;
        }

        public static HttpServiceResult<T> FromServiceResult(ServiceResult<T> baseResult)
        {
            return new HttpServiceResult<T>(baseResult);
        }
    }
}
