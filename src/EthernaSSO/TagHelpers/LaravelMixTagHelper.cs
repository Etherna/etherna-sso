using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;

namespace Etherna.SSOServer.TagHelpers
{
    [HtmlTargetElement(Attributes = LaravelMixAttributeName)]
    public class LaravelMixTagHelper : UrlResolutionTagHelper
    {
        // Fields.
        private const string LaravelMixAttributeName = "mix-version";
        private readonly IWebHostEnvironment hostingEnvironment;

        // Constructor.
        public LaravelMixTagHelper(
            IWebHostEnvironment env,
            HtmlEncoder htmlEncoder,
            IUrlHelperFactory urlHelperFactory)
            : base(urlHelperFactory, htmlEncoder)
        {
            this.hostingEnvironment = env;
        }

        // Methods.
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            //get mix manifest file
            var manifestFileInfo = hostingEnvironment.WebRootFileProvider.GetFileInfo("dist/mix-manifest.json");
            if (manifestFileInfo.Exists)
            {
                try
                {
                    // Process urls with '~/'
                    var attributeName = output.TagName == "script" || output.TagName == "img" ? "src" : "href";
                    ProcessUrlAttribute(attributeName, output);

                    //get mix manifest versioned filename
                    var fileMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(manifestFileInfo.PhysicalPath));
                    var srcAttribute = output.Attributes.FirstOrDefault(a => a.Name == attributeName);
                    var srcPath = srcAttribute?.Value.ToString() ?? "";
                    var assetFileName = "/" + Path.GetFileName(srcPath);

                    if (fileMap.ContainsKey(assetFileName))
                    {
                        var outputAssetName = fileMap[assetFileName];
                        output.Attributes.SetAttribute(attributeName, srcPath.Replace(assetFileName, outputAssetName));
                    }
                    else
                    {
                        var versionProvider = ViewContext.HttpContext.RequestServices.GetRequiredService<IFileVersionProvider>();
                        output.Attributes.SetAttribute(attributeName, versionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, srcPath));
                    }
                }
                catch { }
            }

            output.Attributes.RemoveAll(LaravelMixAttributeName);
        }

    }
}
