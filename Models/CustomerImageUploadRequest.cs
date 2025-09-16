namespace CustomerLeadImageApi.Models
{
    public class CustomerImageUploadRequest
    {
        public string? Name { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }
}