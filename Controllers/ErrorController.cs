using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var errorViewModel = new ErrorViewModel
            {
                StatusCode = statusCode,
                Message = statusCode switch
                {
                    404 => "Sorry, the page you're looking for cannot be found.",
                    500 => "An internal server error occurred. Please try again later.",
                    403 => "You don't have permission to access this resource.",
                    _ => "An unexpected error occurred."
                }
            };

            return View("Error", errorViewModel);
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var errorViewModel = new ErrorViewModel
            {
                StatusCode = 500,
                Message = "An unexpected error occurred while processing your request.",
                Details = exceptionHandlerPathFeature?.Error.Message
            };

            return View("Error", errorViewModel);
        }
    }

    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        // REMOVED: public string? RequestId { get; set; }
    }
}