using FluentAssertions;
using LogsheetXtractor.Application.Common.Mappings;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.Tests.Common;
using Mapster;
using MapsterMapper;
using Moq;

namespace LogsheetXtractor.Tests.Application.Features.Template;

public class ListTemplatesTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();

    [Fact]
    public async Task Handle_ShouldReturnAllTemplates_WhenSearchIsEmpty()
    {
        var file1 = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        var file2 = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "sample.pdf",
            StoredFileName = "sample.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.AddRange(file1, file2);
        await _dbContext.SaveChangesAsync();

        var templates = new List<LogsheetXtractor.Domain.Entities.Template>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Template 1",
                FileId = file1.Id,
                Width = 10,
                Height = 10,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Template 2",
                FileId = file2.Id,
                Width = 10,
                Height = 10,
            },
        };
        _dbContext.Templates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        var query = new ListTemplatesQuery(null);

        var config = new TypeAdapterConfig();
        new MappingConfig().Register(config);
        var mapper = new Mapper(config);

        var result = await ListTemplatesHandler.Handle(query, _dbContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
