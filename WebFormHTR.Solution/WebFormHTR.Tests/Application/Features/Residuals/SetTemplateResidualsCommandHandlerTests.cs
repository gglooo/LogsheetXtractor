using FluentAssertions;
using Moq;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Application.Features.Residuals;

public class SetTemplateResidualsCommandHandlerTests : IDisposable
{
    private readonly AppDbContext Context;
    private readonly Mock<IResidualService> _residualServiceMock;

    public SetTemplateResidualsCommandHandlerTests()
    {
        Context = TestDbContextFactory.Create();
        _residualServiceMock = new Mock<IResidualService>();
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCallService()
    {
        var templateId = Guid.NewGuid();
        var template = new WebFormHTR.Domain.Entities.Template
            { Id = templateId, Name = "Test Template", FileId = Guid.NewGuid() };
        await Context.Templates.AddAsync(template);
        await Context.SaveChangesAsync();

        var residualsDto = new List<SetResidualDto>
        {
            new(null, "Content 1", new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 })
        };

        var expectedResult = new List<ResidualDto>
        {
            new(Guid.NewGuid(), templateId, "Content 1", new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 })
        };

        _residualServiceMock
            .Setup(s => s.SetResidualsForTemplateAsync(templateId, residualsDto, CancellationToken.None))
            .ReturnsAsync(expectedResult);

        var command = new SetTemplateResidualsCommand(templateId, residualsDto);

        var result =
            await SetTemplateResidualsHandler.Handle(command, _residualServiceMock.Object, Context,
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResult);
        _residualServiceMock.Verify(
            s => s.SetResidualsForTemplateAsync(templateId, residualsDto, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenTemplateNotFound()
    {
        var templateId = Guid.NewGuid();
        var residualsDto = new List<SetResidualDto>();
        var command = new SetTemplateResidualsCommand(templateId, residualsDto);

        var result =
            await SetTemplateResidualsHandler.Handle(command, _residualServiceMock.Object, Context,
                CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }
}