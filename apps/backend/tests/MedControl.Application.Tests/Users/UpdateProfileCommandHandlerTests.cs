using FluentAssertions;
using MedControl.Application.Common.Interfaces;
using MedControl.Application.Users.Commands.UpdateProfile;
using MedControl.Domain.Common;
using MedControl.Domain.Users;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace MedControl.Application.Tests.Users;

public sealed class UpdateProfileCommandHandlerTests
{
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateProfileCommandHandler _sut;

    public UpdateProfileCommandHandlerTests()
    {
        _sut = new UpdateProfileCommandHandler(_currentUser, _userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(new UpdateProfileCommand("Novo Nome"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_UsuarioNaoEncontrado_DeveRetornarNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).ReturnsNull();

        var result = await _sut.Handle(new UpdateProfileCommand("Novo Nome"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ComDisplayNameValido_DeveAtualizarERetornarDto()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("user@example.com", "Nome Antigo").Value;
        _currentUser.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.Handle(new UpdateProfileCommand("Nome Novo"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayName.Should().Be("Nome Novo");
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComDisplayNameNull_DeveAtualizarParaNull()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("user@example.com", "Nome Antigo").Value;
        _currentUser.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _sut.Handle(new UpdateProfileCommand(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayName.Should().BeNull();
    }

    [Fact]
    public async Task Validator_ComDisplayNameMuitoLongo_DeveRetornarErro()
    {
        var validator = new UpdateProfileCommandValidator();
        var command = new UpdateProfileCommand(new string('A', 101));

        var validation = await validator.ValidateAsync(command);

        validation.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_ComDisplayNameNull_DevePassar()
    {
        var validator = new UpdateProfileCommandValidator();
        var command = new UpdateProfileCommand(null);

        var validation = await validator.ValidateAsync(command);

        validation.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_ComDisplayNameValido_DevePassar()
    {
        var validator = new UpdateProfileCommandValidator();
        var command = new UpdateProfileCommand("Nome Válido");

        var validation = await validator.ValidateAsync(command);

        validation.IsValid.Should().BeTrue();
    }
}
