namespace Market.Application.Modules.Staff.Queries.List;

public sealed class ListStaffQueryValidator : AbstractValidator<ListStaffQuery>
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "firstName",
        "lastName",
        "hireDate",
        "position"
    };

    public ListStaffQueryValidator()
    {
        RuleFor(x => x.Paging.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Sort)
            .Must(BeValidSort)
            .WithMessage("Sort must be one of: firstName, lastName, hireDate, position. Prefix with '-' for descending.")
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));
    }

    private static bool BeValidSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return true;

        var trimmed = sort.Trim();
        var field = trimmed.StartsWith('-') ? trimmed[1..] : trimmed;
        return AllowedSortFields.Contains(field);
    }
}
