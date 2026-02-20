namespace CleanArchitectureIdentityTemplate.Application.ResultModels
{
    public class BaseResult
    {
        public bool Succeeded { get; set; }
        public int? StatusCode { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
