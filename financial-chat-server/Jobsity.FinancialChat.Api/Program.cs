using Jobsity.FinancialChat.Api.RealTime;
using Jobsity.FinancialChat.Application;
using Jobsity.FinancialChat.Application.Abstractions.Messaging;
using Jobsity.FinancialChat.Domain.Models;
using Jobsity.FinancialChat.Infrastructure;
using Jobsity.FinancialChat.Infrastructure.Persistance;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Jobsity Financial Chat API",
        Version = "v1"
    });
});

builder.Services.AddInfrastructureDependencies(builder.Configuration);
builder.Services.AddApplicationDependencies();
builder.Services.AddSignalR();
builder.Services.AddScoped<IChatNotificationService, SignalRChatNotificationService>();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jobsity Financial Chat API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

    var maxRetries = 20;
    var delay = TimeSpan.FromSeconds(5);
    var attempt = 0;

    while (true)
    {
        try
        {
            // Aplica migraciones
            db.Database.Migrate();

            // Seed básico
            if (!db.ChatRooms.Any())
            {
                db.ChatRooms.Add(new ChatRoom
                {
                    Id = Guid.NewGuid(),
                    Name = "General"
                });
            }

            if (!db.ChatUsers.Any(u => u.UserName == "ChatBot"))
            {
                db.ChatUsers.Add(new ChatUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "ChatBot"
                });
            }

            db.SaveChanges();
            break; // éxito, salimos del loop
        }
        catch (MySqlException ex) when (attempt < maxRetries)
        {
            attempt++;
            Console.WriteLine($"[Startup] MySQL no disponible aún. Reintento {attempt}/{maxRetries} en {delay.TotalSeconds} segundos...");
            Thread.Sleep(delay);
        }
    }
}

app.Run();
