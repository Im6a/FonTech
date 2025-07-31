using AutoMapper;
using FonTech.Application.Resources;
using FonTech.Domain.Dto.Role;
using FonTech.Domain.Dto.UserRole;
using FonTech.Domain.Entity;
using FonTech.Domain.Enum;
using FonTech.Domain.Interfaces.Databases;
using FonTech.Domain.Interfaces.Repositories;
using FonTech.Domain.Interfaces.Services;
using FonTech.Domain.Result;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IBaseRepository<UserRole> _userRoleRepository;
        private readonly IMapper _mapper;
        public RoleService(IBaseRepository<User> userRepository, IBaseRepository<Role> roleRepository, 
            IBaseRepository<UserRole> userRoleRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<UserRoleDto>> AddRoleForUserAsync(UserRoleDto dto)
        {
            var user = await _userRepository.GetAll()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Login == dto.Login);
            if (user == null)
            {
                return new BaseResult<UserRoleDto>
                {
                    ErrorMessage = ErrorMessage.UserNotFound,
                    ErrorCode = (int)ErrorCodes.UserNotFound
                };
            }
            var role = await _roleRepository.GetAll().FirstOrDefaultAsync(r => r.Name == dto.RoleName);
            if (role == null)
            {
                return new BaseResult<UserRoleDto>
                {
                    ErrorMessage = ErrorMessage.RoleNotFound,
                    ErrorCode = (int)ErrorCodes.RoleNotFound
                };
            }
            var userRoles = user.Roles.Select(r => r.Name);
            if (userRoles.Any(r => r == dto.RoleName))
            {
                return new BaseResult<UserRoleDto>
                {
                    ErrorMessage = ErrorMessage.UserAlreadyHaveRole,
                    ErrorCode = (int)ErrorCodes.UserAlreadyHaveRole
                };
            }

            var userRole = new UserRole()
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            await _userRoleRepository.UpdateAsync(userRole);

            return new BaseResult<UserRoleDto>()
            {
                Data = new UserRoleDto
                (
                    Login : user.Login,
                    RoleName : role.Name
                )
            };
        }

        public async Task<BaseResult<RoleDto>> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = await _roleRepository.GetAll().FirstOrDefaultAsync(x => x.Name == dto.Name);
            if (role != null)
            {
                return new BaseResult<RoleDto>()
                {
                    ErrorMessage = ErrorMessage.RoleAlreadyExists,
                    ErrorCode = (int)ErrorCodes.RoleAlreadyExists
                };
            }
            role = new Role()
            {
                Name = dto.Name
            };
            
            await _roleRepository.CreateAsync(role);
            return new BaseResult<RoleDto>()
            {
                Data = _mapper.Map<RoleDto>(role)
            };
        }

        public async Task<BaseResult<RoleDto>> DeleteRoleAsync(long id)
        {
            var role = await _roleRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
            if (role == null)
            {
                return new BaseResult<RoleDto>()
                {
                    ErrorMessage = ErrorMessage.RoleNotFound,
                    ErrorCode = (int)ErrorCodes.RoleNotFound
                };
            }
            await _roleRepository.RemoveAsync(role);
            return new BaseResult<RoleDto>()
            {
                Data = _mapper.Map<RoleDto>(role)
            };


        }

        public async Task<BaseResult<RoleDto>> UpdateRoleAsync(RoleDto dto)
        {
            var role = await _roleRepository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (role == null)
            {
                return new BaseResult<RoleDto>()
                {
                    ErrorMessage = ErrorMessage.RoleNotFound,
                    ErrorCode = (int)ErrorCodes.RoleNotFound
                };
            }

            role.Name = dto.Name;
            await _roleRepository.UpdateAsync(role);
            return new BaseResult<RoleDto>()
            {
                Data = _mapper.Map<RoleDto>(role)
            };
        }
        public async Task<BaseResult<UserRoleDto>> DeleteRoleForUserAsync(DeleteUserRoleDto dto)
        {
            var user = await _userRepository.GetAll()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Login == dto.Login);
            if (user == null)
            {
                return new BaseResult<UserRoleDto>
                {
                    ErrorMessage = ErrorMessage.UserNotFound,
                    ErrorCode = (int)ErrorCodes.UserNotFound
                };
            }
            var role = user.Roles.FirstOrDefault(x => x.Id == dto.RoleId);
            if (role == null)
            {
                return new BaseResult<UserRoleDto>()
                {
                    ErrorMessage = ErrorMessage.RoleNotFound,
                    ErrorCode = (int)ErrorCodes.RoleNotFound
                };
            }
            var userRole = await _userRoleRepository.GetAll()
                .Where(x => x.RoleId == role.Id)
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            await _userRoleRepository.RemoveAsync(userRole);
            return new BaseResult<UserRoleDto>()
            {
                Data = new UserRoleDto
                (
                    Login: user.Login,
                    RoleName: role.Name
                )
            };
        }

        public async Task<BaseResult<UserRoleDto>> UpdateRoleForUserAsync(UpdateUserRoleDto dto) //т.е. для обновления именно сначала удалить, а потом добавить (т.к. при просто update ошибка, т.к. many-to-many)
        {
            var user = await _userRepository.GetAll()
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.Login == dto.Login);

            if (user == null) //вынести все такие проверки в отдельные валидаторы как ReportValidator
            {
                return new BaseResult<UserRoleDto>
                {
                    ErrorMessage = ErrorMessage.UserNotFound,
                    ErrorCode = (int)ErrorCodes.UserNotFound
                };
            }

            var role = user.Roles.FirstOrDefault(x => x.Id == dto.FromRoleId);
            if (role == null)
            {
                return new BaseResult<UserRoleDto>()
                {
                    ErrorMessage = ErrorMessage.RoleNotFound,
                    ErrorCode = (int)ErrorCodes.RoleNotFound
                };
            }
            
            var newRoleForUser = await _roleRepository.GetAll().FirstOrDefaultAsync(x => x.Id == dto.ToRoleId);
            if (newRoleForUser == null)
            {
                return new BaseResult<UserRoleDto>()
                {
                    ErrorMessage = ErrorMessage.RoleNotFound,
                    ErrorCode = (int)ErrorCodes.RoleNotFound
                };
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var userRole = await _userRoleRepository.GetAll()
                        .Where(x => x.RoleId == role.Id)
                        .FirstOrDefaultAsync(x => x.UserId == user.Id);
                    await _unitOfWork.UserRoles.RemoveAsync(userRole);

                    var newUserRole = new UserRole()
                    {
                        UserId = user.Id,
                        RoleId = newRoleForUser.Id
                    };
                    await _unitOfWork.UserRoles.CreateAsync(newUserRole);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();//вроде это не нужно
                }

            }

            return new BaseResult<UserRoleDto>()
            {
                Data = new UserRoleDto
                (
                    Login: user.Login,
                    RoleName: newRoleForUser.Name
                )
            };


        }
    }
}
