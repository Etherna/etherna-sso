using System.Threading.Tasks;

namespace Etherna.SSOServer.Services.Utilities
{
    public interface IRazorViewRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
}