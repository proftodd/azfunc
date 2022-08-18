using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using My.DAO;
using My.Models;

namespace Orgref.PostgreSqlDao
{
    public class OrgrefPostgreSQLDAO : OrgrefDAO
    {
        OrgrefContext ctx;

        public OrgrefPostgreSQLDAO(IOptions<DAOOptions> options)
        {
            ctx = new OrgrefContext(options);
        }

        private readonly Regex NUM_9000_PATTERN = new Regex("9\\d{3}");
        private readonly Regex INCHI_KEY_PATTERN = new Regex("[A-Z]{14}-[A-Z]{10}-[A-Z]");

        public async Task<SearchResult> GetSubstances(string [] searchTerms)
        {
            (string bestSearchTerm, List<string> restOfTheSearchTerms) = BestSearchTerm(searchTerms);
            IQueryable<Entity> candidates = FirstSearch(bestSearchTerm);
            for (int i = 0; i < restOfTheSearchTerms.Count; ++i)
            {
                candidates = NextSearch(candidates, restOfTheSearchTerms[i]);
            }

            var entityList = await candidates.ToListAsync();
            return new SearchResult(searchTerms, entityList);
        }

        private (string, List<string>) BestSearchTerm(string [] searchTermArray)
        {
            string bestSearchTerm;
            List<string> rest;
            if ((bestSearchTerm = Array.Find(searchTermArray, st => NUM_9000_PATTERN.Match(st).Success)) != null)
            {
                rest = searchTermArray.Where(st => st != bestSearchTerm).ToList();
                return (bestSearchTerm, rest);
            } else if ((bestSearchTerm = Array.Find(searchTermArray, st => INCHI_KEY_PATTERN.Match(st).Success)) != null)
            {
                rest = searchTermArray.Where(st => st != bestSearchTerm).ToList();
                return (bestSearchTerm, rest);
            } else
            {
                var orderedByLength = searchTermArray.OrderByDescending(st => st.Length);
                bestSearchTerm = orderedByLength.First();
                rest = orderedByLength.Skip(1).ToList();
                return (bestSearchTerm, rest);
            }
        }

        private IQueryable<Entity> FirstSearch(string searchTerm)
        {
            if (NUM_9000_PATTERN.Match(searchTerm).Success)
            {
                return ctx.Entities.Where(e => e.Num.Num == int.Parse(searchTerm));
            } else if (INCHI_KEY_PATTERN.Match(searchTerm).Success)
            {
                return ctx.Entities.Where(e => e.Sub.InchiKey == searchTerm);
            } else
            {
                return ctx.Entities.Where(e => e.Descriptors.Any(d => Regex.IsMatch(d.Desc, searchTerm, RegexOptions.IgnoreCase)));
            }
        }

        private IQueryable<Entity> NextSearch(IQueryable<Entity> candidates, string searchTerm)
        {
            if (NUM_9000_PATTERN.Match(searchTerm).Success)
            {
                return candidates.Where(e => e.Num.Num == int.Parse(searchTerm));
            } else if (INCHI_KEY_PATTERN.Match(searchTerm).Success)
            {
                return candidates.Where(e => e.Sub.InchiKey == searchTerm);
            } else
            {
                return candidates.Where(e => e.Descriptors.Any(d => Regex.IsMatch(d.Desc, searchTerm, RegexOptions.IgnoreCase)));
            }
        }

        public async Task<string> GetStructure(string inchiKey)
        {
            return await ctx.Substances.Where(s => s.InchiKey == inchiKey).Select(s => s.Inchi).FirstOrDefaultAsync();
        }
    }
}