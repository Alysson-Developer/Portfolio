namespace Portfolio.Models;

public class SiteSettings
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ProfessionalTitle { get; set; } = "";
    public string Email { get; set; } = "";
    public string GithubUrl { get; set; } = "";
    public string LinkedInUrl { get; set; } = "";
    public string CvUrl { get; set; } = "";
    public string ProfileImage { get; set; } = "";
}