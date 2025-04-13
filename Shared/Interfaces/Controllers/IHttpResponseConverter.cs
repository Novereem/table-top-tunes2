using Microsoft.AspNetCore.Mvc;
using Shared.Models.Common;

namespace Shared.Interfaces.Controllers;

public interface IHttpResponseConverter
{
    IActionResult Convert<T>(HttpServiceResult<T> serviceResult);
}