using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Seed;

public sealed class EadDemoSeeder
{
    private static readonly IReadOnlyList<DemoCourse> DemoCourses =
    [
        new(
            "Primeiros Passos na Recuperacao",
            "Curso introdutorio para orientar acolhidos nos primeiros dias de rotina, pertencimento e compromisso terapeutico.",
            [
                new("Acolhimento e rotina", "Fundamentos para entrada segura na comunidade.", ["Boas-vindas ao programa", "Rotina diaria e combinados", "Como pedir ajuda"]),
                new("Compromisso pessoal", "Primeiras metas e acordos de cuidado.", ["Meu plano de recuperacao", "Rede de apoio inicial", "Primeira semana com foco"])
            ]),
        new(
            "Prevencao a Recaida: Reconhecer e Agir",
            "Trilha pratica para identificar gatilhos, construir planos de resposta e fortalecer escolhas saudaveis.",
            [
                new("Reconhecendo sinais", "Mapeamento de riscos e sinais de alerta.", ["Gatilhos internos e externos", "Pensamentos de risco", "Sinais corporais"]),
                new("Plano de acao", "Respostas concretas para momentos criticos.", ["Tecnicas de interrupcao", "Acionando a rede de apoio", "Plano de emergencia"])
            ]),
        new(
            "Inteligencia Emocional e Autocuidado",
            "Conteudo para desenvolver regulacao emocional, autoconsciencia e praticas de autocuidado.",
            [
                new("Emocoes e escolhas", "Reconhecer emocoes antes de agir.", ["Nomeando emocoes", "Diario emocional", "Pausa consciente"]),
                new("Autocuidado sustentavel", "Rotinas simples para manter equilibrio.", ["Sono e alimentacao", "Respiracao guiada", "Cuidado com o corpo"])
            ]),
        new(
            "Familia como Rede de Apoio",
            "Curso para preparar o acolhido para dialogos familiares, limites saudaveis e reconstrucao de vinculos.",
            [
                new("Vinculos e limites", "Como lidar com expectativas e limites.", ["Conversas dificeis", "Limites claros", "Reparacao possivel"]),
                new("Plano familiar", "Organizacao da rede de apoio.", ["Mapa de apoio", "Acordos familiares", "Reuniao de acompanhamento"])
            ]),
        new(
            "Projeto de Vida e Reintegracao Social",
            "Trilha para transformar metas terapeuticas em escolhas de vida, estudo, trabalho e convivencia.",
            [
                new("Proposito e direcao", "Clareza sobre valores e futuro.", ["Valores pessoais", "Metas de curto prazo", "Projeto de vida"]),
                new("Reintegracao social", "Preparacao para retorno seguro ao convivio.", ["Ambientes de risco", "Novas referencias", "Plano de continuidade"])
            ]),
        new(
            "Preparacao para o Mercado de Trabalho",
            "Conteudo pratico para empregabilidade, rotina profissional, curriculo e entrevistas.",
            [
                new("Base profissional", "Organizacao para oportunidades reais.", ["Rotina e pontualidade", "Curriculo objetivo", "Postura profissional"]),
                new("Entrevistas e permanencia", "Como conquistar e manter uma oportunidade.", ["Preparacao para entrevista", "Comunicao no trabalho", "Primeiros 30 dias"])
            ])
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var environment = services.GetRequiredService<IHostEnvironment>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<EadDemoSeeder>>();

        if (!environment.IsDevelopment() || !configuration.GetValue<bool>("DemoData:SeedEad"))
        {
            return;
        }

        var db = services.GetRequiredService<AppDbContext>();
        var timestamp = DateTime.UtcNow;

        var tenant = await db.Tenants
            .AsNoTracking()
            .Where(item => item.IsActive && !item.IsDeleted)
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefaultAsync();

        if (tenant is null)
        {
            logger.LogWarning("EAD demo seed skipped because no active tenant exists.");
            return;
        }

        foreach (var demoCourse in DemoCourses)
        {
            if (await db.Courses.AnyAsync(course => course.Title == demoCourse.Title))
            {
                continue;
            }

            var course = new Course
            {
                Title = demoCourse.Title,
                Description = demoCourse.Description,
                IsActive = true,
                CreatedAt = timestamp
            };

            var moduleOrder = 1;
            foreach (var demoModule in demoCourse.Modules)
            {
                var module = new CourseModule
                {
                    Course = course,
                    Title = demoModule.Title,
                    Description = demoModule.Description,
                    Order = moduleOrder++,
                    CreatedAt = timestamp
                };

                var lessonOrder = 1;
                foreach (var lessonTitle in demoModule.Lessons)
                {
                    module.Lessons.Add(new Lesson
                    {
                        Title = lessonTitle,
                        Description = $"Aula demonstrativa sobre {lessonTitle.ToLowerInvariant()} com foco em aplicacao pratica no plano terapeutico.",
                        VideoProvider = "YouTube",
                        VideoExternalId = $"demo-{NormalizeCode(lessonTitle)}",
                        DurationInMinutes = 12 + lessonOrder * 4,
                        Order = lessonOrder++,
                        CreatedAt = timestamp
                    });
                }

                course.Modules.Add(module);
            }

            db.Courses.Add(course);
        }

        await db.SaveChangesAsync();
        await SeedProgressAsync(db, tenant.Id, timestamp);
        logger.LogInformation("EAD demo seed finished.");
    }

    private static async Task SeedProgressAsync(AppDbContext db, Guid tenantId, DateTime timestamp)
    {
        var students = await db.Students
            .Where(student => student.TenantId == tenantId && !student.IsDeleted)
            .OrderBy(student => student.CreatedAt)
            .Take(6)
            .ToListAsync();

        if (students.Count == 0)
        {
            return;
        }

        var courses = await db.Courses
            .Include(course => course.Modules)
                .ThenInclude(module => module.Lessons)
            .Where(course => DemoCourses.Select(item => item.Title).Contains(course.Title))
            .OrderBy(course => course.Title)
            .ToListAsync();

        var progressProfiles = new[] { 100, 86, 64, 42, 28, 12 };

        for (var studentIndex = 0; studentIndex < students.Count; studentIndex++)
        {
            var student = students[studentIndex];
            var targetPercent = progressProfiles[Math.Min(studentIndex, progressProfiles.Length - 1)];

            foreach (var course in courses.Take(Math.Max(2, courses.Count - studentIndex % 3)))
            {
                var lessons = course.Modules.OrderBy(module => module.Order).SelectMany(module => module.Lessons.OrderBy(lesson => lesson.Order)).ToList();
                for (var lessonIndex = 0; lessonIndex < lessons.Count; lessonIndex++)
                {
                    var lesson = lessons[lessonIndex];
                    if (await db.StudentProgress.AnyAsync(item => item.StudentId == student.Id && item.LessonId == lesson.Id))
                    {
                        continue;
                    }

                    var watched = studentIndex == 0
                        ? 100
                        : Math.Clamp(targetPercent - lessonIndex * 7, 0, 100);

                    if (watched == 0)
                    {
                        continue;
                    }

                    db.StudentProgress.Add(new StudentProgress
                    {
                        StudentId = student.Id,
                        LessonId = lesson.Id,
                        WatchedPercentage = watched,
                        CompletedAt = watched >= 100 ? timestamp.AddDays(-lessonIndex) : null,
                        CreatedAt = timestamp.AddDays(-10 + lessonIndex),
                        UpdatedAt = timestamp.AddDays(-studentIndex)
                    });
                }

                if (studentIndex == 0 && !await db.Certificates.AnyAsync(item => item.StudentId == student.Id && item.CourseId == course.Id))
                {
                    db.Certificates.Add(new Certificate
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        VerificationCode = $"RENOVA-{Guid.NewGuid():N}"[..32].ToUpperInvariant(),
                        IssuedAt = timestamp
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static string NormalizeCode(string value)
    {
        return new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).Take(18).ToArray());
    }

    private sealed record DemoCourse(string Title, string Description, IReadOnlyList<DemoModule> Modules);

    private sealed record DemoModule(string Title, string Description, IReadOnlyList<string> Lessons);
}
