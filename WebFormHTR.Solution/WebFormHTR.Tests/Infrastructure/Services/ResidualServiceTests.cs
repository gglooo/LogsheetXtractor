using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Tests.Common;

using WebFormHTR.Infrastructure.Persistence;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class ResidualServiceTests : IDisposable
{
    private readonly AppDbContext Context;
    private readonly ResidualService _sut;
    private readonly Mock<IMapper> _mapperMock;

    public ResidualServiceTests()
    {
        Context = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _sut = new ResidualService(Context, _mapperMock.Object);
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    [Fact]
    public async Task SetResidualsForTemplateAsync_ShouldCreateNewResiduals_WhenIdsAreEmpty()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var updateResiduals = new List<SetResidualDto>
        {
            new(null, "Content 1", new Coordinates(0, 0, 10, 10)),
            new(Guid.Empty, "Content 2", new Coordinates(10, 10, 20, 20))
        };

        _mapperMock.Setup(m => m.Map<Residual>(It.IsAny<SetResidualDto>()))
            .Returns((SetResidualDto dto) => new Residual
            {
                Content = dto.Content,
                Coordinates = dto.Coordinates,
                Template = null!
            });

        _mapperMock.Setup(m => m.Map<IEnumerable<ResidualDto>>(It.IsAny<IEnumerable<Residual>>()))
            .Returns((IEnumerable<Residual> residuals) => residuals.Select(r => new ResidualDto(r.Id, r.TemplateId, r.Content, r.Coordinates, r.CreatedAt, r.UpdatedAt)));

        // Act
        var result = await _sut.SetResidualsForTemplateAsync(templateId, updateResiduals, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        var residualsInDb = await Context.Residuals.Where(r => r.TemplateId == templateId).ToListAsync();
        residualsInDb.Should().HaveCount(2);
        residualsInDb.Should().Contain(r => r.Content == "Content 1");
        residualsInDb.Should().Contain(r => r.Content == "Content 2");
    }

    [Fact]
    public async Task SetResidualsForTemplateAsync_ShouldUpdateExistingResiduals_WhenIdsAreProvided()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var existingResidual = new Residual
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Content = "Old Content",
            Coordinates = new Coordinates(0, 0, 10, 10),
            Template = null!
        };
        await Context.Residuals.AddAsync(existingResidual);
        await Context.SaveChangesAsync();

        var updateResiduals = new List<SetResidualDto>
        {
            new(existingResidual.Id, "New Content", new Coordinates(0, 0, 10, 10))
        };

        _mapperMock.Setup(m => m.Map(It.IsAny<SetResidualDto>(), It.IsAny<Residual>()))
            .Callback((SetResidualDto dto, Residual residual) =>
            {
                residual.Content = dto.Content;
            });

        _mapperMock.Setup(m => m.Map<IEnumerable<ResidualDto>>(It.IsAny<IEnumerable<Residual>>()))
            .Returns((IEnumerable<Residual> residuals) => residuals.Select(r => new ResidualDto(r.Id, r.TemplateId, r.Content, r.Coordinates, r.CreatedAt, r.UpdatedAt)));

        // Act
        var result = await _sut.SetResidualsForTemplateAsync(templateId, updateResiduals, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var residualInDb = await Context.Residuals.FirstAsync(r => r.Id == existingResidual.Id);
        residualInDb.Content.Should().Be("New Content");
    }

    [Fact]
    public async Task SetResidualsForTemplateAsync_ShouldDeleteResiduals_WhenNotIncludedInUpdate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var residualToDelete = new Residual
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Content = "To Delete",
            Coordinates = new Coordinates(0, 0, 10, 10),
            Template = null!
        };
        var residualToKeep = new Residual
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Content = "To Keep",
            Coordinates = new Coordinates(10, 10, 20, 20),
            Template = null!
        };
        await Context.Residuals.AddRangeAsync(residualToDelete, residualToKeep);
        await Context.SaveChangesAsync();

        var updateResiduals = new List<SetResidualDto>
        {
            new(residualToKeep.Id, "To Keep", new Coordinates(10, 10, 20, 20))
        };

        _mapperMock.Setup(m => m.Map(It.IsAny<SetResidualDto>(), It.IsAny<Residual>()))
            .Callback((SetResidualDto dto, Residual residual) => { });

        _mapperMock.Setup(m => m.Map<IEnumerable<ResidualDto>>(It.IsAny<IEnumerable<Residual>>()))
            .Returns((IEnumerable<Residual> residuals) => residuals.Select(r => new ResidualDto(r.Id, r.TemplateId, r.Content, r.Coordinates, r.CreatedAt, r.UpdatedAt)));

        // Act
        await _sut.SetResidualsForTemplateAsync(templateId, updateResiduals, CancellationToken.None);

        // Assert
        var residualsInDb = await Context.Residuals.Where(r => r.TemplateId == templateId).ToListAsync();
        residualsInDb.Should().HaveCount(1);
        residualsInDb.Should().Contain(r => r.Id == residualToKeep.Id);
        residualsInDb.Should().NotContain(r => r.Id == residualToDelete.Id);
    }
}
