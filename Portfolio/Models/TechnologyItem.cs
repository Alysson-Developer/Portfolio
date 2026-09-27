namespace Portfolio.Models;

public class Technology
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Url { get; set; }
    public int Order { get; set; }
    public List<ProjectTechnology> ProjectTechnologies { get; set; } = [];
    public List<WorkExperienceTechnology> WorkExperienceTechnologies { get; set; } = [];
}

// Compatibility with the existing public technology card component.
public class TechnologyItem : Technology
{
}
