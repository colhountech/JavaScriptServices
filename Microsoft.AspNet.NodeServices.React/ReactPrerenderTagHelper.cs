using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;

namespace Microsoft.AspNet.NodeServices.React
{
    [HtmlTargetElement(Attributes = PrerenderModuleAttributeName)]
    public class ReactPrerenderTagHelper : TagHelper
    {
        static INodeServices fallbackNodeServices; // Used only if no INodeServices was registered with DI
        
        const string PrerenderModuleAttributeName = "asp-react-prerender-module";
        const string PrerenderExportAttributeName = "asp-react-prerender-export";
        
        [HtmlAttributeName(PrerenderModuleAttributeName)]
        public string ModuleName { get; set; }
        
        [HtmlAttributeName(PrerenderExportAttributeName)]
        public string ExportName { get; set; }

        private IHttpContextAccessor contextAccessor;
        private INodeServices nodeServices;

        public ReactPrerenderTagHelper(IServiceProvider nodeServices, IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
            this.nodeServices = (INodeServices)nodeServices.GetService(typeof (INodeServices)) ?? fallbackNodeServices;
            
            // Consider removing the following. Having it means you can get away with not putting app.AddNodeServices()
            // in your startup file, but then again it might be confusing that you don't need to.
            if (this.nodeServices == null) {
                this.nodeServices = fallbackNodeServices = Configuration.CreateNodeServices(NodeHostingModel.Http);
            }
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var request = this.contextAccessor.HttpContext.Request;
            var result = await ReactRenderer.RenderToString(
                nodeServices: this.nodeServices,
                componentModuleName: this.ModuleName,
                componentExportName: this.ExportName,
                requestUrl: request.Path + request.QueryString.Value);
            output.Content.SetContentEncoded(result);
        }
    }
}