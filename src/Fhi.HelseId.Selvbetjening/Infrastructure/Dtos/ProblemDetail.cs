namespace Fhi.HelseIdSelvbetjening.Infrastructure.Dtos
{
    internal record ProblemDetail(
    string Type,
    string Title,
    int Status,
    string Detail,
    string? Instance
    );
}
