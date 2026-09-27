using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations;

public partial class ContentDatesTechnologyLinksAndLanguageLevels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE ContentMigrationIssues (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                EntityType TEXT NOT NULL,
                EntityId INTEGER NOT NULL,
                FieldName TEXT NOT NULL,
                OriginalValue TEXT NOT NULL,
                Reason TEXT NOT NULL
            );
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'WorkExperience', Id, 'Duration', Duration, 'Duração antiga guardada para auditoria; agora é calculada a partir das datas.'
            FROM WorkExperiences WHERE trim(Duration) <> '';
            """);

        migrationBuilder.Sql("""
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'ProjectTechnology', old.ProjectId, 'Technology', old.Name,
                   'Não havia Technology cadastrada com este nome; associação antiga preservada para revisão.'
            FROM ProjectTechnologies old
            WHERE NOT EXISTS (SELECT 1 FROM Technologies t WHERE lower(trim(t.Name)) = lower(trim(old.Name)));
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'WorkExperienceTechnology', old.WorkExperienceId, 'Technology', old.Name,
                   'Não havia Technology cadastrada com este nome; associação antiga preservada para revisão.'
            FROM WorkExperienceTechnologies old
            WHERE NOT EXISTS (SELECT 1 FROM Technologies t WHERE lower(trim(t.Name)) = lower(trim(old.Name)));
            """);

        migrationBuilder.Sql("""
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'Language', Id, 'Proficiency', Proficiency,
                   'Proficiência antiga não era um inteiro válido entre 0 e 100; foi convertida para 0.'
            FROM Languages
            WHERE length(replace(trim(Proficiency), '%', '')) = 0
               OR replace(trim(Proficiency), '%', '') GLOB '*[^0-9]*'
               OR CAST(replace(trim(Proficiency), '%', '') AS INTEGER) NOT BETWEEN 0 AND 100;
            """);

        migrationBuilder.Sql($"""
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'WorkExperience', Id, 'StartDate', StartDate, 'Data antiga não reconhecida; valor preservado para revisão.'
            FROM WorkExperiences WHERE trim(StartDate) <> '' AND NOT ({ValidDate("StartDate")});
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'WorkExperience', Id, 'EndDate', EndDate, 'Data final antiga não reconhecida; valor preservado para revisão.'
            FROM WorkExperiences WHERE trim(EndDate) <> '' AND lower(trim(EndDate)) NOT IN ('presente', 'present', 'current')
              AND NOT ({ValidDate("EndDate")});
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'EducationItem', Id, 'StartDate', StartDate, 'Data antiga não reconhecida; valor preservado para revisão.'
            FROM EducationItems WHERE trim(StartDate) <> '' AND NOT ({ValidDate("StartDate")});
            INSERT INTO ContentMigrationIssues(EntityType, EntityId, FieldName, OriginalValue, Reason)
            SELECT 'EducationItem', Id, 'EndDate', EndDate, 'Data antiga não reconhecida; valor preservado para revisão.'
            FROM EducationItems WHERE trim(EndDate) <> '' AND NOT ({ValidDate("EndDate")});
            """);

        migrationBuilder.Sql("""
            ALTER TABLE WorkExperiences ADD COLUMN IsCurrent INTEGER NOT NULL DEFAULT 0;
            ALTER TABLE WorkExperiences RENAME COLUMN StartDate TO StartDateLegacy;
            ALTER TABLE WorkExperiences RENAME COLUMN EndDate TO EndDateLegacy;
            ALTER TABLE WorkExperiences ADD COLUMN StartDate TEXT NULL;
            ALTER TABLE WorkExperiences ADD COLUMN EndDate TEXT NULL;
            """);
        migrationBuilder.Sql($"""
            UPDATE WorkExperiences SET
                StartDate = {ValidDate("StartDateLegacy")},
                IsCurrent = CASE WHEN lower(trim(EndDateLegacy)) IN ('presente', 'present', 'current') THEN 1 ELSE 0 END,
                EndDate = CASE WHEN lower(trim(EndDateLegacy)) IN ('presente', 'present', 'current') THEN NULL ELSE {ValidDate("EndDateLegacy")} END;
            ALTER TABLE WorkExperiences DROP COLUMN StartDateLegacy;
            ALTER TABLE WorkExperiences DROP COLUMN EndDateLegacy;
            ALTER TABLE WorkExperiences DROP COLUMN Duration;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE EducationItems RENAME COLUMN StartDate TO StartDateLegacy;
            ALTER TABLE EducationItems RENAME COLUMN EndDate TO EndDateLegacy;
            ALTER TABLE EducationItems ADD COLUMN StartDate TEXT NULL;
            ALTER TABLE EducationItems ADD COLUMN EndDate TEXT NULL;
            """);
        migrationBuilder.Sql($"""
            UPDATE EducationItems SET StartDate = {ValidDate("StartDateLegacy")}, EndDate = {ValidDate("EndDateLegacy")};
            ALTER TABLE EducationItems DROP COLUMN StartDateLegacy;
            ALTER TABLE EducationItems DROP COLUMN EndDateLegacy;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE Languages RENAME COLUMN Proficiency TO ProficiencyLegacy;
            ALTER TABLE Languages ADD COLUMN Proficiency INTEGER NOT NULL DEFAULT 0;
            UPDATE Languages SET Proficiency = CASE
                WHEN length(replace(trim(ProficiencyLegacy), '%', '')) > 0
                 AND replace(trim(ProficiencyLegacy), '%', '') NOT GLOB '*[^0-9]*'
                 AND CAST(replace(trim(ProficiencyLegacy), '%', '') AS INTEGER) BETWEEN 0 AND 100
                THEN CAST(replace(trim(ProficiencyLegacy), '%', '') AS INTEGER)
                ELSE 0 END;
            ALTER TABLE Languages DROP COLUMN ProficiencyLegacy;
            """);

        CreateTechnologyLinks(migrationBuilder, "ProjectTechnologies", "ProjectId", "ProjectTechnology");
        CreateTechnologyLinks(migrationBuilder, "WorkExperienceTechnologies", "WorkExperienceId", "WorkExperienceTechnology");

        migrationBuilder.Sql("""
            CREATE TRIGGER TR_WorkExperiences_CurrentDate_Insert BEFORE INSERT ON WorkExperiences
            WHEN NEW.IsCurrent=1 AND NEW.EndDate IS NOT NULL
            BEGIN SELECT RAISE(ABORT, 'IsCurrent experiences must not have an EndDate.'); END;
            CREATE TRIGGER TR_WorkExperiences_CurrentDate_Update BEFORE UPDATE OF IsCurrent,EndDate ON WorkExperiences
            WHEN NEW.IsCurrent=1 AND NEW.EndDate IS NOT NULL
            BEGIN SELECT RAISE(ABORT, 'IsCurrent experiences must not have an EndDate.'); END;
            CREATE TRIGGER TR_Languages_Proficiency_Insert BEFORE INSERT ON Languages
            WHEN NEW.Proficiency<0 OR NEW.Proficiency>100
            BEGIN SELECT RAISE(ABORT, 'Language proficiency must be between 0 and 100.'); END;
            CREATE TRIGGER TR_Languages_Proficiency_Update BEFORE UPDATE OF Proficiency ON Languages
            WHEN NEW.Proficiency<0 OR NEW.Proficiency>100
            BEGIN SELECT RAISE(ABORT, 'Language proficiency must be between 0 and 100.'); END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER IF EXISTS TR_WorkExperiences_CurrentDate_Insert;
            DROP TRIGGER IF EXISTS TR_WorkExperiences_CurrentDate_Update;
            DROP TRIGGER IF EXISTS TR_Languages_Proficiency_Insert;
            DROP TRIGGER IF EXISTS TR_Languages_Proficiency_Update;
            """);
        RestoreTechnologyLinks(migrationBuilder, "ProjectTechnologies", "ProjectId", "ProjectTechnology");
        RestoreTechnologyLinks(migrationBuilder, "WorkExperienceTechnologies", "WorkExperienceId", "WorkExperienceTechnology");

        migrationBuilder.Sql("""
            ALTER TABLE WorkExperiences ADD COLUMN Duration TEXT NOT NULL DEFAULT '';
            UPDATE WorkExperiences SET Duration = COALESCE((SELECT OriginalValue FROM ContentMigrationIssues i
                WHERE i.EntityType='WorkExperience' AND i.EntityId=WorkExperiences.Id AND i.FieldName='Duration' ORDER BY i.Id LIMIT 1), '');
            ALTER TABLE WorkExperiences RENAME COLUMN StartDate TO StartDateTyped;
            ALTER TABLE WorkExperiences RENAME COLUMN EndDate TO EndDateTyped;
            ALTER TABLE WorkExperiences ADD COLUMN StartDate TEXT NOT NULL DEFAULT '';
            ALTER TABLE WorkExperiences ADD COLUMN EndDate TEXT NOT NULL DEFAULT '';
            UPDATE WorkExperiences SET StartDate=COALESCE(strftime('%d/%m/%Y',StartDateTyped),''),
                EndDate=CASE WHEN IsCurrent=1 THEN 'Presente' ELSE COALESCE(strftime('%d/%m/%Y',EndDateTyped),'') END;
            ALTER TABLE WorkExperiences DROP COLUMN StartDateTyped;
            ALTER TABLE WorkExperiences DROP COLUMN EndDateTyped;
            ALTER TABLE WorkExperiences DROP COLUMN IsCurrent;

            ALTER TABLE EducationItems RENAME COLUMN StartDate TO StartDateTyped;
            ALTER TABLE EducationItems RENAME COLUMN EndDate TO EndDateTyped;
            ALTER TABLE EducationItems ADD COLUMN StartDate TEXT NOT NULL DEFAULT '';
            ALTER TABLE EducationItems ADD COLUMN EndDate TEXT NOT NULL DEFAULT '';
            UPDATE EducationItems SET StartDate=COALESCE(strftime('%d/%m/%Y',StartDateTyped),''),
                EndDate=COALESCE(strftime('%d/%m/%Y',EndDateTyped),'');
            ALTER TABLE EducationItems DROP COLUMN StartDateTyped;
            ALTER TABLE EducationItems DROP COLUMN EndDateTyped;

            ALTER TABLE Languages RENAME COLUMN Proficiency TO ProficiencyTyped;
            ALTER TABLE Languages ADD COLUMN Proficiency TEXT NOT NULL DEFAULT '';
            UPDATE Languages SET Proficiency=CAST(ProficiencyTyped AS TEXT)||'%';
            ALTER TABLE Languages DROP COLUMN ProficiencyTyped;

            INSERT INTO ProjectTechnologies(ProjectId,Name)
            SELECT EntityId,OriginalValue FROM ContentMigrationIssues WHERE EntityType='ProjectTechnology' AND FieldName='Technology';
            INSERT INTO WorkExperienceTechnologies(WorkExperienceId,Name)
            SELECT EntityId,OriginalValue FROM ContentMigrationIssues WHERE EntityType='WorkExperienceTechnology' AND FieldName='Technology';
            DROP TABLE ContentMigrationIssues;
            """);
    }

    private static void CreateTechnologyLinks(MigrationBuilder migrationBuilder, string table, string ownerColumn, string entityType)
    {
        var temp = $"{table}_new";
        migrationBuilder.Sql($"""
            CREATE TABLE {temp} (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                {ownerColumn} INTEGER NOT NULL,
                TechnologyId INTEGER NOT NULL,
                CONSTRAINT FK_{table}_Owners_{ownerColumn} FOREIGN KEY ({ownerColumn}) REFERENCES {OwnerTable(ownerColumn)} (Id) ON DELETE CASCADE,
                CONSTRAINT FK_{table}_Technologies_TechnologyId FOREIGN KEY (TechnologyId) REFERENCES Technologies (Id) ON DELETE CASCADE,
                CONSTRAINT UQ_{table}_{ownerColumn}_TechnologyId UNIQUE ({ownerColumn}, TechnologyId)
            );
            INSERT INTO {temp}(Id,{ownerColumn},TechnologyId)
            SELECT MIN(old.Id),old.{ownerColumn},MIN(t.Id)
            FROM {table} old JOIN Technologies t ON lower(trim(t.Name))=lower(trim(old.Name))
            GROUP BY old.{ownerColumn},lower(trim(old.Name));
            DROP TABLE {table};
            ALTER TABLE {temp} RENAME TO {table};
            CREATE INDEX IX_{table}_TechnologyId ON {table}(TechnologyId);
            """);
    }

    private static void RestoreTechnologyLinks(MigrationBuilder migrationBuilder, string table, string ownerColumn, string entityType)
    {
        var temp = $"{table}_legacy";
        migrationBuilder.Sql($"""
            CREATE TABLE {temp} (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                {ownerColumn} INTEGER NOT NULL,
                Name TEXT NOT NULL,
                CONSTRAINT FK_{table}_Owners_{ownerColumn} FOREIGN KEY ({ownerColumn}) REFERENCES {OwnerTable(ownerColumn)} (Id) ON DELETE CASCADE
            );
            INSERT INTO {temp}(Id,{ownerColumn},Name)
            SELECT old.Id,old.{ownerColumn},t.Name FROM {table} old JOIN Technologies t ON t.Id=old.TechnologyId;
            INSERT INTO {temp}({ownerColumn},Name)
            SELECT EntityId,OriginalValue FROM ContentMigrationIssues
            WHERE EntityType='{entityType}' AND FieldName='Technology';
            DROP TABLE {table};
            ALTER TABLE {temp} RENAME TO {table};
            CREATE INDEX IX_{table}_{ownerColumn} ON {table}({ownerColumn});
            """);
    }

    private static string OwnerTable(string ownerColumn) => ownerColumn == "ProjectId" ? "Projects" : "WorkExperiences";

    private static string ValidDate(string column)
    {
        var value = $"trim(\"{column}\")";
        var candidate = $"CASE " +
                        $"WHEN {value} GLOB '[0-9][0-9]/[0-9][0-9]/[0-9][0-9][0-9][0-9]' THEN substr({value},7,4)||'-'||substr({value},4,2)||'-'||substr({value},1,2) " +
                        $"WHEN {value} GLOB '[0-9][0-9]/[0-9][0-9][0-9][0-9]' THEN substr({value},4,4)||'-'||substr({value},1,2)||'-01' " +
                        $"WHEN {value} GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' THEN {value} " +
                        $"WHEN {value} GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]' THEN substr({value},1,7)||'-01' ELSE NULL END";
        return $"CASE WHEN ({candidate}) IS NOT NULL AND date(({candidate}), '+0 days') = ({candidate}) THEN ({candidate}) ELSE NULL END";
    }
}
