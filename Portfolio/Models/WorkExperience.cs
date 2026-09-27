using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

public class WorkExperience
{
    public int Id { get; set; }
    public string Company { get; set; } = "";
    public string Position { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Summary { get; set; } = "";
    public string Details { get; set; } = "";
    public int Order { get; set; }
    public List<WorkExperienceTechnology> WorkExperienceTechnologies { get; set; } = [];

    [NotMapped] public string Duration => ExperienceDuration.Format(StartDate, IsCurrent ? DateOnly.FromDateTime(DateTime.Today) : EndDate);
}

public class WorkExperienceTechnology
{
    public int Id { get; set; }
    public int WorkExperienceId { get; set; }
    public int TechnologyId { get; set; }
    public Technology Technology { get; set; } = null!;
    public WorkExperience WorkExperience { get; set; } = null!;
}
