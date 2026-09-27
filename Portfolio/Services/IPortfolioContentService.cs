using Portfolio.Models;

namespace Portfolio.Services;

public interface IPortfolioContentService
{
    Task<SiteSettings?> GetSettingsAsync();
    Task<About?> GetAboutAsync();
    Task<List<Language>> GetLanguagesAsync();
    Task<List<Project>> GetProjectsAsync();
    Task<List<WorkExperience>> GetExperiencesAsync();
    Task<List<EducationItem>> GetEducationAsync();
    Task<List<Technology>> GetTechnologiesAsync();
    Task SaveProjectAsync(Project item);
    Task DeleteProjectAsync(int id);
    Task SaveExperienceAsync(WorkExperience item);
    Task DeleteExperienceAsync(int id);
    Task SaveEducationAsync(EducationItem item);
    Task DeleteEducationAsync(int id);
    Task SaveTechnologyAsync(Technology item);
    Task DeleteTechnologyAsync(int id);
    Task SaveLanguageAsync(Language item);
    Task DeleteLanguageAsync(int id);
    Task SaveSettingsAsync(SiteSettings item);
    Task SaveAboutAsync(About item);
    Task DeleteAsync<T>(int id) where T : class;
    Task<int[]> GetCountsAsync();
}
