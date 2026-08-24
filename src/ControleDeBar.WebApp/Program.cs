using System.Globalization;
using ControleDeBar.Aplicacao;
using ControleDeBar.Infra;
using ControleDeBar.Infra.Compartilhado.Orm;
using ControleDeBar.WebApp.Compartilhado;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configuração do container de injeção de dependência
builder.Services.AddInfraRepositories(
    builder.Configuration,
    builder.Logging,
    builder.Environment
);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddPresentationConfig(builder.Configuration);

// Configura health checks do banco de dados
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ControleDeBarDbContext>(
        name: "database_check",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]
    );

var app = builder.Build();

// Aplica migrações automaticamente em Desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<ControleDeBarDbContext>();

    dbContext.Database.Migrate();
}

// Middlewares de roteamento
RequestLocalizationOptions localizacaoPortuguesBrasil = new()
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = [new CultureInfo("pt-BR")],
    SupportedUICultures = [new CultureInfo("pt-BR")]
};

app.UseRequestLocalization(localizacaoPortuguesBrasil);
app.UseStaticFiles();
app.UseRouting();

// Middlewares de Auth
app.UseAuthentication();
app.UseAuthorization();

// Middleware de reconhecimento de rotas de controllers
app.MapHealthChecks("/health").AllowAnonymous();
app.MapDefaultControllerRoute();

// Execução do Servidor
app.Run();
