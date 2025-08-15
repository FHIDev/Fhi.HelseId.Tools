namespace Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos
{
    internal record ProblemDetail(
    string Type,
    string Title,
    int Status,
    string Detail,
    string? Instance
    );
}
