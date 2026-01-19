using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class ListLogsheetsByTemplateQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;

    public ListLogsheetsByTemplateQueryHandlerTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Handle_ShouldReturnLogsheets_ForGivenTemplate()
    {
        var templateId = Guid.NewGuid();
        var otherTemplateId = Guid.NewGuid();

        var getMockFile = () => new Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path/to/test.pdf",
            ContentType = "application/pdf",
            SizeBytes = 123,
            CreatedAt = DateTime.UtcNow
        };

        var logsheet1 = new Logsheet
        {
            Id = Guid.NewGuid(), TemplateId = templateId, Template = null!, FileId = Guid.NewGuid(),
            File = getMockFile()
        };
        var logsheet2 = new Logsheet
        {
            Id = Guid.NewGuid(), TemplateId = templateId, Template = null!, FileId = Guid.NewGuid(),
            File = getMockFile()
        };
        var logsheet3 = new Logsheet
        {
            Id = Guid.NewGuid(), TemplateId = otherTemplateId, Template = null!, FileId = Guid.NewGuid(),
            File = getMockFile()
        };

        _dbContext.Logsheets.AddRange(logsheet1, logsheet2, logsheet3);
        await _dbContext.SaveChangesAsync();

        var query = new ListLogsheetsByTemplateQuery(templateId);
        var expectedDtos = new List<LogsheetListDto>
        {
            new(logsheet1.Id, templateId, null,
                new FileDto(logsheet1.FileId, "test.pdf", "application/pdf", 123, DateTime.UtcNow),
                ELogSheetStatus.Pending, false, null, DateTime.UtcNow, null),
            new(logsheet2.Id, templateId, null,
                new FileDto(logsheet2.FileId, "test.pdf", "application/pdf", 123, DateTime.UtcNow),
                ELogSheetStatus.Pending, false, null, DateTime.UtcNow, null)
        };

        _mapperMock.Setup(x => x.Map<IEnumerable<LogsheetListDto>>(It.IsAny<IEnumerable<Logsheet>>()))
            .Returns((IEnumerable<Logsheet> logsheets) =>
            {
                var list = logsheets.ToList();
                if (list.Count == 2 && list.Any(l => l.Id == logsheet1.Id) && list.Any(l => l.Id == logsheet2.Id))
                {
                    return expectedDtos;
                }

                return new List<LogsheetListDto>();
            });

        var result = await ListLogsheetsByTemplateHandler.Handle(query, _dbContext, _mapperMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtos);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}