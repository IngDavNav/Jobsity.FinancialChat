using Jobsity.FinancialChat.Application.Bot.Contracts;
using Jobsity.FinancialChat.Domain.Models;

namespace Jobsity.FinancialChat.Application.Abstractions.Services;

public interface IStockCommandBus
{
    Task EnqueueAsync(EnqueueBotStockCommand command, CancellationToken ct = default);
}
