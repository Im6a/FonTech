using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.Domain.Settings
{
    /// <summary>
    /// Класс нужен для того чтобы подтягивать настроки Jwt из файла appsettings.json
    /// </summary>
    public class JwtSettings
    {
        public const string DefaultSection = "Jwt";
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Authority { get; set; }
        public string JwtKey { get; set; }
        public int LifeTime { get; set; }

        public int RefreshTokenValidityInDays { get; set; }
    }
}
