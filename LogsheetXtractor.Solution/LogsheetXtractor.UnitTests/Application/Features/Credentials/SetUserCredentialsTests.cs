using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Features.Credentials.SetUserCredentials;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.Extensions.Options;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Credentials;

public class SetUserCredentialsTests
{
    [Fact]
    public async Task Handle_ShouldCreateNewHandleReleasePreviousHandleAndSave()
    {
        var storeMock = new Mock<IUserCredentialHandleStore>();
        var dbContextMock = new Mock<IAppDbContext>();
        var keys = new Dictionary<ECredentialType, string>
        {
            [ECredentialType.Google] = "google-key",
        };
        storeMock
            .Setup(s =>
                s.CreateAsync(
                    keys,
                    TimeSpan.FromMinutes(15),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Result.Ok("new-handle"));

        var result = await SetUserCredentialsHandler.Handle(
            new SetUserCredentialsCommand(keys, "old-handle"),
            storeMock.Object,
            dbContextMock.Object,
            Options.Create(new UserCredentialCookieOptions { Ttl = TimeSpan.FromMinutes(15) }),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("new-handle");
        storeMock.Verify(s => s.ReleaseAsync("old-handle", CancellationToken.None), Times.Once);
        dbContextMock.Verify(d => d.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotReleasePreviousHandleOrSave_WhenCreateFails()
    {
        var storeMock = new Mock<IUserCredentialHandleStore>();
        var dbContextMock = new Mock<IAppDbContext>();
        var keys = new Dictionary<ECredentialType, string>
        {
            [ECredentialType.Google] = "google-key",
        };
        storeMock
            .Setup(s =>
                s.CreateAsync(
                    keys,
                    It.IsAny<TimeSpan>(),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Result.Fail<string>("credential storage failed"));

        var result = await SetUserCredentialsHandler.Handle(
            new SetUserCredentialsCommand(keys, "old-handle"),
            storeMock.Object,
            dbContextMock.Object,
            Options.Create(new UserCredentialCookieOptions()),
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        storeMock.Verify(
            s => s.ReleaseAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        dbContextMock.Verify(
            d => d.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
