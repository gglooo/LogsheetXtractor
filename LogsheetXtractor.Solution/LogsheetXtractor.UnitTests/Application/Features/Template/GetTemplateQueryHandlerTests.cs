using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class GetTemplateQueryHandlerTests
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnTemplate_WhenTemplateExists()
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

        var templateId = Guid.NewGuid();
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = templateId,
            Name = "Existing Template",
            FileId = file.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var expectedDto = new TemplateDetailDto(
            templateId,
            "Existing Template",
            0,
            0,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            [],
            [],
            true
        );
        _mapperMock
            .Setup(x =>
                x.Map<TemplateDetailDto>(
                    It.Is<LogsheetXtractor.Domain.Entities.Template>(t => t.Id == templateId)
                )
            )
            .Returns(expectedDto);

        var query = new GetTemplateQuery(templateId);

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTemplateDoesNotExist()
    {
        var query = new GetTemplateQuery(Guid.NewGuid());

        var result = await GetTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
