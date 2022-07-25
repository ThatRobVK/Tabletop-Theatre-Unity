using System.Threading.Tasks;
using TT.Shared.GameContent;

namespace TT.Shared
{
    public interface IGameContent
    {
        /// <summary>
        /// Loads and returns the available content packs for this version of TT.
        /// </summary>
        /// <returns>An array of content packs, or null if none could be loaded.</returns>
        Task<ContentPack[]> GetContentAsync();
        
        /// <summary>
        /// Returns the location of the addressables content catalog for this version of TT.
        /// </summary>
        /// <returns>A string with the content location. This can be a local file path or a URI.</returns>
        Task<string> GetContentCatalogLocationAsync();
    }
}