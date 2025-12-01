using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Tests.Common;
using WebFormHTR.Infrastructure.Persistence;

namespace WebFormHTR.Tests.Application.Features.Residuals;

public class CreateResidualCommandHandlerTests : IDisposable
{
    private readonly AppDbContext Context;
    private readonly Mock<IMapper> _mapperMock;

    public CreateResidualCommandHandlerTests()
    {
        Context = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCreateResiduals()
    {
        var templateId = Guid.NewGuid();
        var template = new WebFormHTR.Domain.Entities.Template
            { Id = templateId, Name = "Test Template", FileId = Guid.NewGuid() };
        await Context.Templates.AddAsync(template);
        await Context.SaveChangesAsync();

        var residualsDto = new List<CreateResidualDto>
        {
            new("Content 1", new Coordinates { X = 0, Y = 0, Width = 10, Height = 10 }),
            new("Content 2", new Coordinates { X = 10, Y = 10, Width = 20, Height = 20 })
        };

        var command = new CreateResidualCommand(templateId, residualsDto);

        _mapperMock.Setup(m => m.Map<List<Residual>>(It.IsAny<IEnumerable<CreateResidualDto>>()))
            .Returns((IEnumerable<CreateResidualDto> dtos) => dtos.Select(d => new Residual
            {
                Content = d.Content,
                Coordinates = d.Coordinates,
                Template = null!
            }).ToList());

        var result = await CreateResidualHandler.Handle(command, Context, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var residualsInDb = await Context.Residuals.Where(r => r.TemplateId == templateId).ToListAsync();
        residualsInDb.Should().HaveCount(2);
        residualsInDb.Should().Contain(r => r.Content == "Content 1");
        residualsInDb.Should().Contain(r => r.Content == "Content 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenTemplateNotFound()
    {
        var templateId = Guid.NewGuid();
        var residualsDto = new List<CreateResidualDto>();
        var command = new CreateResidualCommand(templateId, residualsDto);

        var result = await CreateResidualHandler.Handle(command, Context, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }
}