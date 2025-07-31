using AutoMapper;
using FonTech.Domain.Dto.Role;
using FonTech.Domain.Dto.User;
using FonTech.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.Application.Mapping
{
    internal class RoleMapping : Profile
    {
        public RoleMapping()
        {
            CreateMap<Role, RoleDto>().ReverseMap();
        }

    }
}
