namespace ApifySdk.Actors.Indeed.Models;

public class IndeedJobResult
{
    public string Key { get; set; } = null!;
    
    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;

    public DateTime DatePublished { get; set; }

    public JobLocation? Location { get; set; }

    public JobEmployer? Employer { get; set; }

    public JobSalary? BaseSalary { get; set; }

    public JobDescription Description { get; set; } = null!;

    /// <summary>
    /// Only the values are human-readable 'requirements'.<c>Attributes?.Values</c> yields <c>["C#", "Python", "C"]</c>.
    /// </summary>
    public Dictionary<string, string>? Attributes { get; set; }
}

public class JobLocation
{
    public string? City { get; set; }

    public string? CountryName { get; set; }
}

public class JobEmployer
{
    public string? Name { get; set; }

    public string? CompanyPageUrl { get; set; }
}

public class JobSalary
{
    public decimal? Min { get; set; }

    public decimal? Max { get; set; }

    public string? CurrencyCode { get; set; }
}

public class JobDescription
{
    public string Text { get; set; } = null!;
}
