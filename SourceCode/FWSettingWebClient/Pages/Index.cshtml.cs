using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FWSettingWebClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            HttpResponse response = HttpContext.Response;
            response.Headers["Access-Control-Allow-Origin"]= "*";
            response.Headers["Access-Control-Allow-Methods"] = "POST,GET,PUT,DELETE";
            response.Headers["Access-Control-Max-Age"] = "3600";
            response.Headers["Access-Control-Allow-Headers"] = "*";
            response.Headers["Access-Control-Allow-Credentials"] = "true";

        }
    }
}