using AutoMapper;
using outofoffice.Dto;
using outofoffice.Models;

internal class MappingProfile : Profile
{
        public MappingProfile()
        {
            // Configure AutoMapper to map UserAppMessage to UserAppMessageDTO
            CreateMap<UserAppMessage, UserAppMessageDTO>();

            CreateMap<MessageAppList, MessageAppListDTO>().ReverseMap();
        }
}