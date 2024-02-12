using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Filters;

public class ExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        base.OnException(context);

        if (context.Exception is ForbiddenAccessException)
        {
            var details = new ExceptionResponse
            {
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Errors = new List<string> { context.Exception.Message }
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
        else if (context.Exception is NotAuthenticatedException)
        {
            context.Result = new ChallengeResult();
        }
        else if (context.Exception is EntityNotFoundException)
        {
            var details = new ExceptionResponse
            {
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Errors = new List<string> { context.Exception.Message }
            };

            context.Result = new NotFoundObjectResult(details);
        }
        else if (context.Exception is DbUpdateConcurrencyException)
        {
            context.Result = new ConflictResult();
        }
        else if (context.Exception is ArgumentNullException)
        {
            context.Result = new BadRequestResult();
        }
        else if (context.Exception is OperationNotAllowedException)
        {
            var details = new ExceptionResponse
            {
                Title = "Unprocessable Entity",
                Status = StatusCodes.Status422UnprocessableEntity,
                Errors = new List<string> { context.Exception.Message }
            };

            context.Result = new UnprocessableEntityObjectResult(details);
        }
    }
}