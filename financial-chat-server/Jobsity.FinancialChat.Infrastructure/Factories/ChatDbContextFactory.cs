using Jobsity.FinancialChat.Infrastructure.Persistance;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jobsity.FinancialChat.Infrastructure.Factories
{
    public sealed class ChatDbContextFactory
        : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

            // SOLO PARA MIGRACIONES
            var connectionString = "Server=localhost;Port=3306;Database=financialchat;User=user123;Password=PassWord;";


            optionsBuilder.UseMySQL(connectionString);

            return new ChatDbContext(optionsBuilder.Options);
        }

    }
}
