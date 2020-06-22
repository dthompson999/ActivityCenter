using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace ActivityCenter.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "First Name should be at least 2 characters.")]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Last Name should be at least 2 characters.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
        [DataType(DataType.Password)]
        [StrongPassword]
        public string Password { get; set; }

        [NotMapped]
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPW { get; set; }

        // public List<Transaction> CreatedTransactions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        public class StrongPasswordAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var reg = new Regex(@"^(?=.*?\d)(?=.*?[A-Z])(?=.*?[a-z])[A-Za-z\d,!@#$%^&*+=]{8,}$");
                if ((string)value == null)
                {
                    return new ValidationResult("Password must have at least 1 number, 1 letter, 1 Uppercase letter and 1 special character.");
                }
                else
                {
                    if (!reg.IsMatch((string)value))
                        return new ValidationResult("Password must have at least 1 number, 1 letter, 1 Uppercase letter and 1 special character.");
                    return ValidationResult.Success;
                }
            }
        }
    }
}