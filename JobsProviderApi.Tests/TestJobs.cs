using JobsProviderApi.Models;

namespace JobsProviderApi.Tests;

internal static class TestJobs
{
    public static Job Create(
        int id,
        string title = "Backend Engineer",
        string description = "General description.",
        params string[] requirements) =>
        new(
            Id: id,
            Title: title,
            Company: $"Company {id}",
            Location: "Remote",
            Description: description,
            Requirements: requirements,
            Link: $"https://example.com/jobs/{id}",
            SourcingPlatform: "Test",
            DatePublished: new DateTime(2026-01-01));
}
