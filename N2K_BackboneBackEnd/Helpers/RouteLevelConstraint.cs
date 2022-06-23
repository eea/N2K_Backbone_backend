using N2K_BackboneBackEnd.Enumerations;
using System.Globalization;
namespace N2K_BackboneBackEnd.Helpers
{
    public class RouteLevelConstraint : IRouteConstraint
    {

        public bool Match(HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            //validate input params  
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (routeKey == null)
                throw new ArgumentNullException(nameof(routeKey));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            object routeValue;

            if (values.TryGetValue(routeKey, out routeValue))
            {
                var parameterValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);
                return Enum.TryParse(parameterValueString, true, out Level outLevel);
            }

            return false;
        }
    }
}