using AutoMapper;

using Jobsity.FinancialChat.Application.Common.Mappings;
using Jobsity.FinancialChat.Application.Messages.Commands.SendMessage;

using Microsoft.Extensions.DependencyInjection;

namespace Jobsity.FinancialChat.Application
{
    public static class ApplicationDependencyInjector
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {

            services.AddAutoMapper(typeof(MessagesMappingProfile).Assembly);
            
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(SendMessageCommand).Assembly));

            return services;
        }
    }
}
