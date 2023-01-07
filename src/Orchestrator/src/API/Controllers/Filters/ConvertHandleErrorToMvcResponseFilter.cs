using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Domain.Errors;
using Domain.Results;

namespace API.Controllers.Filters;

public class ConvertHandleErrorToMvcResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        var handleResult = (HandleResult)((ObjectResult)context.Result).Value!;
        if (handleResult.Error != null)
        {
            switch (handleResult.Error)
            {
                case AuthorizationError:
                    context.Result = new ObjectResult(handleResult)
                    {
                        StatusCode = handleResult.Error.Errors.Values.First().First() == "Forbidden" ?
                            StatusCodes.Status403Forbidden :
                            StatusCodes.Status401Unauthorized
                    };
                    break;
                default:
                    context.Result = new BadRequestObjectResult(handleResult);
                    break;
            }
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}