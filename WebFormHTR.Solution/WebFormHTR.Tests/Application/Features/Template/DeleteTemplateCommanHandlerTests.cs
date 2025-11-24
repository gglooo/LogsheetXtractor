using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Features.Template;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Application.Features.Template;

public class DeleteTemplateCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();

    [Fact]
    public async Task Handle_ShouldDeleteTemplate_WhenExists()
    {
        var templateId = Guid.NewGuid();
        var template = new Domain.Entities.Template { Id = templateId, File = null! };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTemplateCommand(templateId);

        var result = await DeleteTemplateHandler.Handle(command, _dbContext, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var deletedTemplate = await _dbContext.Templates.FindAsync(templateId);
        deletedTemplate.Should().NotBeNull();
        deletedTemplate.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTemplate_NotFound()
    {
        var command = new DeleteTemplateCommand(Guid.NewGuid());

        var result = await DeleteTemplateHandler.Handle(command, _dbContext, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Template not found");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}