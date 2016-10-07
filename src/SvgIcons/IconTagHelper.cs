using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Xml.Linq;

namespace SvgIcons
{
    [HtmlTargetElement("svg", Attributes ="src")]
    public class IconTagHelper : TagHelper
    {
        private readonly IFileProvider _wwwroot;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public IconTagHelper(IHostingEnvironment env, IUrlHelperFactory urlHelperFactory)
        {
            _wwwroot = env.WebRootFileProvider;
            _urlHelperFactory = urlHelperFactory;
        }

        [HtmlAttributeName]
        public string Src { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var element = GetSvgElementFromSrc();
            if (element == null)
            {
                return;
            }

            foreach (var attribute in element.Attributes())
            {
                if (!output.Attributes.ContainsName(attribute.Name.LocalName))
                {
                    output.Attributes.Add(new TagHelperAttribute(attribute.Name.LocalName, attribute.Value));
                }
            }
            output.Content.AppendHtml(element.FirstNode.ToString());
        }


        private string ResolveSrcPath()
        {
            var path = Src;
            if (path == null)
            {
                return default(string);
            }
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            path = urlHelper.Content(path);

            var resolvedPath = path;

            var queryStringStartIndex = path.IndexOf('?');
            if (queryStringStartIndex != -1)
            {
                resolvedPath = path.Substring(0, queryStringStartIndex);
            }

            Uri uri;
            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out uri))
            {
                // Don't inline if the path is absolute
                return default(string);
            }
            return resolvedPath;
        }

        private IFileInfo GetFileInfo(string path)
        {
            var fileInfo = _wwwroot.GetFileInfo(path);
            var requestPathBase = ViewContext.HttpContext.Request.PathBase;
            if (!fileInfo.Exists)
            {
                if (requestPathBase.HasValue &&
                    path.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(requestPathBase.Value.Length);
                    fileInfo = _wwwroot.GetFileInfo(path);
                }
            }
            return fileInfo;

        }

        private XElement ParseSvgElement(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var xn = XName.Get("svg", "http://www.w3.org/2000/svg");
            if (doc.Root.Name != xn)
                return null;
            return doc.Root;
        }

        private XElement GetSvgElementFromSrc()
        {
            var resolvedPath = ResolveSrcPath();
            if (string.IsNullOrEmpty(resolvedPath))
            {
                return null;
            }

            var fileInfo = GetFileInfo(resolvedPath);
            if (!fileInfo.Exists)
            {
                return null;
            }
            return ParseSvgElement(fileInfo.CreateReadStream());

        }

    }

}
