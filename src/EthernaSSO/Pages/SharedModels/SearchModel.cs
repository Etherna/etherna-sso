namespace Etherna.SSOServer.Pages.SharedModels
{
    public class SearchModel
    {
        public SearchModel(
            string? query,
            string? razorPage = default,
            string? razorPageHandler = default,
            string searchParamName = "q")
        {
            Query = query ?? "";
            RazorPage = razorPage;
            RazorPageHandler = razorPageHandler;
            SearchParamName = searchParamName;
        }

        public string Query { get; }
        public string? RazorPage { get; }
        public string? RazorPageHandler { get; }
        public string SearchParamName { get; }
    }
}
