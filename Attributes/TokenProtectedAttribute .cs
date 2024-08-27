using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StockMarket.Attributes
{

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TokenProtectedAttribute : Attribute, IAuthorizationFilter
    {
        private const string ApiKeyQueryParam = "pYR3gnW9oyf6juDsf5rtdP7hs2d8wuHg";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var query = context.HttpContext.Request.Query;

            string UserApiKey = context.HttpContext.Request.Query["apikey"].ToString();

            if (!ValidateToken(UserApiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private bool ValidateToken(string token)
        {
            return token == ApiKeyQueryParam; 
        }
    }




}
