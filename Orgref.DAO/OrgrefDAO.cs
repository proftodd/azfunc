using System.Threading.Tasks;
using My.Models;

namespace My.DAO
{
    public class DAOOptions
    {
        public string host { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }

    public interface OrgrefDAO
    {
        Task<SearchResult> GetSubstances(string [] searchTerms);

        Task<string> GetStructure(string inchiKey);
    }
}