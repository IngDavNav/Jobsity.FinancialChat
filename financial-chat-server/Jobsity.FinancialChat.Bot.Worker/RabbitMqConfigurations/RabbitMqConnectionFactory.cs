using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly RabbitMqOptions _options;
        public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password
            };

            return factory.CreateConnection();
        }
    }
}
