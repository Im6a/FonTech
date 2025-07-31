using FonTech.Domain.Entity;
using FonTech.Domain.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FonTech.Domain.Dto.Role;
using FonTech.Domain.Dto.UserRole;
namespace FonTech.Domain.Interfaces.Services
{
    /// <summary>
    /// Сервис предназначен для управления ролями
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Создание роли
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<BaseResult<RoleDto>> CreateRoleAsync(CreateRoleDto dto);

        /// <summary>
        /// Удаление роли
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BaseResult<RoleDto>> DeleteRoleAsync(long id);

        /// <summary>
        /// Обновление роли
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<BaseResult<RoleDto>> UpdateRoleAsync(RoleDto dto);
        /// <summary>
        /// Добавление роли для пользователя
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        Task<BaseResult<UserRoleDto>> AddRoleForUserAsync(UserRoleDto dto);
        /// <summary>
        /// Удаление роли у пользователя
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        Task<BaseResult<UserRoleDto>> DeleteRoleForUserAsync(DeleteUserRoleDto dto);
        /// <summary>
        /// Обновление роли у пользователя
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<BaseResult<UserRoleDto>> UpdateRoleForUserAsync(UpdateUserRoleDto dto);


    }
}
