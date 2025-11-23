using AutoMapper;

using Jobsity.FinancialChat.Application.ChatRooms;
using Jobsity.FinancialChat.Domain.Models;

namespace Jobsity.FinancialChat.Application.Common.Mappings
{
    public class RoomsMappingProfile : Profile
    {
        public RoomsMappingProfile()
        {
            CreateMap<ChatRoom, ChatRoomDto>();
        }
    }
}
