using System;

namespace Etherna.SSOServer.Pages.SharedModels
{
    public class PageSelectorModel
    {
        public PageSelectorModel(
            string baseUrl,
            int currentPage,
            int maxPage,
            string pageParamName = "p")
        {
            if (currentPage < 0)
                throw new ArgumentOutOfRangeException(nameof(currentPage), "Value can't be negative");
            if (maxPage < 0)
                throw new ArgumentOutOfRangeException(nameof(maxPage), "Value can't be negative");

            BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            CurrentPage = currentPage;
            MaxPage = maxPage;
            PageParamName = pageParamName ?? throw new ArgumentNullException(nameof(pageParamName));
        }

        public string BaseUrl { get; }
        public int CurrentPage { get; }
        public int MaxPage { get; }
        public string PageParamName { get; }
    }
}
