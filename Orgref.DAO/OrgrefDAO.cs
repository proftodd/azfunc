using My.Models;

namespace My.DAO
{
    public interface OrgrefDAO
    {
        SearchResult GetSubstances(string [] searchTerms);

        string GetStructure(string inchiKey);
    }
}