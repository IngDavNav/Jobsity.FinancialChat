using Jobsity.FinancialChat.Application.Common.Identity;
using Jobsity.FinancialChat.Domain.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jobsity.FinancialChat.Infrastructure.Persistance;

public class ChatDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
           : base(options)
    {
    }

    public DbSet<ChatUser> ChatUsers => Set<ChatUser>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
    }

}
