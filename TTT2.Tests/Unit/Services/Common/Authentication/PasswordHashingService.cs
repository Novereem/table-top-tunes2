using Shared.Interfaces.Services.Common.Authentication;
using TTT2.Services.Common.Authentication;

namespace TTT2.Tests.Unit.Services.Common.Authentication;

[Trait("Category", "Unit")]
public class PasswordHashingServiceTests
{
    private readonly IPasswordHashingService _passwordHashingService = new PasswordHashingService();

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var plainPassword = "mySecret123";

        // Act
        var hashedPassword = _passwordHashingService.HashPassword(plainPassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEqual(plainPassword, hashedPassword);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var plainPassword = "mySecret123";
        var hashedPassword = _passwordHashingService.HashPassword(plainPassword);

        // Act
        var result = _passwordHashingService.VerifyPassword(plainPassword, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var plainPassword = "mySecret123";
        var wrongPassword = "wrongPassword";
        var hashedPassword = _passwordHashingService.HashPassword(plainPassword);

        // Act
        var result = _passwordHashingService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }
}