using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FonTech.Domain.Entity;
using FonTech.Domain.Result;

namespace FonTech.Domain.Interfaces.Validations
{
    public interface IReportValidator : IBaseValidator<Report>
    {
        /// <summary>
        /// Проверяется наличие отчета, если отчет с переданным названием есть в БД, то создать точно такой же нельзя
        /// Проверяется пользователь, если с UserId пользователь не найден, то такого пользователя нет
        /// </summary>
        /// <param name="report"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        BaseResult CreateValidator(Report report, User user);

    }
}
