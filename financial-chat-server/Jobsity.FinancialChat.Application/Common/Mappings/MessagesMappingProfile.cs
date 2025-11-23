using AutoMapper;

using Jobsity.FinancialChat.Application.Messages.Dtos;
using Jobsity.FinancialChat.Domain.Models;

namespace Jobsity.FinancialChat.Application.Common.Mappings;

public class MessagesMappingProfile : Profile
{
    public MessagesMappingProfile()
    {
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.UserName));
    }
}
