﻿using System.ComponentModel.DataAnnotations.Schema;
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
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }
        public string Name { get; set; }
        [NotMapped] // Để không được ánh xạ vào cơ sở dữ liệu
        public IFormFile AvatarFile { get; set; }
        public string? Avatar { get; set; }
        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }
        public DateTime CreateDate { get; set; }
        public Accounts()
        {
            IFormFile avatarFile = null;
            // Sử dụng giá trị mặc định khác Stream.Null
            AvatarFile = avatarFile ?? new FormFile(Stream.Null, 0, 0, "AvatarFile", "avatar.jpg");
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
        Salesperson, Customer
    }

    public enum Status
    {
        Active,
        InActive,
        Block
    }
}
