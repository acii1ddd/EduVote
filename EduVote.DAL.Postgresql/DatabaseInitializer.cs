using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVote.DAL.Postgresql;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly EduVoteDbContext _context;

    //private static readonly string _passwordHash = "$2a$11$0p4EJ6BqWtZUkaZCBr.f8eyKFMGmfw/GeaI7h5uW3TOUyQoQBVR6y";
    
    public DatabaseInitializer(
        EduVoteDbContext context, 
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");
            
            var migrations = await _context.Database.GetPendingMigrationsAsync();
            if (migrations.Any())
            {
                _logger.LogInformation("Migration...");
                await _context.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("All migrations are applied.");
            }
            
            _logger.LogInformation("Seeding voting data...");
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with applying migrations.");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        if (await _context.Votings.AnyAsync())
        {
            _logger.LogInformation("Voting seed data already exists. Skipping.");
            return;
        }

        var now = DateTime.UtcNow;

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
                VotingStatus = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "Алина Ковальчук",
                        Description = "Организатор научного клуба, победительница университетской олимпиады по математике.",
                        PhotoUrl = "https://picsum.photos/200?random=101"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "Мария Шевченко",
                        Description = "Староста потока, координировала волонтерские проекты факультета в этом году.",
                        PhotoUrl = "https://picsum.photos/200?random=102"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "Екатерина Левченко",
                        Description = "Капитан команды дебатов, автор серии образовательных подкастов для первокурсников.",
                        PhotoUrl = "https://picsum.photos/200?random=103"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = princessVotingId,
                        Name = "София Дорошенко",
                        Description = "Лидер студенческого театра, инициировала благотворительный фестиваль в кампусе.",
                        PhotoUrl = "https://picsum.photos/200?random=104"
                    }
                ]
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
                EndTime = now.AddDays(-1),
                VotingStatus = VotingStatus.Finished,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Проф. Ирина Мельник",
                        Description = "Кафедра программной инженерии, известна практико-ориентированными занятиями и менторством.",
                        PhotoUrl = "https://picsum.photos/200?random=105"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Доц. Алексей Ткаченко",
                        Description = "Кафедра экономики, внедрил кейс-метод и еженедельные карьерные воркшопы.",
                        PhotoUrl = "https://picsum.photos/200?random=106"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Проф. Наталья Бондарь",
                        Description = "Кафедра биотехнологий, руководитель лабораторных практикумов и студенческих исследований.",
                        PhotoUrl = "https://picsum.photos/200?random=107"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = bestTeacherVotingId,
                        Name = "Ст. преп. Дмитрий Поляков",
                        Description = "Кафедра кибербезопасности, проводит открытые разборы реальных инцидентов.",
                        PhotoUrl = "https://picsum.photos/200?random=108"
                    }
                ]
            },
            new()
            {
                Id = eventsVotingId,
                Title = "Студенческие мероприятия на следующий семестр",
                Description = "Выберите несколько активностей, которые вы хотите видеть в календаре кампуса.",
                Type = VotingType.MultipleChoice,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(14),
                VotingStatus = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Хакатон выходного дня",
                        Description = "48-часовой командный хакатон с треками по AI, web и мобильной разработке.",
                        PhotoUrl = "https://picsum.photos/200?random=109"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Фестиваль культур",
                        Description = "Неделя стендов, мастер-классов и кулинарных презентаций от международных студентов.",
                        PhotoUrl = "https://picsum.photos/200?random=110"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Ярмарка стажировок",
                        Description = "Встреча с работодателями и быстрые интервью для студентов 2–4 курсов.",
                        PhotoUrl = "https://picsum.photos/200?random=111"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Ночь кино в кампусе",
                        Description = "Открытый кинопоказ и дискуссия о фильмах с приглашенными спикерами.",
                        PhotoUrl = "https://picsum.photos/200?random=112"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = eventsVotingId,
                        Name = "Спортивный кубок факультетов",
                        Description = "Серия соревнований между факультетами по футболу, волейболу и настольному теннису.",
                        PhotoUrl = "https://picsum.photos/200?random=113"
                    }
                ]
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
                VotingStatus = VotingStatus.Draft,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "24/7 онлайн-поддержка студентов",
                        Description = "Единый чат для вопросов по расписанию, справкам и административным процедурам.",
                        PhotoUrl = "https://picsum.photos/200?random=114"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Сервис записи к психологу",
                        Description = "Конфиденциальная платформа для бронирования консультаций с психологической службой.",
                        PhotoUrl = "https://picsum.photos/200?random=115"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Мобильный пропуск в кампус",
                        Description = "Доступ в корпуса и общежития через приложение вместо пластиковых карт.",
                        PhotoUrl = "https://picsum.photos/200?random=116"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Сервис аренды оборудования",
                        Description = "Онлайн-бронирование ноутбуков, камер и лабораторных наборов для проектов.",
                        PhotoUrl = "https://picsum.photos/200?random=117"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = servicesVotingId,
                        Name = "Платформа поиска соседей по общежитию",
                        Description = "Подбор совместимого соседа по интересам, графику и бытовым привычкам.",
                        PhotoUrl = "https://picsum.photos/200?random=118"
                    }
                ]
            },
            new()
            {
                Id = cafeteriaRatingVotingId,
                Title = "Оценка столовой университета",
                Description = "Поставьте оценку ключевым аспектам работы столовой, чтобы улучшить питание в кампусе.",
                Type = VotingType.Rating,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now.AddDays(-7),
                EndTime = now.AddDays(7),
                VotingStatus = VotingStatus.Active,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Качество блюд",
                        Description = "Насколько вкусные и свежие блюда подаются в столовой.",
                        PhotoUrl = "https://picsum.photos/200?random=119"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Разнообразие меню",
                        Description = "Оцените выбор блюд, включая вегетарианские и диетические опции.",
                        PhotoUrl = "https://picsum.photos/200?random=120"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Цены",
                        Description = "Соответствие стоимости блюд студенческому бюджету.",
                        PhotoUrl = "https://picsum.photos/200?random=121"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = cafeteriaRatingVotingId,
                        Name = "Чистота и обслуживание",
                        Description = "Состояние зала, скорость обслуживания и вежливость персонала.",
                        PhotoUrl = "https://picsum.photos/200?random=122"
                    }
                ]
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
                EndTime = now.AddDays(-2),
                VotingStatus = VotingStatus.Finished,
                Candidates =
                [
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Архитектура программных систем",
                        Description = "Курс о проектировании масштабируемых приложений и паттернах интеграции.",
                        PhotoUrl = "https://picsum.photos/200?random=123"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Анализ данных и визуализация",
                        Description = "Практический курс по Python, статистике и построению дашбордов.",
                        PhotoUrl = "https://picsum.photos/200?random=124"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Экономика инноваций",
                        Description = "Изучение бизнес-моделей стартапов и механизмов финансирования проектов.",
                        PhotoUrl = "https://picsum.photos/200?random=125"
                    },
                    new Candidate
                    {
                        Id = Guid.NewGuid(),
                        VotingId = coursesRatingVotingId,
                        Name = "Кибербезопасность веб-приложений",
                        Description = "Основы защиты приложений, secure coding и тестирование на уязвимости.",
                        PhotoUrl = "https://picsum.photos/200?random=126"
                    }
                ]
            },
            new()
            {
                Id = improvementsOpenVotingId,
                Title = "Предложения по улучшению университета",
                Description = "Открытый вопрос: какие изменения помогут сделать обучение и жизнь в кампусе лучше?",
                Type = VotingType.OpenAnswer,
                IsAnonymous = true,
                AllowVoteChange = true,
                StartTime = now,
                EndTime = now.AddDays(30),
                VotingStatus = VotingStatus.Active,
                Candidates = []
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
                VotingStatus = VotingStatus.Draft,
                Candidates = []
            }
        };

        await _context.Votings.AddRangeAsync(votings);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Voting seed data created successfully.");
    }
}