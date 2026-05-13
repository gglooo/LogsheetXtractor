using FluentAssertions;
using LogsheetXtractor.Application.Common.Mappings;
using LogsheetXtractor.Application.Features.Residuals;
using LogsheetXtractor.Application.Features.Residuals.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Infrastructure.Services;
using LogsheetXtractor.UnitTests.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Residuals;

public class UpsertResidualCommandHandlerTests : IDisposable
{
    private readonly AppDbContext Context;
    private readonly Mock<IResidualService> _residualServiceMock;

    public UpsertResidualCommandHandlerTests()
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
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Test Template",
            FileId = Guid.NewGuid(),
        };
        await Context.Templates.AddAsync(template);
        await Context.SaveChangesAsync();

        var residualDto = new UpsertResidualDto(null, "Content 1", new Coordinates(0, 0, 10, 10));

        var expectedResult = new ResidualDto(
            Guid.NewGuid(),
            templateId,
            "Content 1",
            new Coordinates(0, 0, 10, 10),
            DateTime.UtcNow,
            null
        );

        _residualServiceMock
            .Setup(s =>
                s.UpsertResidualForTemplateAsync(templateId, residualDto, CancellationToken.None)
            )
            .ReturnsAsync(expectedResult);

        var command = new UpsertResidualCommand(templateId, residualDto);

        var result = await UpsertResidualHandler.Handle(
            command,
            _residualServiceMock.Object,
            Context,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResult);
        _residualServiceMock.Verify(
            s => s.UpsertResidualForTemplateAsync(templateId, residualDto, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldPersistUpsertedResidualAtApplicationBoundary()
    {
        var templateId = Guid.NewGuid();
        await Context.Templates.AddAsync(
            new LogsheetXtractor.Domain.Entities.Template
            {
                Id = templateId,
                Name = "Template With Upserted Residual",
                FileId = Guid.NewGuid(),
            }
        );
        await Context.SaveChangesAsync();

        var residualDto = new UpsertResidualDto(
            null,
            "Upserted content",
            new Coordinates(0, 0, 10, 10)
        );
        var command = new UpsertResidualCommand(templateId, residualDto);
        var service = new ResidualService(Context, CreateMapper());

        var result = await UpsertResidualHandler.Handle(
            command,
            service,
            Context,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var persistedResiduals = await Context
            .Residuals.AsNoTracking()
            .Where(r => r.TemplateId == templateId)
            .ToListAsync();

        persistedResiduals.Should().ContainSingle(r => r.Content == "Upserted content");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenTemplateNotFound()
    {
        var templateId = Guid.NewGuid();
        var residualDto = new UpsertResidualDto(null, "Content 1", new Coordinates(0, 0, 10, 10));
        var command = new UpsertResidualCommand(templateId, residualDto);

        var result = await UpsertResidualHandler.Handle(
            command,
            _residualServiceMock.Object,
            Context,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }

    private static IMapper CreateMapper()
    {
        var config = new TypeAdapterConfig();
        new MappingConfig().Register(config);
        return new Mapper(config);
    }
}
