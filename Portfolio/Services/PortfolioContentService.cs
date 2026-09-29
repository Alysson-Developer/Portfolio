using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public class PortfolioContentService(
    IDbContextFactory<PortfolioDbContext> factory) : IPortfolioContentService
{
    public async Task<SiteSettings?> GetSettingsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.SiteSettings
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<About?> GetAboutAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.About
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Languages
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Projects
            .AsNoTracking()
            .Include(x => x.ProjectTechnologies)
            .ThenInclude(x => x.Technology)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    public async Task<List<WorkExperience>> GetExperiencesAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.WorkExperiences
            .AsNoTracking()
            .Include(x => x.WorkExperienceTechnologies)
            .ThenInclude(x => x.Technology)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    public async Task<List<EducationItem>> GetEducationAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.EducationItems
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    public async Task<List<Technology>> GetTechnologiesAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Technologies
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .ToListAsync();
    }

    public async Task SaveProjectAsync(Project item)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = item.Id == 0
            ? new Project()
            : await db.Projects
                .Include(x => x.ProjectTechnologies)
                .SingleAsync(x => x.Id == item.Id);

        entity.Name = item.Name;
        entity.Organization = item.Organization;
        entity.Description = item.Description;
        entity.Image = item.Image;
        entity.Url = item.Url;
        entity.Order = item.Order;

        await SyncTechnologyIdsAsync(
            db,
            item.ProjectTechnologies.Select(x => x.TechnologyId),
            entity.ProjectTechnologies,
            entity.ProjectTechnologies.Select(x => x.TechnologyId),
            id => entity.ProjectTechnologies.Add(
                new ProjectTechnology
                {
                    TechnologyId = id
                }));

        if (item.Id == 0)
            db.Projects.Add(entity);

        await db.SaveChangesAsync();

        item.Id = entity.Id;
    }

    public async Task DeleteProjectAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = await db.Projects
            .SingleOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return;

        db.Projects.Remove(entity);

        await db.SaveChangesAsync();
    }

    public async Task SaveExperienceAsync(WorkExperience item)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = item.Id == 0
            ? new WorkExperience()
            : await db.WorkExperiences
                .Include(x => x.WorkExperienceTechnologies)
                .SingleAsync(x => x.Id == item.Id);

        entity.Company = item.Company;
        entity.Position = item.Position;
        entity.StartDate = item.StartDate;
        entity.IsCurrent = item.IsCurrent;
        entity.EndDate = item.IsCurrent ? null : item.EndDate;
        entity.Summary = item.Summary;
        entity.Details = item.Details;
        entity.Order = item.Order;

        await SyncTechnologyIdsAsync(
            db,
            item.WorkExperienceTechnologies.Select(x => x.TechnologyId),
            entity.WorkExperienceTechnologies,
            entity.WorkExperienceTechnologies.Select(x => x.TechnologyId),
            id => entity.WorkExperienceTechnologies.Add(
                new WorkExperienceTechnology
                {
                    TechnologyId = id
                }));

        if (item.Id == 0)
            db.WorkExperiences.Add(entity);

        await db.SaveChangesAsync();

        item.Id = entity.Id;
    }

    public async Task DeleteExperienceAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = await db.WorkExperiences
            .SingleOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return;

        db.WorkExperiences.Remove(entity);

        await db.SaveChangesAsync();
    }

    public Task SaveEducationAsync(EducationItem item)
    {
        return SaveSimpleAsync(
            item.Id,
            item,
            db => db.EducationItems.SingleOrDefaultAsync(x => x.Id == item.Id),
            (db, entity) => db.EducationItems.Add(entity),
            id => item.Id = id);
    }

    public Task DeleteEducationAsync(int id)
    {
        return DeleteSimpleAsync<EducationItem>(id);
    }

    public Task SaveTechnologyAsync(Technology item)
    {
        return SaveSimpleAsync(
            item.Id,
            item,
            db => db.Technologies.SingleOrDefaultAsync(x => x.Id == item.Id),
            (db, entity) => db.Technologies.Add(entity),
            id => item.Id = id);
    }

    public Task DeleteTechnologyAsync(int id)
    {
        return DeleteSimpleAsync<Technology>(id);
    }

    public Task SaveLanguageAsync(Language item)
    {
        return SaveSimpleAsync(
            item.Id,
            item,
            db => db.Languages.SingleOrDefaultAsync(x => x.Id == item.Id),
            (db, entity) => db.Languages.Add(entity),
            id => item.Id = id);
    }

    public Task DeleteLanguageAsync(int id)
    {
        return DeleteSimpleAsync<Language>(id);
    }

    public async Task SaveSettingsAsync(SiteSettings item)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = await db.SiteSettings
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            entity = new SiteSettings();
            db.SiteSettings.Add(entity);
        }

        entity.Name = item.Name;
        entity.ProfessionalTitle = item.ProfessionalTitle;
        entity.Email = item.Email;
        entity.GithubUrl = item.GithubUrl;
        entity.LinkedInUrl = item.LinkedInUrl;
        entity.CvUrl = item.CvUrl;
        entity.ProfileImage = item.ProfileImage;

        var about = await db.About
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (about is not null)
            about.Image = item.ProfileImage;

        await db.SaveChangesAsync();

        item.Id = entity.Id;
    }

    public async Task SaveAboutAsync(About item)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = await db.About
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            entity = new About();
            db.About.Add(entity);
        }

        entity.Title = item.Title;
        entity.Description = item.Description;
        entity.Image = item.Image;

        var settings = await db.SiteSettings
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (settings is not null)
            settings.ProfileImage = item.Image;

        await db.SaveChangesAsync();

        item.Id = entity.Id;
    }

    public async Task DeleteAsync<T>(int id) where T : class
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = await db.Set<T>().FindAsync(id);

        if (entity is null)
            return;

        db.Remove(entity);

        await db.SaveChangesAsync();
    }

    public async Task<int[]> GetCountsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        var projects = await db.Projects.CountAsync();
        var experiences = await db.WorkExperiences.CountAsync();
        var education = await db.EducationItems.CountAsync();
        var technologies = await db.Technologies.CountAsync();

        return [projects, experiences, education, technologies];
    }

    private async Task SaveSimpleAsync<TEntity>(
        int id,
        TEntity incoming,
        Func<PortfolioDbContext, Task<TEntity?>> find,
        Action<PortfolioDbContext, TEntity> add,
        Action<int> setId)
        where TEntity : class
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = id == 0
            ? null
            : await find(db);

        if (entity is null)
        {
            entity = incoming;
            add(db, entity);
        }
        else
        {
            db.Entry(entity)
                .CurrentValues
                .SetValues(incoming);
        }

        await db.SaveChangesAsync();

        setId((int)db.Entry(entity).Property("Id").CurrentValue!);
    }

    private async Task DeleteSimpleAsync<TEntity>(int id)
        where TEntity : class
    {
        await using var db = await factory.CreateDbContextAsync();

        var set = db.Set<TEntity>();

        var entity = await set.FindAsync(id);

        if (entity is null)
            return;

        set.Remove(entity);

        await db.SaveChangesAsync();
    }

    private async Task SyncTechnologyIdsAsync<TChild>(
        PortfolioDbContext db,
        IEnumerable<int> requested,
        ICollection<TChild> current,
        IEnumerable<int> existingIds,
        Action<int> add)
        where TChild : class
    {
        var requestedIds = requested
            .Where(x => x > 0)
            .ToHashSet();

        var existing = existingIds.ToHashSet();

        var resolved = await db.Technologies
            .Where(x => requestedIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (resolved.Count != requestedIds.Count)
        {
            throw new InvalidOperationException(
                "A seleção contém uma tecnologia que não existe mais.");
        }

        foreach (var child in current.ToList())
        {
            var id = child switch
            {
                ProjectTechnology projectTechnology =>
                    projectTechnology.TechnologyId,

                WorkExperienceTechnology experienceTechnology =>
                    experienceTechnology.TechnologyId,

                _ => throw new InvalidOperationException(
                    "Tipo de relacionamento de tecnologia inválido.")
            };

            if (!requestedIds.Contains(id))
                current.Remove(child);
        }

        foreach (var id in requestedIds.Except(existing))
            add(id);
    }
}