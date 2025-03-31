using Moq;
using TrueCode.CurrencyService.Domain.Entities;
using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.Core.Services;

namespace TrueCode.CurrencyService.Tests.Unit;

public class CurrencyServiceTests
{
    [Fact]
    public async Task GetFavoritesByUserIdAsync_ReturnsCurrencyDtos_WhenUserHasFavorites()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FavoriteCurrencies = new List<Currency>
            {
                new() { Name = "USD", Rate = 90.5m },
                new() { Name = "EUR", Rate = 99.1m }
            }
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.GetUserWithFavoritesAsync(userId))
                          .ReturnsAsync(user);

        var service = new Core.Services.CurrencyService(userRepositoryMock.Object);

        // Act
        var result = await service.GetFavoritesByUserIdAsync(userId);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("USD", resultList[0].Name);
        Assert.Equal(90.5m, (decimal)resultList[0].Rate);
        Assert.Equal("EUR", resultList[1].Name);
        Assert.Equal(99.1m, (decimal)resultList[1].Rate);
    }

    [Fact]
    public async Task GetFavoritesByUserIdAsync_ReturnsEmptyList_WhenUserOrFavoritesIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock.Setup(r => r.GetUserWithFavoritesAsync(userId))
                          .ReturnsAsync((User?)null);

        var service = new Core.Services.CurrencyService(userRepositoryMock.Object);

        // Act
        var result = await service.GetFavoritesByUserIdAsync(userId);

        // Assert
        Assert.Empty(result);
    }
}