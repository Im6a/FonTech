using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.Domain.Enum
{
    public enum ErrorCodes
    {
        //0 - 10 - Report
        
        ReportsNotFound = 0,
        ReportNotFound = 1,
        ReportAlreadyExist = 2,

        //11 - 20 - User
        InternalServerError = 10,
        UserNotFound = 11,
        UserAlreadyExist = 12,
        UserUnauthorizedAccess = 13,
        UserAlreadyHaveRole = 14,

        //21 - 30 - Login
        PasswordNotEqualsPasswordConfirm = 21,
        PasswordIsIncorrect = 22,
        InvalidClientRequest = 23,

        //31 - 40 - Roles
        RoleAlreadyExists = 31,
        RoleNotFound = 32,


    }
}
