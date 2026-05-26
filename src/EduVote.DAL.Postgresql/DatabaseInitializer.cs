using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;
using EduVote.DAL.Postgresql.Models.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVote.DAL.Postgresql;

public class DatabaseInitializer(
    EduVoteDbContext context,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            logger.LogInformation("Initializing database...");
            
            var migrations = await context.Database.GetPendingMigrationsAsync();
            if (migrations.Any())
            {
                logger.LogInformation("Applying pending migrations...");
                await context.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("All migrations are already applied.");
            }
            
            logger.LogInformation("Seeding database with initial data...");
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization.");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        if (await context.Votings.AnyAsync())
        {
            logger.LogInformation("Seed data already exists. Skipping.");
            return;
        }

        logger.LogInformation("Starting seed data initialization...");
        
        await SeedRolesAsync();
        await SeedEducationUnitsAsync();
        await SeedUsersAsync();
        await SeedVotingsWithTargetsAsync();
        
        logger.LogInformation("Seed data initialization completed successfully.");
    }

    #region Roles

    private async Task SeedRolesAsync()
    {
        var roles = new List<Role>
        {
            new() { Id = Guid.NewGuid(), Name = "Student" },
            new() { Id = Guid.NewGuid(), Name = "Teacher" },
            new() { Id = Guid.NewGuid(), Name = "Administrator" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
        logger.LogInformation($"Created {roles.Count} roles.");
    }

    #endregion

    #region Education Units

    private async Task SeedEducationUnitsAsync()
    {
        var units = new List<EducationUnit>();

        // University Root
        var university = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Государственный университет",
            Type = EducationUnitType.University,
            ParentId = null
        };

        // Faculty of Information Technology
        var facultyIT = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Факультет автоматизированных и информационных систем",
            Type = EducationUnitType.Faculty,
            ParentId = university.Id
        };

        // Faculty of Economics
        var facultyEcon = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Факультет экономики и управления",
            Type = EducationUnitType.Faculty,
            ParentId = university.Id
        };

        // Speciality: Software Engineering (under Faculty IT)
        var specialitySoftwareEng = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Разработка программного обеспечения",
            Type = EducationUnitType.Speciality,
            ParentId = facultyIT.Id
        };

        // Speciality: Information Security (under Faculty IT)
        var specialityInfoSec = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Информационная безопасность",
            Type = EducationUnitType.Speciality,
            ParentId = facultyIT.Id
        };

        // Course 1 - Software Engineering
        var courseSE1 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "1 курс",
            Type = EducationUnitType.Course,
            ParentId = specialitySoftwareEng.Id
        };

        // Course 2 - Software Engineering
        var courseSE2 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "2 курс",
            Type = EducationUnitType.Course,
            ParentId = specialitySoftwareEng.Id
        };

        // Course 1 - Information Security
        var courseIS1 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "1 курс",
            Type = EducationUnitType.Course,
            ParentId = specialityInfoSec.Id
        };

        // Groups - Software Engineering 1st year
        var groupSE1_1 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Группа ИП-11",
            Type = EducationUnitType.Group,
            ParentId = courseSE1.Id
        };

        var groupSE1_2 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Группа ИП-12",
            Type = EducationUnitType.Group,
            ParentId = courseSE1.Id
        };

        // Groups - Software Engineering 2nd year
        var groupSE2_1 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Группа ИП-21",
            Type = EducationUnitType.Group,
            ParentId = courseSE2.Id
        };

        // Groups - Information Security 1st year
        var groupIS1_1 = new EducationUnit
        {
            Id = Guid.NewGuid(),
            Name = "Группа ИБ-11",
            Type = EducationUnitType.Group,
            ParentId = courseIS1.Id
        };

        units.AddRange(new[] { university, facultyIT, facultyEcon, specialitySoftwareEng, specialityInfoSec,
                               courseSE1, courseSE2, courseIS1, groupSE1_1, groupSE1_2, groupSE2_1, groupIS1_1 });

        await context.EducationUnits.AddRangeAsync(units);
        await context.SaveChangesAsync();
        logger.LogInformation($"Created {units.Count} education units.");
    }

    #endregion

    #region Users

    private async Task SeedUsersAsync()
    {
        var studentRole = await context.Roles.FirstAsync(r => r.Name == "Student");
        var teacherRole = await context.Roles.FirstAsync(r => r.Name == "Teacher");
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrator");

        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "ivan@edu.com", Name = "Ivan", RoleId = studentRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "maria@edu.com", Name = "Maria", RoleId = studentRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "dmitry@edu.com", Name = "Dima", RoleId = studentRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "anna@edu.com", Name = "Anna", RoleId = studentRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "student@edu.com", Name = "Alex", RoleId = studentRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "elena@edu.com", Name = "Elana", RoleId = studentRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "irina@edu.com", Name = "Irina", RoleId = teacherRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "alex2@edu.com", Name = "Alex2", RoleId = teacherRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.NewGuid(), Email = "teacher@gmail.com", Name = "Nata", RoleId = teacherRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"},
            new() { Id = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11"), Email = "admin@admin", Name = "Pasha", RoleId = adminRole.Id, PasswordHash = "$2a$11$KJ039LcL7.rf3UQXJMisse1JfiaorlbOTLlJZGv4iiDjwbLJIS7Fm"}
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Link users to education units
        var educationUnits = await context.EducationUnits.ToListAsync();
        
        var groupSE1_1 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИП-11")!;
        var groupSE1_2 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИП-12")!;
        var groupSE2_1 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИП-21")!;
        var groupIS1_1 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИБ-11")!;
        var facultyIT = educationUnits.FirstOrDefault(u => u.Name == "Факультет автоматизированных и информационных систем")!;

        var userEducationUnits = new List<UserEducationUnit>
        {
            new() { UserId = users[0].Id, EducationUnitId = groupSE1_1.Id },
            new() { UserId = users[1].Id, EducationUnitId = groupSE1_2.Id },
            new() { UserId = users[2].Id, EducationUnitId = groupSE2_1.Id },
            new() { UserId = users[3].Id, EducationUnitId = groupIS1_1.Id },
            new() { UserId = users[4].Id, EducationUnitId = groupSE1_1.Id },
            new() { UserId = users[5].Id, EducationUnitId = groupSE1_2.Id },
            new() { UserId = users[6].Id, EducationUnitId = facultyIT.Id },
            new() { UserId = users[7].Id, EducationUnitId = facultyIT.Id },
            new() { UserId = users[8].Id, EducationUnitId = facultyIT.Id },
        };

        await context.UserEducationUnits.AddRangeAsync(userEducationUnits);
        await context.SaveChangesAsync();
        logger.LogInformation($"Created {users.Count} users and linked them to education units.");
    }

    #endregion

    #region Votings

    private async Task SeedVotingsWithTargetsAsync()
    {
        var now = DateTime.UtcNow;
        var votings = CreateVotings(now);

        await context.Votings.AddRangeAsync(votings);
        await context.SaveChangesAsync();

        await SeedVotingTargetsAsync(votings);
        
        logger.LogInformation($"Created {votings.Count} votings with targets.");
    }

    private List<Voting> CreateVotings(DateTime now)
    {
        var princessVotingId = Guid.NewGuid();
        var bestTeacherVotingId = Guid.NewGuid();
        var eventsVotingId = Guid.NewGuid();
        var servicesVotingId = Guid.NewGuid();
        var cafeteriaRatingVotingId = Guid.NewGuid();
        var coursesRatingVotingId = Guid.NewGuid();
        var improvementsOpenVotingId = Guid.NewGuid();
        var whyUniversityOpenVotingId = Guid.NewGuid();

        var votings = new List<Voting>
        {
            new()
            {
                Id = princessVotingId,
                Title = "Принцесса университета",
                Description = "Выберите студентку, которая по вашему мнению является победителем Конкурса \"Пренцесса ГГТУ\".",
                Type = VotingType.SingleChoice,
                IsAnonymous = true,
                AllowVoteChange = false,
                StartTime = now.AddDays(-2),
                EndTime = now.AddDays(5),
                Status = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "Алина Ковальчук",
                        Description = "Обаятельная и прекрасная, с тёплой улыбкой и уверенной манерой держаться.",
                        PhotoObjectName = "https://picsum.photos/200?random=101"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "Мария Шевченко",
                        Description = "Изящная и светлая, умеет очаровать вниманием к деталям и доброжелательностью.",
                        PhotoObjectName = "https://picsum.photos/200?random=102"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "Екатерина Левченко",
                        Description = "Грациозная и жизнерадостная, с яркой энергией и искренним обаянием.",
                        PhotoObjectName = "https://picsum.photos/200?random=103"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "София Дорошенко",
                        Description = "Изящная и вдохновляющая, с мягким характером и по-настоящему королевской осанкой.",
                        PhotoObjectName = "https://picsum.photos/200?random=104"
                    }
                ],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = bestTeacherVotingId,
                Title = "Лучший преподаватель года",
                Description = "Оцените преподавателей, которые сделали обучение наиболее полезным и вдохновляющим.",
                Type = VotingType.SingleChoice,
                IsAnonymous = true,
                AllowVoteChange = false,
                StartTime = now.AddDays(-10),
                EndTime = now.AddDays(5),
                Status = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Проф. Ирина Мельник",
                        Description = "Кафедра программной инженерии, известна практико-ориентированными занятиями и менторством.",
                        PhotoObjectName = "https://picsum.photos/200?random=105"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Доц. Алексей Ткаченко",
                        Description = "Кафедра экономики, ведёт лекции и практические занятия.",
                        PhotoObjectName = "https://picsum.photos/200?random=106"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Проф. Наталья Бондарь",
                        Description = "Кафедра биотехнологий, руководитель лабораторных практикумов и студенческих исследований.",
                        PhotoObjectName = "https://picsum.photos/200?random=107"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Ст. преп. Дмитрий Поляков",
                        Description = "Кафедра кибербезопасности, проводит открытые разборы реальных инцидентов.",
                        PhotoObjectName = "https://picsum.photos/200?random=108"
                    }
                ],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = eventsVotingId,
                Title = "Студенческие мероприятия на следующий семестр",
                Description = "Выберите несколько активностей, которые вы хотите видеть в календаре университета.",
                Type = VotingType.MultipleChoice,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(14),
                Status = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Хакатон выходного дня",
                        Description = "48-часовой командный хакатон с треками по AI, web и мобильной разработке.",
                        PhotoObjectName = "https://picsum.photos/200?random=109"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Фестиваль культур",
                        Description = "Неделя стендов, мастер-классов и кулинарных презентаций от международных студентов.",
                        PhotoObjectName = "https://picsum.photos/200?random=110"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Ярмарка стажировок",
                        Description = "Встреча с работодателями и быстрые интервью для студентов 2–4 курсов.",
                        PhotoObjectName = "https://picsum.photos/200?random=111"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Кинопоказ для студентов",
                        Description = "Просмотр фильма в общежитии или актовом зале.",
                        PhotoObjectName = "https://picsum.photos/200?random=112"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Спортивный кубок факультетов",
                        Description = "Серия соревнований между факультетами по футболу, волейболу и настольному теннису.",
                        PhotoObjectName = "https://picsum.photos/200?random=113"
                    }
                ],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = servicesVotingId,
                Title = "Какие сервисы нужны университету",
                Description = "Отметьте сервисы, которые стоит внедрить в первую очередь для студентов и преподавателей.",
                Type = VotingType.MultipleChoice,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now.AddDays(3),
                EndTime = now.AddDays(21),
                Status = VotingStatus.Draft,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "24/7 онлайн-поддержка студентов",
                        Description = "Единый чат для вопросов по расписанию, справкам и административным процедурам.",
                        PhotoObjectName = "https://picsum.photos/200?random=114"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Сервис записи к психологу",
                        Description = "Конфиденциальная платформа для бронирования консультаций с психологической службой.",
                        PhotoObjectName = "https://picsum.photos/200?random=115"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Электронный пропуск",
                        Description = "Вход в здания по телефону вместо студенческого билета.",
                        PhotoObjectName = "https://picsum.photos/200?random=116"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Сервис аренды оборудования",
                        Description = "Онлайн-бронирование ноутбуков, камер и лабораторных наборов для проектов.",
                        PhotoObjectName = "https://picsum.photos/200?random=117"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Платформа поиска соседей по общежитию",
                        Description = "Подбор совместимого соседа по интересам, графику и бытовым привычкам.",
                        PhotoObjectName = "https://picsum.photos/200?random=118"
                    }
                ],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = cafeteriaRatingVotingId,
                Title = "Оценка столовой университета",
                Description = "Поставьте оценку ключевым аспектам работы столовой, чтобы улучшить питание в университете.",
                Type = VotingType.Rating,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now.AddDays(-7),
                EndTime = now.AddDays(7),
                Status = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Качество блюд",
                        Description = "Насколько вкусные и свежие блюда подаются в столовой.",
                        PhotoObjectName = "https://picsum.photos/200?random=119"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Разнообразие меню",
                        Description = "Оцените выбор блюд, включая вегетарианские и диетические опции.",
                        PhotoObjectName = "https://picsum.photos/200?random=120"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Цены",
                        Description = "Соответствие стоимости блюд студенческому бюджету.",
                        PhotoObjectName = "https://picsum.photos/200?random=121"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Чистота и обслуживание",
                        Description = "Состояние зала, скорость обслуживания и вежливость персонала.",
                        PhotoObjectName = "https://picsum.photos/200?random=122"
                    }
                ],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = coursesRatingVotingId,
                Title = "Оценка курсов семестра",
                Description = "Оцените курсы этого семестра по полезности и качеству преподавания.",
                Type = VotingType.Rating,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now.AddDays(-20),
                EndTime = now.AddDays(7),
                Status = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Архитектура программных систем",
                        Description = "Курс о проектировании масштабируемых приложений и паттернах интеграции.",
                        PhotoObjectName = "https://picsum.photos/200?random=123"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Анализ данных и визуализация",
                        Description = "Практический курс по Python, статистике и построению дашбордов.",
                        PhotoObjectName = "https://picsum.photos/200?random=124"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Экономика инноваций",
                        Description = "Изучение бизнес-моделей стартапов и механизмов финансирования проектов.",
                        PhotoObjectName = "https://picsum.photos/200?random=125"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Кибербезопасность веб-приложений",
                        Description = "Основы защиты приложений, secure coding и тестирование на уязвимости.",
                        PhotoObjectName = "https://picsum.photos/200?random=126"
                    }
                ],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = improvementsOpenVotingId,
                Title = "Предложения по улучшению университета",
                Description = "Открытый вопрос: какие изменения помогут сделать обучение и жизнь в университете лучше?",
                Type = VotingType.OpenAnswer,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now,
                EndTime = now.AddDays(30),
                Status = VotingStatus.Active,
                Candidates = [],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            },
            new()
            {
                Id = whyUniversityOpenVotingId,
                Title = "Почему вы выбрали наш университет?",
                Description = "Открытый вопрос для абитуриентов и студентов о главных причинах выбора университета.",
                Type = VotingType.OpenAnswer,
                IsAnonymous = false,
                AllowVoteChange = true,
                StartTime = now.AddDays(1),
                EndTime = now.AddDays(40),
                Status = VotingStatus.Draft,
                Candidates = [],
                CreatedById = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11")
            }
        };

        return votings;
    }

    #endregion

    #region Voting Targets

    private async Task SeedVotingTargetsAsync(List<Voting> votings)
    {
        var educationUnits = await context.EducationUnits.ToListAsync();
        
        var facultyIT = educationUnits.FirstOrDefault(u => u.Name == "Факультет автоматизированных и информационных систем");
        var groupSE1_1 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИП-11");
        var groupSE1_2 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИП-12");
        var groupSE2_1 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИП-21");
        var groupIS1_1 = educationUnits.FirstOrDefault(u => u.Name == "Группа ИБ-11");

        var targets = new List<VotingTarget>();

        // "Princess of the University" - only IT Faculty students
        var princessVoting = votings.FirstOrDefault(v => v.Title == "Принцесса университета");
        if (princessVoting != null && facultyIT != null)
        {
            targets.Add(new VotingTarget { VotingId = princessVoting.Id, EducationUnitId = facultyIT.Id });
        }

        // "Best Teacher" - all IT students
        var bestTeacherVoting = votings.FirstOrDefault(v => v.Title == "Лучший преподаватель года");
        if (bestTeacherVoting != null && facultyIT != null)
        {
            targets.Add(new VotingTarget { VotingId = bestTeacherVoting.Id, EducationUnitId = facultyIT.Id });
        }

        // "Student Events" - all groups
        var eventsVoting = votings.FirstOrDefault(v => v.Title == "Студенческие мероприятия на следующий семестр");
        if (eventsVoting != null)
        {
            if (groupSE1_1 != null)
                targets.Add(new VotingTarget { VotingId = eventsVoting.Id, EducationUnitId = groupSE1_1.Id });
            if (groupSE1_2 != null)
                targets.Add(new VotingTarget { VotingId = eventsVoting.Id, EducationUnitId = groupSE1_2.Id });
            if (groupSE2_1 != null)
                targets.Add(new VotingTarget { VotingId = eventsVoting.Id, EducationUnitId = groupSE2_1.Id });
            if (groupIS1_1 != null)
                targets.Add(new VotingTarget { VotingId = eventsVoting.Id, EducationUnitId = groupIS1_1.Id });
        }

        // "Services Needed" - 1st and 2nd year students
        var servicesVoting = votings.FirstOrDefault(v => v.Title == "Какие сервисы нужны университету");
        if (servicesVoting != null)
        {
            if (groupSE1_1 != null)
                targets.Add(new VotingTarget { VotingId = servicesVoting.Id, EducationUnitId = groupSE1_1.Id });
            if (groupSE1_2 != null)
                targets.Add(new VotingTarget { VotingId = servicesVoting.Id, EducationUnitId = groupSE1_2.Id });
            if (groupSE2_1 != null)
                targets.Add(new VotingTarget { VotingId = servicesVoting.Id, EducationUnitId = groupSE2_1.Id });
        }

        // "Cafeteria Rating" - all IT students
        var cafeteriaVoting = votings.FirstOrDefault(v => v.Title == "Оценка столовой и услуг питания");
        if (cafeteriaVoting != null && facultyIT != null)
        {
            targets.Add(new VotingTarget { VotingId = cafeteriaVoting.Id, EducationUnitId = facultyIT.Id });
        }

        // "Courses Rating" - all IT students
        var coursesVoting = votings.FirstOrDefault(v => v.Title == "Оценка прослушанных курсов");
        if (coursesVoting != null && facultyIT != null)
        {
            targets.Add(new VotingTarget { VotingId = coursesVoting.Id, EducationUnitId = facultyIT.Id });
        }

        // "Improvements" - all students
        var improvementsVoting = votings.FirstOrDefault(v => v.Title == "Предложения по улучшению университета");
        if (improvementsVoting != null)
        {
            if (groupSE1_1 != null)
                targets.Add(new VotingTarget { VotingId = improvementsVoting.Id, EducationUnitId = groupSE1_1.Id });
            if (groupSE1_2 != null)
                targets.Add(new VotingTarget { VotingId = improvementsVoting.Id, EducationUnitId = groupSE1_2.Id });
            if (groupSE2_1 != null)
                targets.Add(new VotingTarget { VotingId = improvementsVoting.Id, EducationUnitId = groupSE2_1.Id });
            if (groupIS1_1 != null)
                targets.Add(new VotingTarget { VotingId = improvementsVoting.Id, EducationUnitId = groupIS1_1.Id });
        }

        // "Why our University" - all students
        var whyUniversityVoting = votings.FirstOrDefault(v => v.Title == "Почему вы выбрали наш университет?");
        if (whyUniversityVoting != null)
        {
            if (groupSE1_1 != null)
                targets.Add(new VotingTarget { VotingId = whyUniversityVoting.Id, EducationUnitId = groupSE1_1.Id });
            if (groupSE1_2 != null)
                targets.Add(new VotingTarget { VotingId = whyUniversityVoting.Id, EducationUnitId = groupSE1_2.Id });
            if (groupSE2_1 != null)
                targets.Add(new VotingTarget { VotingId = whyUniversityVoting.Id, EducationUnitId = groupSE2_1.Id });
            if (groupIS1_1 != null)
                targets.Add(new VotingTarget { VotingId = whyUniversityVoting.Id, EducationUnitId = groupIS1_1.Id });
        }

        await context.VotingTargets.AddRangeAsync(targets);
        await context.SaveChangesAsync();
        logger.LogInformation($"Created {targets.Count} voting targets.");
    }

    #endregion
}