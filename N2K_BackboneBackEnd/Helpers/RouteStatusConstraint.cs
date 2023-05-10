using N2K_BackboneBackEnd.Enumerations;
using System.Globalization;

namespace N2K_BackboneBackEnd.Helpers
{
    public class RouteStatusConstraint : IRouteConstraint
    {

#pragma warning disable CS8767 // La nulabilidad de los tipos de referencia del tipo de parámetro no coincide con el miembro implementado de forma implícita (posiblemente debido a los atributos de nulabilidad).
        public bool Match(HttpContext httpContext,
#pragma warning restore CS8767 // La nulabilidad de los tipos de referencia del tipo de parámetro no coincide con el miembro implementado de forma implícita (posiblemente debido a los atributos de nulabilidad).
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
                return Enum.TryParse(parameterValueString, true, out SiteChangeStatus outStatus);
                //return outStatus;
            }

            return false;
        }
    }
}