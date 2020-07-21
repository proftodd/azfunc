using System.Collections.Generic;

namespace My.Models
{
    public class Entity
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

    public class SearchResult
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
}
