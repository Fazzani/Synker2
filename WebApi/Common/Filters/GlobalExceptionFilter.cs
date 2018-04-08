using hfa.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace hfa.WebApi.Common.Filters
{
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        public GlobalExceptionFilterAttribute(
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory logger)
        {
            _hostingEnvironment = hostingEnvironment;
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._logger = logger.CreateLogger("Global Exception Filter");
        }

        public override void OnException(ExceptionContext context)
        {

            var status = HttpStatusCode.InternalServerError;
            String message = String.Empty;

            var exceptionType = context.Exception.GetType();
            if (exceptionType == typeof(UnauthorizedAccessException))
            {
                message = "Unauthorized Access";
                status = HttpStatusCode.Unauthorized;
            }
            else if (exceptionType == typeof(NotImplementedException))
            {
                message = "A server error occurred.";
                status = HttpStatusCode.NotImplemented;
            }
            else if (exceptionType == typeof(BusinessException))
            {
                message = context.Exception.ToString();
                status = HttpStatusCode.InternalServerError;
            }
            else
            {
                message = context.Exception.Message;
                status = HttpStatusCode.NotFound;
            }

            _logger.LogError(context.Exception, message);
            var response = context.HttpContext.Response;
            response.StatusCode = (int)status;
            response.ContentType = "application/json";

            if (_hostingEnvironment.IsDevelopment())
                context.Result = new ObjectResult(context.Exception);
            else
                context.Result = new ObjectResult(new { status = status, message = message });
        }
    }
}
