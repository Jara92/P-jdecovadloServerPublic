using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Filters;

public class ValidateIdFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Získání hodnoty 'id' z URL
        if (context.RouteData.Values.TryGetValue("id", out var routeId))
        {
            // Porovnání s hodnotou 'Id' z těla požadavku
            if (context.ActionArguments.TryGetValue("request", out var request) && request is EntityRequest)
            {
                // Get Id from the request
                var idFromRequest = (((EntityRequest)request).Id).ToString();

                // Both Ids are not null
                if (routeId != null && idFromRequest != null)
                {
                    // Throw exception if Id does not match
                    if (routeId.ToString() != idFromRequest)
                    {
                        context.Result = new BadRequestResult();
                    }
                }
            }
        }

        // Pokud Id jsou stejné nebo nelze je porovnat, pokračujeme ve standardním zpracování akce
        base.OnActionExecuting(context);
    }
}