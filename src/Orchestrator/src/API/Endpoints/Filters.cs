using System.Diagnostics;

using Domain.Errors;
using Domain.Results;

namespace API.Endpoints;

public static class Filters
{
    public static async ValueTask<object?> ConvertHandleResult(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next
    )
    {
        var handleResult = (HandleResult?)await next(context);
        Debug.Assert(handleResult != null);
        if (handleResult.Error != null)
        {
            switch (handleResult.Error)
            {
                case AuthorizationError:
                    return TypedResults.Json(
                        handleResult,
                        statusCode: handleResult.Error.Message == "Forbidden" ?
                            StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized
                    );
                default:
                    return TypedResults.BadRequest(handleResult);
            }
        }

        return TypedResults.Ok(handleResult);
    }
}
