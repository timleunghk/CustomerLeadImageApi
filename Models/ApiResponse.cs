namespace CustomerLeadImageApi.Models
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } = "success"; // "success" or "error"
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}