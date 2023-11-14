using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NetTechnology_Final.Models
{
    public class Accounts
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
        [Required]
        public string password { get; set; }
        public Role Role { get; set; }
        public string Phone {  get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ProfilePicture { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Accounts()
        {
            
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"@gmail\.com$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
    }

    public enum Role
    {
        Admin,
        Salesperson, 
        Customer
    }

    public enum Status
    {
        Active,
        InActive,
        Block
    }
}
