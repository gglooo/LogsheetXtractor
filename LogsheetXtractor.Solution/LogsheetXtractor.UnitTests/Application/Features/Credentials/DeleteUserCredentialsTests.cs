using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Interfaces;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Credentials;

public class DeleteUserCredentialsTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        var storeMock = new Mock<IUserCredentialHandleStore>();
        var dbContextMock = new Mock<IAppDbContext>();

        var result = await DeleteUserCredentialsHandler.Handle(
            new DeleteUserCredentialsCommand("0123456789abcdef0123456789abcdef"),
            storeMock.Object,
            dbContextMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        storeMock.Verify(
            s =>
                s.ReleaseAsync(
                    "0123456789abcdef0123456789abcdef",
                    CancellationToken.None
            ),
            Times.Once
        );
        dbContextMock.Verify(d => d.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
