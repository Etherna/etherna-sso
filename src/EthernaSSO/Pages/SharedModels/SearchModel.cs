using System.Collections.Generic;

namespace Etherna.SSOServer.Pages.SharedModels
{
    public class SearchModel
    {
        public SearchModel(
            string? query,
            string? razorPage = default,
            string? razorPageHandler = default,
            Dictionary<string, string>? routeData = null,
            string searchParamName = "q")
        {
            Query = query ?? "";
            RazorPage = razorPage;
            RazorPageHandler = razorPageHandler;
            RouteData = routeData ?? new Dictionary<string, string>();
            SearchParamName = searchParamName;
        }

        public string Query { get; }
        public string? RazorPage { get; }
        public string? RazorPageHandler { get; }
        public IDictionary<string, string> RouteData { get; }
        public string SearchParamName { get; }
    }
}
