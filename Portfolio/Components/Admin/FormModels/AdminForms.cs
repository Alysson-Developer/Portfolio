using System.ComponentModel.DataAnnotations;
using Portfolio.Models;

namespace Portfolio.Components.Admin.FormModels;

public sealed class ProjectForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o nome do projeto.")] public string Name { get; set; } = "";
    public string Organization { get; set; } = "";
    public string Description { get; set; } = "";
    [Url(ErrorMessage = "Informe uma URL válida.")] public string? Url { get; set; }
    public string Image { get; set; } = "";
    public int Order { get; set; }
    public List<int> TechnologyIds { get; set; } = [];
}

public sealed class ExperienceForm : IValidatableObject
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe a empresa.")] public string Company { get; set; } = "";
    [Required(ErrorMessage = "Informe o cargo.")] public string Position { get; set; } = "";
    [Required(ErrorMessage = "Informe a data inicial.")] public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string Duration => ExperienceDuration.Format(StartDate, IsCurrent ? DateOnly.FromDateTime(DateTime.Today) : EndDate);
    public string Summary { get; set; } = "";
    public string Details { get; set; } = "";
    public int Order { get; set; }
    public List<int> TechnologyIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsCurrent && StartDate is not null && EndDate is not null && EndDate < StartDate)
            yield return new ValidationResult("A data final deve ser igual ou posterior à data inicial.", [nameof(EndDate)]);
    }
}

public sealed class EducationForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe a instituição.")] public string Institution { get; set; } = "";
    [Required(ErrorMessage = "Informe o título.")] public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [Url(ErrorMessage = "Informe uma URL válida.")] public string? Url { get; set; }
    public string Image { get; set; } = "";
    public bool IsAcademic { get; set; }
    public int Order { get; set; }
}

public sealed class TechnologyForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o nome da tecnologia.")] public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    [Required(ErrorMessage = "Selecione uma categoria.")] public string Category { get; set; } = "";
    [Url(ErrorMessage = "Informe uma URL válida.")] public string? Url { get; set; }
    public int Order { get; set; }
}

public sealed class SettingsForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o nome.")] public string Name { get; set; } = "";
    [Required(ErrorMessage = "Informe o título profissional.")] public string ProfessionalTitle { get; set; } = "";
    [Required(ErrorMessage = "Informe o e-mail.")][EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")] public string Email { get; set; } = "";
    public string GithubUrl { get; set; } = "";
    public string LinkedInUrl { get; set; } = "";
    public string CvUrl { get; set; } = "";
    public string ProfileImage { get; set; } = "";
}

public sealed class AboutForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o título.")] public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
}

public sealed class LanguageForm
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o idioma.")] public string Name { get; set; } = "";
    [Range(0, 100, ErrorMessage = "A proficiência deve estar entre 0 e 100.")] public int Proficiency { get; set; }
    public int Order { get; set; }
}
