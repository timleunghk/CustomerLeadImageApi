using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerLeadImageApi.Models
{
    public class CustomerImage
    {
        public int Id { get; set; }

        [Required]
        public string Base64Data { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        [JsonIgnore]   // 👈 prevent infinite loop during JSON serialization
        public Customer Customer { get; set; }
    }
}