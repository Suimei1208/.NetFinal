using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NetTechnology_Final.Models
{
    public class Orders
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Required]
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Accounts Account { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public long UnitPrice { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }
    }
}
