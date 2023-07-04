using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class RequestDto
    {
        [Required(ErrorMessage = "This field is required.")]
        [MinLength(4, ErrorMessage = "Minimum 4 characters are required.")]
        [MaxLength(250, ErrorMessage = "Maximum 250 characters are allowed.")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [MinLength(4, ErrorMessage = "Minimum 4 characters are required.")]
        [MaxLength(250, ErrorMessage = "Maximum 250 characters are allowed.")]
        public string Description { get; set; }

    }
}
