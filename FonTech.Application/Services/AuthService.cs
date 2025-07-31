using AutoMapper;
using FonTech.Application.Resources;
using FonTech.Domain.Dto;
using FonTech.Domain.Dto.Report;
using FonTech.Domain.Dto.User;
using FonTech.Domain.Entity;
using FonTech.Domain.Enum;
using FonTech.Domain.Interfaces.Databases;
using FonTech.Domain.Interfaces.Repositories;
using FonTech.Domain.Interfaces.Services;
using FonTech.Domain.Result;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace FonTech.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<UserToken> _userTokenRepository;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IBaseRepository<UserRole> _userRoleRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        
        public AuthService(IBaseRepository<User> userRepository, 
            ILogger logger, IBaseRepository<UserToken> userTokenRepository, 
            ITokenService tokenService, IMapper mapper,
            IBaseRepository<Role> roleRepository,
            IBaseRepository<UserRole> userRoleRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _logger = logger;
            _userTokenRepository = userTokenRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<BaseResult<UserDto>> Register(RegisterUserDto dto)
        {
            if (dto.Password != dto.PasswordConfirm)
            {
                return new BaseResult<UserDto>
                {
                    ErrorMessage = ErrorMessage.PasswordNotEqualsPasswordConfirm,
                    ErrorCode = (int)ErrorCodes.PasswordNotEqualsPasswordConfirm,
                };
            }
            try //УБРАТЬ TRY/CATH ВЕЗДЕ И ПОСМОТРЕЬ КАК БУДЕТ РАБОТАТЬ С MIDDLEWARE (СДЕЛАТЬ ЧТОБЫ ОБРАБОТКА ИСКЛЮЧЕНИЙ РАБОТАЛА ТАК ЖЕ, КАК СЕЙЧАС)
            {
                var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Login == dto.Login);
                if (user != null)
                {
                    return new BaseResult<UserDto>
                    {
                        ErrorMessage = ErrorMessage.UserAlreadyExist,
                        ErrorCode = (int)ErrorCodes.UserAlreadyExist
                    };
                }

                var hashUserPassword = PasswordHasher.HashPassword(dto.Password);

                using (var transaction = await _unitOfWork.BeginTransactionAsync())
                {
                    try
                    {
                        user = new User()
                        {
                            Login = dto.Login,
                            Password = hashUserPassword
                        };
                        await _unitOfWork.Users.CreateAsync(user);

                        var role = await _roleRepository.GetAll().FirstOrDefaultAsync(x => x.Name == nameof(Roles.User));
                        if (role == null)
                        {
                            return new BaseResult<UserDto>
                            {
                                ErrorMessage = ErrorMessage.RoleNotFound,
                                ErrorCode = (int)ErrorCodes.RoleNotFound
                            };
                        }
                        UserRole userRole = new UserRole()
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        };

                        await _unitOfWork.UserRoles.CreateAsync(userRole);
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();//вроде это можно убрать, но это не точно
                    }
                }



                return new BaseResult<UserDto>()
                {
                    Data = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                //запись в файл для разработчика
                _logger.Error(ex, ex.Message);

                //сообщение пользователю
                return new BaseResult<UserDto>()
                {
                    ErrorMessage = ErrorMessage.InternalServerError,
                    ErrorCode = (int)ErrorCodes.InternalServerError
                };
            }
        }
        public async Task<BaseResult<TokenDto>> Login(LoginUserDto dto)
        {
            try
            {
                var user = await _userRepository.GetAll()
                    .Include(x => x.Roles)
                    .FirstOrDefaultAsync(x => x.Login == dto.Login);
                if (user == null)
                {
                    return new BaseResult<TokenDto>
                    {
                        ErrorMessage = ErrorMessage.UserNotFound,
                        ErrorCode = (int)ErrorCodes.UserNotFound
                    };
                }
                var isVerifyPassword = PasswordHasher.IsVerifyPassword(dto.Password, user.Password);
                if (!isVerifyPassword)
                {
                    return new BaseResult<TokenDto>
                    {
                        ErrorMessage = ErrorMessage.PasswordIsIncorrect,
                        ErrorCode = (int)ErrorCodes.PasswordIsIncorrect

                    };
                }

                var userToken = await _userTokenRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == user.Id);
                var refreshToken = _tokenService.GenerateRefrashToken();

                var userRoles = user.Roles;
                var claims = userRoles.Select(x => new Claim(ClaimTypes.Role, x.Name)).ToList();
                claims.Add(new Claim(ClaimTypes.Name, user.Login));

                var accessToken = _tokenService.GenerateAccessToken(claims);

                if (userToken == null)
                {
                    userToken = new UserToken()
                    {
                         UserId = user.Id,
                         RefreshToken = refreshToken,
                         RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
                    };
                    await _userTokenRepository.CreateAsync(userToken);
                }
                else //обновляем refreshtoken при логировании пользователя
                {
                    userToken.RefreshToken = refreshToken;
                    userToken.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                    await _userTokenRepository.UpdateAsync(userToken);

                }
                return new BaseResult<TokenDto>()
                {
                    Data = new TokenDto(
                    
                        AccessToken: accessToken,
                        RefreshToken: refreshToken
                    )
                };

            }
            catch (Exception ex)
            {
                //запись в файл для разработчика
                _logger.Error(ex, ex.Message);

                //сообщение пользователю
                return new BaseResult<TokenDto>()
                {
                    ErrorMessage = ErrorMessage.InternalServerError,
                    ErrorCode = (int)ErrorCodes.InternalServerError
                };
            }
        }

        public class PasswordHasher
        {
            // По умолчанию 16 байт соли и 100_000 итераций – можно настраивать
            private const int SaltSize = 16;
            private const int KeySize = 32; // длина итогового хеша в байтах
            private const int Iterations = 100_000;

            /// <summary>
            /// Хеширует пароль и возвращает строку с форматированием: iterations.salt.hash (base64).
            /// </summary>
            public static string HashPassword(string password)
            {
                // 1) Генерируем соль
                var salt = new byte[SaltSize];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(salt);

                // 2) Вычисляем PBKDF2-хеш
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(KeySize);

                // 3) Склеиваем параметры в одну строку
                var parts = new[]
                {
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash)
        };
                return string.Join('.', parts);
            }

            /// <summary>
            /// Проверяет введённый пароль на соответствие ранее сохранённому хешу.
            /// </summary>
            public static bool IsVerifyPassword(string password, string storedHash)
            {
                var parts = storedHash.Split('.');
                if (parts.Length != 3) return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hash = Convert.FromBase64String(parts[2]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                byte[] computed = pbkdf2.GetBytes(hash.Length);

                // Сравниваем безопасно — чтобы избежать timing-атак
                return CryptographicOperations.FixedTimeEquals(computed, hash);
            }
        }

        //private string HashPassword(string password)
        //{
        //    byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        //    return BitConverter.ToString(bytes).ToLower();
        //}

        //private bool IsVerifyPassword(string userPasswordHash, string userPassword)
        //{
        //    bool result = false;
        //    var hash = HashPassword(userPassword);
        //    if (userPasswordHash == userPassword)
        //    {
        //        result = true;
        //    }
        //    return result;
        //}
        
    }
}
