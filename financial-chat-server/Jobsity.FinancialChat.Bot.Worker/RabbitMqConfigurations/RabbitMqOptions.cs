namespace Jobsity.FinancialChat.Bot.Worker.RabbitMqConfigurations
{
    public class RabbitMqOptions
    {
        public string HostName { get; init; } = "localhost";
        public string UserName { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public string BotMessagesQueue { get; init; } = "bot-messages";
        public string BotUserName { get; init; } = "ChatBot";
        public string QueueName { get; init; } = "stock-commands";
    }
}
