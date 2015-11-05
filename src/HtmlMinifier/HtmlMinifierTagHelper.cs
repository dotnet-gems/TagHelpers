using Microsoft.AspNet.Razor.TagHelpers;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using System;

namespace DotNetGems.TagHelpers
{
    [HtmlTargetElement("html", Attributes = MinifyAttributeName)]
    public class HtmlMinifierTagHelper : TagHelper
    {
        private readonly string CurrentEnvironmentName;
        private static readonly char[] NameSeparator = new[] { ',' };
        private readonly IHtmlMinifier HtmlMinifier;
        private const string MinifyAttributeName = "minify-environment";

        /// <summary>
        /// A comma separated list of environment names in which the content should be rendered.
        /// </summary>
        /// <remarks>
        /// The specified environment names are compared case insensitively to the current value of
        /// <see cref="IHostingEnvironment.EnvironmentName"/>.
        /// </remarks>
        [HtmlAttributeName(MinifyAttributeName)]
        public string EnvironmentNames { get; set; }

        public HtmlMinifierTagHelper(IHtmlMinifier htmlMinifier, IHostingEnvironment env)
        {
            CurrentEnvironmentName = env.EnvironmentName?.Trim();
            HtmlMinifier = htmlMinifier;
        }

        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var environments = EnvironmentNames?.Split(NameSeparator, StringSplitOptions.RemoveEmptyEntries)
                                        .Where(name => !string.IsNullOrWhiteSpace(name));

            if (!environments.Any() || environments.Any(name =>
                 string.Equals(name.Trim(), CurrentEnvironmentName, StringComparison.OrdinalIgnoreCase)))
            {
                // Matching environment name found, minify
                var childContent = await output.GetChildContentAsync();
                var minifyContent = await HtmlMinifier.MinifyAsync(childContent.GetContent());
                output.Content.SetHtmlContent(minifyContent);
            }

            TagHelperAttribute attr;
            if (output.Attributes.TryGetAttribute(MinifyAttributeName, out attr))
            {
                output.Attributes.Remove(attr);
            }
        }

    }
}
