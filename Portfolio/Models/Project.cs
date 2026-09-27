using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    [NotMapped]
    public string Title
    {
        get => Name;
        set => Name = value;
    }

    public string Organization { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string? Url { get; set; }
    public int Order { get; set; }
    public List<ProjectTechnology> ProjectTechnologies { get; set; } = [];

}

public class ProjectTechnology
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int TechnologyId { get; set; }
    public Technology Technology { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
