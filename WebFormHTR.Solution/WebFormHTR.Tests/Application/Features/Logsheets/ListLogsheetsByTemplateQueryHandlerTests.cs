using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
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

        var logsheet1 = new Logsheet { Id = Guid.NewGuid(), TemplateId = templateId, Template = null!, File = null! };
        var logsheet2 = new Logsheet { Id = Guid.NewGuid(), TemplateId = templateId, Template = null!, File = null! };
        var logsheet3 = new Logsheet { Id = Guid.NewGuid(), TemplateId = otherTemplateId, Template = null!, File = null! };

        _dbContext.Logsheets.AddRange(logsheet1, logsheet2, logsheet3);
        await _dbContext.SaveChangesAsync();

        var query = new ListLogsheetsByTemplateQuery(templateId);
        var expectedDtos = new List<LogsheetListDto>
        {
            new LogsheetListDto(logsheet1.Id, templateId, Guid.NewGuid(), ELogSheetStatus.Pending, DateTime.UtcNow),
            new LogsheetListDto(logsheet2.Id, templateId, Guid.NewGuid(), ELogSheetStatus.Pending, DateTime.UtcNow)
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