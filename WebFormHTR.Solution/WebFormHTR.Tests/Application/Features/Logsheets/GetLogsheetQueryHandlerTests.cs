using FluentAssertions;
using FluentResults;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Errors;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using Xunit;

namespace WebFormHTR.Tests.Application.Features.Logsheets;

public class GetLogsheetQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnLogsheet_WhenLogsheetExists()
    {
        var template = new Domain.Entities.Template { Id = Guid.NewGuid(), Name = "Template", File = new Domain.Entities.File { StoredFileName = "t.pdf"} };
        var file = new Domain.Entities.File { Id = Guid.NewGuid(), StoredFileName = "l.pdf" };
        
        var logsheet = new Logsheet 
        { 
            Id = Guid.NewGuid(), 
            Template = template,
            File = file,
            Status = Domain.Enums.ELogSheetStatus.Pending
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var query = new GetLogsheetQuery(logsheet.Id);

        var expectedDto = new LogsheetDetailDto(logsheet.Id, 
            new TemplateListDto(template.Id.ToString(), template.Name, null, null), 
            null, 
            new WebFormHTR.Application.Features.File.DTOs.FileDto(file.Id, file.OriginalFileName, file.ContentType, file.SizeBytes, file.CreatedAt), 
            logsheet.Status, 
            null, 
            null);

        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(It.IsAny<Logsheet>()))
            .Returns(expectedDto);

        var result = await GetLogsheetHandler.Handle(query, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var query = new GetLogsheetQuery(Guid.NewGuid());

        var result = await GetLogsheetHandler.Handle(query, _dbContext, _mapperMock.Object, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Logsheet not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
