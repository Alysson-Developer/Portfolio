namespace Portfolio.Models;

public class EducationItem
{
    public int Id { get; set; }
    public string Institution { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Image { get; set; } = "";
    public string? Url { get; set; }
    public bool IsAcademic { get; set; }
    public int Order { get; set; }
}
