using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleDomainModel
{
    public class ChocolateBox
    {
        public ICollection<Chocolate> Contents { get; set; }

        [Required]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Wrapper { get; set; }
    }

    public class Chocolate
    {
        public bool Dark { get; set; }

        public string Filling { get; set; }

        public bool HasWrapper { get; set; }

        [Required]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
