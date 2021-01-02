using System.Collections.Generic;
using System.Linq;

namespace My.Models
{
    public class Entity
    {
        public int Id { get; set; }

        public Substance Sub { get; set; }

        public Num9000 Num { get; set; }

        public IList<Descriptor> Descriptors { get; set; }

        override public string ToString()
        {
            var descriptorString = string.Join(", ", Descriptors.Select(d => d.Desc));
            return $"{(Num.Num == 0 ? "" : Num.Num.ToString()): %4s} {Sub.InchiKey: %27s} {descriptorString}";
        }
    }

    public class Substance
    {
        public Entity Entity { get; set; }

        public string InchiKey { get; set; }

        public string Inchi { get; set; }
    }

    public class Num9000
    {
        public int Num { get; set; }

        public Entity Entity { get; set; }
    }

    public class Descriptor
    {
        public int Id { get; set; }

        public Entity Entity { get; set; }

        public string Desc { get; set; }
    }

    public class SearchResult
    {
        public IList<string> searchTerms { get; }
        public IList<Entity> entities { get; }

        public SearchResult() : this(new List<string>(), new List<Entity>()) {}

        public SearchResult(IList<string> searchTerms, IList<Entity> entities)
        {
            this.searchTerms = searchTerms;
            this.entities = entities;
        }
    }
}
