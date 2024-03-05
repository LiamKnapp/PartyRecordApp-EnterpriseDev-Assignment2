using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace PartyRecordApp.Middlewares
{
    public class FirstVisitMiddleware
    {
        private readonly RequestDelegate _next;

        public FirstVisitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check if the user has previously visited the app
            if (!context.Request.Cookies.ContainsKey("FirstVisit"))
            {
                // Set the first visit timestamp in a cookie
                context.Response.Cookies.Append("FirstVisit", DateTime.Now.ToString());
            }

            await _next(context);
        }
    }
}
