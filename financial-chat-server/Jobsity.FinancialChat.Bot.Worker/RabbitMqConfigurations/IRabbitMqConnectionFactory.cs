using RabbitMQ.Client;

namespace Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations
{
    public interface IRabbitMqConnectionFactory
    {
        IConnection CreateConnection();
    }
}
