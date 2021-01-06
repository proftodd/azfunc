using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My.Models
{
    public class Entity
    {
        public int Id { get; set; }

        public virtual Substance Sub { get; set; }

        public virtual Num9000 Num { get; set; }

        public virtual IList<Descriptor> Descriptors { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .Append(Num == null || Num.Num == 0 ? new string(' ', 5) : $"{Num.Num,4:#} ")
                .Append(Sub == null ? new string(' ', 28) : $"{Sub.InchiKey} ")
                .Append(string.Join(',', Descriptors.Select(d => d.Desc)))
                .ToString();
        }
    }

    public class Substance
    {
        public int EntityId { get; set; }

        public virtual Entity Entity { get; set; }

        public string InchiKey { get; set; }

        public string Inchi { get; set; }
    }

    public class Num9000
    {
        public int Num { get; set; }

        public int EntityId { get; set; }

        public virtual Entity Entity { get; set; }
    }

    public class Descriptor
    {
        public int DescriptorId { get; set; }

        public int EntityId { get; set; }

        public virtual Entity Entity { get; set; }

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
