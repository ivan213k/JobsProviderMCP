namespace ApifySdk.Actors.Indeed.Models;

public class IndeedJobResult
{
    public string Title { get; set; } = default!;

    public string Url { get; set; } = default!;

    public DateTime? DatePublished { get; set; }

    public JobLocation? Location { get; set; }

    public JobEmployer? Employer { get; set; }

    public JobSalary? BaseSalary { get; set; }

    public JobDescription Description { get; set; } = default!;

    // Keys are Indeed's internal taxonomy IDs (e.g. "X62BT" -> "Python") and
    // aren't stable/named, so a Dictionary is the right shape here rather
    // than fixed properties.
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
    public string Text { get; set; } = default!;
}
