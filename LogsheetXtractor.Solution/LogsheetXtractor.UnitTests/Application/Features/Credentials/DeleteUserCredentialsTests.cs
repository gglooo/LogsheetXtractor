using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;

namespace LogsheetXtractor.UnitTests.Application.Features.Credentials;

public class DeleteUserCredentialsTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        var result = await DeleteUserCredentialsHandler.Handle(
            new DeleteUserCredentialsCommand(),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
    }
}
