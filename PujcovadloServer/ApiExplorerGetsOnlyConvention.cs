using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace PujcovadloServer;

/// <summary>
/// Sets ApiExplorer.IsVisible to true only for actions in controllers that inherit from ControllerBase.
/// That allows to hide admin actions from swagger.
/// </summary>
public class ApiExplorerGetsOnlyConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        /*// Extends ControllerBase (base api controller)
        bool isVisible = action.Controller.ControllerType.BaseType == typeof(ControllerBase);

        // Extends ACrudController (advanced api controller)
        if(action.Controller.ControllerType.BaseType == typeof(ACrudController<>))
        {
            isVisible = true;
        }*/

        // Hide admin controller actions
        bool isVisible = action.Controller.ControllerType.BaseType != typeof(Controller);

        action.ApiExplorer.IsVisible = isVisible;
    }
}