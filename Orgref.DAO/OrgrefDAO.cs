using System.Threading.Tasks;
using My.Models;

namespace My.DAO
{
    public interface OrgrefDAO
    {
        Task<SearchResult> GetSubstances(string [] searchTerms);

        Task<string> GetStructure(string inchiKey);
    }
}