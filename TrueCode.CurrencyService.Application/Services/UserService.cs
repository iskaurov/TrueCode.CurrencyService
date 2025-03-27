using Microsoft.AspNetCore.Identity;
using TrueCode.CurrencyService.Domain.Entities;
using TrueCode.CurrencyService.Domain.Repositories;

namespace TrueCode.CurrencyService.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    ITokenGenerator tokenGenerator)
    : IUserService
{
    public async Task RegisterAsync(string name, string password)
    {
        var exists = await userRepository.ExistsByNameAsync(name);
        if (exists)
            throw new Exception("Пользователь с таким именем уже существует");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Password = passwordHasher.HashPassword(null!, password)
        };

        await userRepository.AddAsync(user);
    }

    public async Task<string> LoginAsync(string name, string password)
    {
        var user = await userRepository.GetByNameAsync(name);
        if (user == null)
            throw new UnauthorizedAccessException("Пользователь не найден");

        var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Неверный пароль");

        return tokenGenerator.GenerateToken(user);
    }
}