using Jobsity.FinancialChat.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jobsity.FinancialChat.Infrastructure.Persistance.Configurations;

public sealed class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        Guid GeneralRoomId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
        builder.HasData(new ChatRoom
        {
            Id = GeneralRoomId,
            Name = "General"
        });
    }
}
