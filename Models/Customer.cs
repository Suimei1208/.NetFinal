using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NetTechnology_Final.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string username { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email
        {
            get { return _email; }
            set
            {
                if (IsValidEmail(value))
                {
                    _email = value;
                }
                else
                {
                    throw new ArgumentException("Invalid email address");
                }
            }
        }

        private string _email;
        public string? password { get; set; }
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }

        public Customer()
        {
           Role = Role.Customer;
        }

        public string Address { get; set; }
        public string? Avatar { get; set; }
        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }

        private bool IsValidEmail(string email)
        {
            string pattern = @"@gmail\.com$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
    } 
}

