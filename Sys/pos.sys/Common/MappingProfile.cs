using pos.sys.Entities;
using pos.sys.Models;
using AutoMapper;

namespace pos.sys.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<user, UserModel>().ReverseMap();  
        }
    }
}
