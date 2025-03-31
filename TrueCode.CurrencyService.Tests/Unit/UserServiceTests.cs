using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using TrueCode.CurrencyService.Core;
using TrueCode.CurrencyService.Domain.Entities;
using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.Core.Services;

namespace TrueCode.CurrencyService.Tests.Unit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher<User>> _hasherMock = new();
    private readonly Mock<ITokenGenerator> _tokenGenMock = new();

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(
            _userRepoMock.Object,
            _hasherMock.Object,
            _tokenGenMock.Object);
    }
    
    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var username = "user";
        var providedPassword = "1234";
        var hashedPassword = "hashed";
        var expectedToken = "token123";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = username,
            Password = hashedPassword
        };

        // Мок: пользователь найден по имени
        _userRepoMock.Setup(r => r.GetByNameAsync(username)).ReturnsAsync(user);

        // Мок: введённый пароль (rawPassword) проходит проверку с хэшем
        _hasherMock
            .Setup(h => h.VerifyHashedPassword(user, hashedPassword, providedPassword))
            .Returns(PasswordVerificationResult.Success);

        // Мок: генерация токена для пользователя
        _tokenGenMock.Setup(g => g.GenerateToken(user)).Returns(expectedToken);

        // Act
        var token = await _userService.LoginAsync(username, providedPassword);

        // Assert
        Assert.Equal(expectedToken, token); // Ожидаем, что вернётся именно наш токен
    }
    
    [Fact]
    public async Task RegisterAsync_ShouldCallAddAsync_WhenUserIsNew()
    {
        // Arrange
        var username = "new_user";
        var password = "password";

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.ExistsByNameAsync(username))
            .ReturnsAsync(false);

        var passwordHasherMock = new Mock<IPasswordHasher<User>>();
        passwordHasherMock
            .Setup(p => p.HashPassword(It.IsAny<User>(), password))
            .Returns("hashed_password");

        var tokenGeneratorMock = new Mock<ITokenGenerator>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenGeneratorMock.Object);

        // Act
        await userService.RegisterAsync(username, password);

        // Assert
        userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var username = "ghost";
        var password = "irrelevant";

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByNameAsync(username))
            .ReturnsAsync((User?)null); // Пользователь не найден

        var passwordHasherMock = new Mock<IPasswordHasher<User>>();
        var tokenGeneratorMock = new Mock<ITokenGenerator>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenGeneratorMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            userService.LoginAsync(username, password));
    }
    
    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordInvalid()
    {
        // Arrange
        var username = "testuser";
        var enteredPassword = "wrong_password";
        var storedHashedPassword = "hashed_password";

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Name = username,
            Password = storedHashedPassword
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByNameAsync(username))
            .ReturnsAsync(existingUser);

        var passwordHasherMock = new Mock<IPasswordHasher<User>>();
        passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(existingUser, storedHashedPassword, enteredPassword))
            .Returns(PasswordVerificationResult.Failed);

        var tokenGeneratorMock = new Mock<ITokenGenerator>();

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenGeneratorMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            userService.LoginAsync(username, enteredPassword));
    }
    
    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var username = "valid_user";
        var inputPassword = "correct_password";
        var hashedPassword = "hashed_password";
        var expectedToken = "jwt_token";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = username,
            Password = hashedPassword
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByNameAsync(username))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher<User>>();
        passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(user, hashedPassword, inputPassword))
            .Returns(PasswordVerificationResult.Success);

        var tokenGeneratorMock = new Mock<ITokenGenerator>();
        tokenGeneratorMock
            .Setup(t => t.GenerateToken(user))
            .Returns(expectedToken);

        var userService = new UserService(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenGeneratorMock.Object);

        // Act
        var actualToken = await userService.LoginAsync(username, inputPassword);

        // Assert
        actualToken.Should().Be(expectedToken);
    }
}