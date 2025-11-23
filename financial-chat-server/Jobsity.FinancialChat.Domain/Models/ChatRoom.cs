using System.Collections.Immutable;

namespace Jobsity.FinancialChat.Domain.Models;

public class ChatRoom
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();
}
