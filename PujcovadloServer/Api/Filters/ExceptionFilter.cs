using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ForbiddenAccessException)
        {
            context.Result = new ForbidResult();
        }
        else if (context.Exception is NotAuthenticatedException)
        {
            context.Result = new ChallengeResult();
        }
        // Todo: get rid of this
        else if (context.Exception is UnauthorizedAccessException)
        {
            context.Result = new UnauthorizedResult();
        }
        else if (context.Exception is EntityNotFoundException)
        {
            context.Result = new NotFoundResult();
        }
        else if (context.Exception is DbUpdateConcurrencyException)
        {
            context.Result = new ConflictResult();
        }
        else if (context.Exception is ArgumentNullException)
        {
            context.Result = new BadRequestResult();
        }
        else if (context.Exception is ActionNotAllowedException)
        {
            context.Result = new BadRequestResult();
        }
    }
}