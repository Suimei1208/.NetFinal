using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NetTechnology_Final.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Barcode { get; set; }
        [Required]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; } = string.Empty;
        [Required]
        public long ImportPrice { get; set; }
        [Required]
        public long RetailPrice { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
    }
}
