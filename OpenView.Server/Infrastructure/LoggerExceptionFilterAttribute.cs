using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace OpenView.Server.Infrastructure
{
    public class LoggerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<LoggerExceptionFilterAttribute> _logger;

        public LoggerExceptionFilterAttribute(ILogger<LoggerExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception");
        }
    }
}
