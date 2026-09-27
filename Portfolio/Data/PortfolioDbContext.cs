using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data;

public class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<About> About => Set<About>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<WorkExperienceTechnology> WorkExperienceTechnologies => Set<WorkExperienceTechnology>();
    public DbSet<EducationItem> EducationItems => Set<EducationItem>();
    public DbSet<Technology> Technologies => Set<Technology>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Project>().HasMany(x => x.ProjectTechnologies).WithOne(x => x.Project).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<WorkExperience>().HasMany(x => x.WorkExperienceTechnologies).WithOne(x => x.WorkExperience)
            .HasForeignKey(x => x.WorkExperienceId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProjectTechnology>().HasOne(x => x.Technology).WithMany(x => x.ProjectTechnologies)
            .HasForeignKey(x => x.TechnologyId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<WorkExperienceTechnology>().HasOne(x => x.Technology).WithMany(x => x.WorkExperienceTechnologies)
            .HasForeignKey(x => x.TechnologyId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProjectTechnology>().HasIndex(x => new { x.ProjectId, x.TechnologyId }).IsUnique();
        b.Entity<WorkExperienceTechnology>().HasIndex(x => new { x.WorkExperienceId, x.TechnologyId }).IsUnique();
        b.Entity<WorkExperience>().ToTable(t => t.HasCheckConstraint(
            "CK_WorkExperiences_CurrentHasNoEndDate", "\"IsCurrent\" = 0 OR \"EndDate\" IS NULL"));
        b.Entity<Language>().ToTable(t => t.HasCheckConstraint(
            "CK_Languages_Proficiency_Range", "\"Proficiency\" >= 0 AND \"Proficiency\" <= 100"));
        b.Entity<Technology>().HasData(
            new Technology
                { Id = 1, Name = "Figma", Icon = "/images/technologies/figma.png", Category = "Design", Order = 0 },
            new Technology
                { Id = 2, Name = "Canva", Icon = "/images/technologies/canva.png", Category = "Design", Order = 1 },
            new Technology
            {
                Id = 3, Name = "Adobe Photoshop", Icon = "/images/technologies/photoshop.png", Category = "Design",
                Order = 2
            },
            new Technology
            {
                Id = 4, Name = "Adobe Illustrator", Icon = "/images/technologies/illustrator.png", Category = "Design",
                Order = 3
            },
            new Technology
                { Id = 5, Name = "Adobe XD", Icon = "/images/technologies/xd.png", Category = "Design", Order = 4 },
            new Technology
            {
                Id = 6, Name = "C#", Icon = "/images/technologies/csharp.png", Category = "Backend e Banco de Dados",
                Order = 5
            },
            new Technology
            {
                Id = 7, Name = ".NET", Icon = "/images/technologies/dotnet.png", Category = "Backend e Banco de Dados",
                Order = 6
            },
            new Technology
            {
                Id = 8, Name = "SQL", Icon = "/images/technologies/sql.png", Category = "Backend e Banco de Dados",
                Order = 7
            },
            new Technology
                { Id = 9, Name = "HTML", Icon = "/images/technologies/html.png", Category = "Front-End", Order = 8 },
            new Technology
                { Id = 10, Name = "CSS", Icon = "/images/technologies/css.png", Category = "Front-End", Order = 9 },
            new Technology
            {
                Id = 11, Name = "JavaScript", Icon = "/images/technologies/javascript.png", Category = "Front-End",
                Order = 10
            },
            new Technology
            {
                Id = 12, Name = "Blazor", Icon = "/images/technologies/blazor.png", Category = "Front-End", Order = 11
            },
            new Technology
            {
                Id = 13, Name = "Git", Icon = "/images/technologies/git.png", Category = "DevOps e Desenvolvimento",
                Order = 12
            },
            new Technology
            {
                Id = 14, Name = "GitHub", Icon = "/images/technologies/github.png",
                Category = "DevOps e Desenvolvimento", Order = 13
            },
            new Technology
            {
                Id = 15, Name = "Docker", Icon = "/images/technologies/docker.png",
                Category = "DevOps e Desenvolvimento", Order = 14
            },
            new Technology
            {
                Id = 16, Name = "Unity", Icon = "/images/technologies/unity.png", Category = "DevOps e Desenvolvimento",
                Order = 15
            });
        b.Entity<SiteSettings>().HasData(new SiteSettings
        {
            Id = 1, Name = "Alysson Serafim Brito", ProfessionalTitle = "Desenvolvedor de Jogos e Softwares",
            Email = "alyssonsb.dev@gmail.com", GithubUrl = "#", LinkedInUrl = "#", CvUrl = "#projetos",
            ProfileImage = "/images/Image.png"
        });
        b.Entity<About>().HasData(new About { Id = 1, Title = "Sobre", Description = "", Image = "/images/Image.png" });
        b.Entity<Language>().HasData(new Language { Id = 1, Name = "Português", Proficiency = 0, Order = 0 },
            new Language { Id = 2, Name = "Inglês", Proficiency = 0, Order = 1 });
    }
}
