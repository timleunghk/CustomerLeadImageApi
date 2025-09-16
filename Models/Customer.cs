using System.ComponentModel.DataAnnotations;

namespace CustomerLeadImageApi.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public ICollection<CustomerImage> Images { get; set; } = new List<CustomerImage>();
    }
}