using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Npgsql;
using NpgsqlTypes;

namespace My.Functions
{
    class Entity
    {
        public int entityId { get; }
        public int num9000 { get; }
        public string inchiKey { get; }
        public IList<string> descriptors { get; }

        internal Entity() : this(0, 0, "", new List<string>()) {}

        internal Entity(int entityId, int num9000, string inchiKey, List<string> descriptors)
        {
            this.entityId = entityId;
            this.num9000 = num9000;
            this.inchiKey = inchiKey;
            this.descriptors = descriptors;
        }

        override public string ToString()
        {
            var descriptorString = string.Join(", ", descriptors);
            return $"{(num9000 == 0 ? "" : num9000.ToString()): %4s} {inchiKey: %27s} {descriptorString}";
        }
    }
    class SearchResult
    {
        public IList<string> searchTerms { get; }
        public IList<Entity> entities { get; }

        internal SearchResult() : this(new List<string>(), new List<Entity>()) {}

        internal SearchResult(string [] searchTerms) : this(new List<string>(searchTerms), new List<Entity>()) {}

        internal SearchResult(IList<string> searchTerms) : this(searchTerms, new List<Entity>()) {}

        internal SearchResult(IList<string> searchTerms, IList<Entity> entities)
        {
            this.searchTerms = searchTerms;
            this.entities = entities;
        }

        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }
    }

    class OrgrefDAO
    {
        private readonly string url;
        private const string NUM9000_QUERY = "select entity_id from nums where num_9000 = @searchTerm";
        private const string INCHI_KEY_QUERY = "select entity_id from substances where inchi_key = @searchTerm";
        private const string DESCRIPTOR_QUERY = "select entity_id from descriptors where descriptor ~* @searchTerm";
        private const string LATER_NUM9000_QUERY = "select entity_id from nums where num_9000 = @searchTerm and entity_id = any(@entityIdList)";
        private const string LATER_INCHI_KEY_QUERY = "select entity_id from substances where inchi_key = @searchTerm and entity_id = any(@entityIdList";
        private const string LATER_DESCRIPTOR_QUERY = "select entity_id from descriptors where descriptor ~* @searchTerm and entity_id = any(@entityIdList)";
        private const string FINAL_QUERY = @"select descriptors.descriptor_id, entities.entity_id, nums.num_9000, substances.inchi_key, descriptors.descriptor
        from
            entities
          left join
            nums on entities.entity_id = nums.entity_id
          left join
            substances on entities.entity_id = substances.entity_id
          left join
            descriptors on entities.entity_id = descriptors.entity_id
        where
          entities.entity_id = any(@entityIdList)
        order by entities.entity_id, descriptors.descriptor_id
        ";

        public OrgrefDAO(string host = "localhost", string username = "guest", string password = "guest")
        {
            url = $"Host={host};Username={username};Password={password};Database=orgref";
        }

        public SearchResult GetSubstances(string [] searchTerms)
        {
            SearchResult result = new SearchResult(searchTerms);

            (string bestSearchTerm, List<string> restOfTheSearchTerms) = BestSearchTerm(searchTerms);

            IList<int> search = FirstSearch(bestSearchTerm);
            int i;
            for (i = 0; i < restOfTheSearchTerms.Count && search.Count > 0; ++i)
            {
                search = NextSearch(search, restOfTheSearchTerms[i]);
            }

            CompleteSearch(result, search);
            return result;
        }

        private (string, List<string>) BestSearchTerm(string [] searchTermArray)
        {
            string bestSearchTerm;
            List<string> searchTerms = new List<string>(searchTermArray);
            List<string> rest;
            if ((bestSearchTerm = searchTerms.Find(st => Regex.Match(st, "9\\d{3}").Success)) != null)
            {
                rest = searchTerms.Where(st => st != bestSearchTerm).ToList();
                return (bestSearchTerm, rest);
            } else if ((bestSearchTerm = searchTerms.Find(st => Regex.Match(st, "[A-Z]{14}-[A-Z]{10}-[A-Z]").Success)) != null)
            {
                rest = searchTerms.Where(st => st != bestSearchTerm).ToList();
                return (bestSearchTerm, rest);
            } else
            {
                var orderedByLength = searchTerms.OrderByDescending(st => st.Length);
                bestSearchTerm = orderedByLength.First();
                rest = orderedByLength.Skip(1).ToList();
                return (bestSearchTerm, rest);
            }
        }

        private IList<int> FirstSearch(string searchTerm)
        {
            string query;
            if (Regex.Match(searchTerm, "9\\d{3}").Success)
            {
                query = NUM9000_QUERY;
            } else if (Regex.Match(searchTerm, "[A-Z]{14}-[A-Z]{10}-[A-Z]").Success)
            {
                query = INCHI_KEY_QUERY;
            } else
            {
                query = DESCRIPTOR_QUERY;
            }

            using var con = new NpgsqlConnection(url);
            con.Open();

            using var cmd = new NpgsqlCommand(query, con);
            cmd.Parameters.AddWithValue("searchTerm", searchTerm);

            var result = new List<int>();
            using NpgsqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                result.Add(rdr.GetInt32(0));
            }
            return result;
        }

        private IList<int> NextSearch(IList<int> entityIds, string searchTerm)
        {
            string query;
            if (Regex.Match(searchTerm, "9\\d{3}").Success)
            {
                query = LATER_NUM9000_QUERY;
            } else if (Regex.Match(searchTerm, "[A-Z]{14}-[A-Z]{10}-[A-Z]").Success)
            {
                query = LATER_INCHI_KEY_QUERY;
            } else
            {
                query = LATER_DESCRIPTOR_QUERY;
            }

            using var con = new NpgsqlConnection(url);
            con.Open();

            using var cmd = new NpgsqlCommand(query, con);
            cmd.Parameters.AddWithValue("searchTerm", searchTerm);
            cmd.Parameters.Add("@entityIdList", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = entityIds;

            var result = new List<int>();
            using NpgsqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                result.Add(rdr.GetInt32(0));
            }
            return result;
        }

        private void CompleteSearch(SearchResult result, IList<int> entityIds)
        {
            using var con = new NpgsqlConnection(url);
            con.Open();

            using var cmd = new NpgsqlCommand(FINAL_QUERY, con);
            cmd.Parameters.Add("@entityIdList", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = entityIds;

            using NpgsqlDataReader rdr = cmd.ExecuteReader();
            int prevEntityId = -1;
            int entityId = -2;
            int num9000 = 0;
            string inchiKey = "";
            List<string> descriptorList = new List<string>();
            while (rdr.Read())
            {
                entityId = rdr.GetInt32(1);
                if (entityId != prevEntityId && prevEntityId != -1)
                {
                    Entity e = new Entity(prevEntityId, num9000, inchiKey, descriptorList);
                    result.AddEntity(e);
                    descriptorList = new List<string>();
                }
                prevEntityId = entityId;
                num9000 = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);
                inchiKey = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                if (!rdr.IsDBNull(4))
                {
                    descriptorList.Add(rdr.GetString(4));
                }
            }
            if (entityId != -1)
            {
                Entity e = new Entity(entityId, num9000, inchiKey, descriptorList);
                result.AddEntity(e);
            }
        }

        public string GetStructure(string inchiKey)
        {
            using var con = new NpgsqlConnection(url);
            con.Open();

            var sql = "select inchi from substances where inchi_key=@ik";
            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("ik", inchiKey);

            using NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                return rdr.GetString(0);
            } else
            {
                return null;
            }
        }
    }
}