using Jobsity.FinancialChat.Application.Abstractions;
using Jobsity.FinancialChat.Application.Abstractions.Auth;
using Jobsity.FinancialChat.Application.Abstractions.Persistence;
using Jobsity.FinancialChat.Application.Abstractions.Services;
using Jobsity.FinancialChat.Application.Common.Identity;
using Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations;
using Jobsity.FinancialChat.Infrastructure.Auth;
using Jobsity.FinancialChat.Infrastructure.Messaging;
using Jobsity.FinancialChat.Infrastructure.Persistance;
using Jobsity.FinancialChat.Infrastructure.Persistance.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace Jobsity.FinancialChat.Infrastructure
{
    public static class InfrastructureDependencyInjector
    {
        public static IServiceCollection AddInfrastructureDependencies(
             this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
            services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

            var connectionString = configuration.GetConnectionString("Default") ?? throw new ArgumentNullException();

            services.AddDbContext<ChatDbContext>(options =>
                options.UseMySQL(connectionString,
                    b => b.MigrationsAssembly(typeof(ChatDbContext).Assembly.FullName)));

            services.AddSingleton<IStockCommandBus, RabbitMqStockCommandBus>();
            services.AddHostedService<BotMessagesConsumer>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IChatUserRepository, ChatUserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.ConfigureIdentity(configuration);

            return services;
        }

        private static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<ChatDbContext>()
            .AddSignInManager();

            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // SOLO dev
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.FromHours(2)
                };
            });

            return services;
        }
    }
}
