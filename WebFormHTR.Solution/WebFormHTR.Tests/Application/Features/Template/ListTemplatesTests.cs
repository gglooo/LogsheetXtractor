using FluentAssertions;
using MapsterMapper;
using Moq;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;
using WebFormHTR.Application.Common.Mappings;
using Mapster;

namespace WebFormHTR.Tests.Application.Features.Template;

public class ListTemplatesTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();

    [Fact]
    public async Task Handle_ShouldReturnAllTemplates_WhenSearchIsEmpty()
    {
        var file1 = new Domain.Entities.File
        {
            OriginalFileName = "test.pdf", StoredFileName = "test.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        var file2 = new Domain.Entities.File
        {
            OriginalFileName = "sample.pdf", StoredFileName = "sample.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        _dbContext.Files.AddRange(file1, file2);
        await _dbContext.SaveChangesAsync();

        var templates = new List<Domain.Entities.Template>
        {
            new() { Id = Guid.NewGuid(), Name = "Template 1", FileId = file1.Id, Width = 10, Height = 10 },
            new() { Id = Guid.NewGuid(), Name = "Template 2", FileId = file2.Id, Width = 10, Height = 10 }
        };
        _dbContext.Templates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        var query = new ListTemplatesQuery(null);

        var result = await ListTemplatesHandler.Handle(query, _dbContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}