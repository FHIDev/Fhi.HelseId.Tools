namespace Fhi.HelseIdSelvbetjening.Infrastructure.Dtos
{
    internal class ProblemDetail
    {
        public required string Type { get; set; }

        public required string Title { get; set; }
        public int Status { get; set; }
        public required string Detail { get; set; }

        public string? Instance { get; set; }
    }
}
