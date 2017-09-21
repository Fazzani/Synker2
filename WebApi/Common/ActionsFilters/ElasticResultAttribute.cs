using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hfa.WebApi.Common.ActionsFilters
{
    public class ElasticResultAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            // OkObjectResult
            if (context.Result is OkObjectResult result)
            {
                if (IsAssignableToGenericType(result.Value.GetType(), typeof(ISearchResponse<>)))
                {
                    var res = (dynamic)result.Value.GetType().GetProperty("GetClassType")
                                .GetValue(result.Value, null);
                    //res.
                }
            }
            base.OnResultExecuted(context);
        }

        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}
