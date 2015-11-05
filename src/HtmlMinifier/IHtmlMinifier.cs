using System.Threading.Tasks;

namespace DotNetGems.TagHelpers
{
    public interface IHtmlMinifier
    {
        Task<string> MinifyAsync(string source);
    }
}
