using FluentAssertions;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using MapsterMapper;
using Moq;

namespace LogsheetXtractor.Tests.Application.Features.Residuals;

public class ListResidualsForTemplateQueryHandlerTests : IDisposable
{
    private readonly AppDbContext Context;
    private readonly Mock<IMapper> _mapperMock;

    public ListResidualsForTemplateQueryHandlerTests()
    {
        Context = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnResidualsForTemplate()
    {
        var templateId = Guid.NewGuid();
        var otherTemplateId = Guid.NewGuid();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Test Template",
            FileId = Guid.NewGuid(),
        };
        await Context.Templates.AddAsync(template);

        var residual1 = new Residual
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Content = "Content 1",
            Coordinates = new Coordinates(0, 0, 10, 10),
            Template = null!,
        };
        var residual2 = new Residual
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Content = "Content 2",
            Coordinates = new Coordinates(10, 10, 20, 20),
            Template = null!,
        };
        var otherResidual = new Residual
        {
            Id = Guid.NewGuid(),
            TemplateId = otherTemplateId,
            Content = "Other Content",
            Coordinates = new Coordinates(20, 20, 30, 30),
            Template = null!,
        };

        await Context.Residuals.AddRangeAsync(residual1, residual2, otherResidual);
        await Context.SaveChangesAsync();

        var query = new ListResidualsForTemplateQuery(templateId);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ResidualDto>>(It.IsAny<IEnumerable<Residual>>()))
            .Returns(
                (IEnumerable<Residual> residuals) =>
                    residuals.Select(r => new ResidualDto(
                        r.Id,
                        r.TemplateId,
                        r.Content,
                        r.Coordinates,
                        r.CreatedAt,
                        r.UpdatedAt
                    ))
            );

        var result = await ListResidualsForTemplateHandler.Handle(
            query,
            Context,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(r => r.Id == residual1.Id);
        result.Value.Should().Contain(r => r.Id == residual2.Id);
        result.Value.Should().NotContain(r => r.Id == otherResidual.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenTemplateNotFound()
    {
        var templateId = Guid.NewGuid();
        var query = new ListResidualsForTemplateQuery(templateId);

        var result = await ListResidualsForTemplateHandler.Handle(
            query,
            Context,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }
}
