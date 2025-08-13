namespace IKARUSWEB.UI.Models.Api
{
    public sealed class ProblemDetailsDto
    {
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
    }

    public sealed class ApiErrorEnvelope
    {
        public ProblemDetailsDto? Problem { get; set; }
        public List<ApiFieldError>? Errors { get; set; }
    }

    public sealed class ApiFieldError
    {
        public string? Field { get; set; }   // "name", "country" ...
        public string? Code { get; set; }    // "Validation.Tenant.Name.Required" ...
        public string? Message { get; set; } // Lokalize mesaj (TR/EN)
    }

    public sealed class ApiException : Exception
    {
        public ProblemDetailsDto? Problem { get; }
        public List<ApiFieldError> Errors { get; }

        public ApiException(ProblemDetailsDto? p, List<ApiFieldError>? e, string? fallback)
            : base(fallback ?? p?.Title)
        {
            Problem = p;
            Errors = e ?? new();
        }
    }
}
