using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        if (Request.Cookies["FirstVisit"] == null)
        {
            // Set the first visit timestamp in ViewData
            ViewData["WelcomeMessage"] = "Welcome to your first visit!";
            ViewData["FirstVisitTimestamp"] = DateTime.Now;

            // Set the FirstVisit cookie
            Response.Cookies.Append("FirstVisit", DateTime.Now.ToString());
        }
        else
        {
            // Get the first visit timestamp from the cookie
            var firstVisitTimestamp = DateTime.Parse(Request.Cookies["FirstVisit"]);
            ViewData["WelcomeMessage"] = $"Welcome back! You first started using the app on {firstVisitTimestamp}.";
            ViewData["FirstVisitTimestamp"] = firstVisitTimestamp;
        }
    }
}
