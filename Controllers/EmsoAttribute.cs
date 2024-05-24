using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace DSR.Controllers
{
   
    public class EMSOAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var emso = value as string;
            if (string.IsNullOrEmpty(emso))
            {
                return new ValidationResult("EMSO is required.");
            }

            if (emso.Length != 13)
            {
                return new ValidationResult("EMSO must be exactly 13 digits long.");
            }

            if (!emso.All(char.IsDigit))
            {
                return new ValidationResult("EMSO must contain only digits.");
            }

            return ValidationResult.Success;
        }


        private bool IsValidControlNumber(string emso)
        {
            // Implement the check for the control number as per EMSO specification
            // This is a placeholder for the actual implementation
            return true;
        }
    }



}
