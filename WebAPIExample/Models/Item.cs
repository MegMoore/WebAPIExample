using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPIExample.Models
{
    [Index("UPC", IsUnique = true)]
    public class Item
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string UPC { get; set; } = string.Empty;
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "decimal(11,2)")]
        public decimal Price { get; set; } = 0;
        public bool Active { get; set; } = true;

        
    }
}
