using BatteryDashboard.Server.Interfaces;
using BatteryDashboard.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace BatteryDashboard.Server.Services
{
    public class AccountService(IUserRepository userRepository, IJwtTokenService jwtService) : IAccountService
    {
        private readonly IUserRepository userRepository = userRepository;
        private readonly IJwtTokenService jwtService = jwtService;
        private readonly PasswordHasher<User> passwordHasher = new();

        public async Task<string?> Login(Login login)
        {
            try
            {
                var user = await userRepository.GetUserByEmail(login.UserEmail);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);

                if (verificationResult != PasswordVerificationResult.Success)
                {
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                return jwtService.GenerateToken(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> Register(Register register)
        {
            if (await userRepository.UserExists(register.UserEmail))
                return null;

            User newUser = new()
            {
                UserId = register.Id,
                UserEmail = register.UserEmail,
                UserName = register.UserName,
                Role = register.Role
            };

            newUser.PasswordHash = passwordHasher.HashPassword(newUser, register.Password);

            await userRepository.AddUser(newUser);

            return jwtService.GenerateToken(newUser);
        }
    }
}
