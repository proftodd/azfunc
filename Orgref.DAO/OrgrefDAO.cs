using System.Threading.Tasks;
using My.Models;

namespace My.DAO
{
    public class DAOOptions
    {
        public string Host { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public interface OrgrefDAO
    {
        Task<SearchResult> GetSubstances(string [] searchTerms);

        Task<string> GetStructure(string inchiKey);
    }
}