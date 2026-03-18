using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Tenants.Commands.CreateTenant;
using MedControl.Domain.Common;
using MedControl.Domain.Tenants;
using NSubstitute;

namespace MedControl.Application.Tests.Tenants;

public sealed class CreateTenantCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly CreateTenantCommandHandler _sut;

    public CreateTenantCommandHandlerTests()
    {
        _sut = new CreateTenantCommandHandler(_currentUser, _tenantRepository, _unitOfWork, _tokenService);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesTenantWithOwnerAndReturnsToken()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var tokenPair = new TokenPair("access", "refresh", DateTimeOffset.UtcNow.AddHours(1));

        _currentUser.UserId.Returns(userId);
        _currentUser.Email.Returns(email);
        _currentUser.GlobalRoles.Returns(new List<string>());
        _tokenService.GenerateTokenPair(
                userId, email, Arg.Any<Guid>(),
                Arg.Any<IReadOnlyList<string>>(),
                Arg.Any<IReadOnlyList<string>>())
            .Returns(tokenPair);

        var result = await _sut.Handle(new CreateTenantCommand("Clínica Saúde"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access");
        await _tenantRepository.Received(1).AddAsync(
            Arg.Is<Tenant>(t =>
                t.Name == "Clínica Saúde" &&
                t.Members.Any(m => m.UserId == userId && m.Role == TenantRole.Owner)),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullUserId_ReturnsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new CreateTenantCommand("Clinic"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Validator_WithEmptyName_ReturnsValidationError()
    {
        var validator = new CreateTenantCommandValidator();
        var validation = await validator.ValidateAsync(new CreateTenantCommand(string.Empty));
        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_WithNameOver200Chars_ReturnsValidationError()
    {
        var validator = new CreateTenantCommandValidator();
        var longName = new string('x', 201);
        var validation = await validator.ValidateAsync(new CreateTenantCommand(longName));
        validation.IsValid.Should().BeFalse();
    }
}
