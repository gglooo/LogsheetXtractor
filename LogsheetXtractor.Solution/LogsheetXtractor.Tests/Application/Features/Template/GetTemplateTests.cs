using FluentAssertions;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LogsheetXtractor.Tests.Application.Features.Template;

public class GetTemplateTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnTemplate_WhenFound()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Test Template",
            FileId = file.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var query = new GetTemplateQuery(template.Id);

        var expectedDto = new TemplateDetailDto(
            template.Id,
            template.Name,
            0,
            0,
            null,
            null,
            null,
            null,
            DateTime.Now,
            DateTime.Now,
            [],
            [],
            true
        );
        _mapperMock
            .Setup(m =>
                m.Map<TemplateDetailDto?>(It.IsAny<LogsheetXtractor.Domain.Entities.Template>())
            )
            .Returns(expectedDto);

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        var query = new GetTemplateQuery(Guid.NewGuid());

        _mapperMock.Setup(m => m.Map<TemplateDetailDto?>(null)).Returns((TemplateDetailDto?)null);

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
