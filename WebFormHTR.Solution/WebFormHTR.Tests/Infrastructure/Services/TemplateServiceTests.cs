using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebFormHTR.Application.Features.Residuals;
using WebFormHTR.Application.Features.ROIs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Persistence;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Tests.Common;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class TemplateServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock;
    private readonly TemplateService _templateService;

    public TemplateServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        var residualServiceMock = new ResidualService(_dbContext, _mapperMock.Object);
        var roiServiceMock = new RoiService(_dbContext, _mapperMock.Object, new Mock<IHtrScriptEngine>().Object);
        _scriptEngineMock = new Mock<IHtrScriptEngine>();
        _templateService = new TemplateService(_dbContext, _mapperMock.Object, residualServiceMock, roiServiceMock, _scriptEngineMock.Object);
    }

    [Fact]
    public async Task CloneTemplateAsync_ShouldCloneTemplate_WhenParentExists()
    {
        var parentFile = new Domain.Entities.File
        {
            OriginalFileName = "parent.pdf", StoredFileName = "parent.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        _dbContext.Files.Add(parentFile);
        await _dbContext.SaveChangesAsync();

        var parentId = Guid.NewGuid();
        var parentTemplate = new Template { Id = parentId, Name = "Parent Template", FileId = parentFile.Id };
        _dbContext.Templates.Add(parentTemplate);
        await _dbContext.SaveChangesAsync();

        var newFile = new Domain.Entities.File
        {
            OriginalFileName = "new.pdf", StoredFileName = "new.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync();

        var newTemplateName = "Cloned Template";
        var fileId = newFile.Id;

        var expectedDto = new TemplateDetailDto(
            Guid.NewGuid(),
            newTemplateName,
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
        _mapperMock.Setup(x => x.Map<TemplateDetailDto>(It.IsAny<Template>())).Returns(expectedDto);
        _scriptEngineMock.Setup(x => x.GetPdfDimensionsAsync(It.IsAny<Domain.Entities.File>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PdfDimensionsDto { Width = 100, Height = 200 });

        var result = await _templateService.CloneTemplateAsync(
            parentId,
            newTemplateName,
            fileId,
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        result.Should().Be(expectedDto);
        _dbContext
            .Templates.Should()
            .Contain(t =>
                t.Name == newTemplateName && t.ParentId == parentId && t.FileId == fileId
            );
    }

    [Fact]
    public async Task CloneTemplateAsync_ShouldCloneRoisAndResiduals_WhenParentExists()
    {
        var parentFile = new Domain.Entities.File
        {
            OriginalFileName = "parent.pdf", StoredFileName = "parent.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        _dbContext.Files.Add(parentFile);
        await _dbContext.SaveChangesAsync();

        var parentId = Guid.NewGuid();
        var parentTemplate = new Template { Id = parentId, Name = "Parent Template", FileId = parentFile.Id };
        _dbContext.Templates.Add(parentTemplate);
        await _dbContext.SaveChangesAsync();

        var roi = new Roi
        {
            TemplateId = parentId, VariableName = "ROI 1",
            Coordinates = new Coordinates { Height = 2, Width = 3, X = 4, Y = 5 }, Template = parentTemplate
        };
        var residual = new Residual
        {
            TemplateId = parentId, Template = parentTemplate, Content = "Residual 1",
            Coordinates = new Coordinates { Height = 2, Width = 3, X = 4, Y = 5 }
        };
        _dbContext.Rois.Add(roi);
        _dbContext.Residuals.Add(residual);
        await _dbContext.SaveChangesAsync();

        var newFile = new Domain.Entities.File
        {
            OriginalFileName = "new.pdf", StoredFileName = "new.pdf", StoragePath = "path",
            ContentType = "application/pdf"
        };
        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync();

        var newTemplateName = "Cloned Template";
        var fileId = newFile.Id;

        _mapperMock.Setup(x => x.Map<TemplateDetailDto>(It.IsAny<Template>()))
            .Returns(new TemplateDetailDto(
                Guid.NewGuid(),
                newTemplateName,
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
            ));

        _mapperMock.Setup(x => x.Map<Residual>(It.IsAny<Residual>()))
            .Returns((Residual r) => new Residual
            {
                Id = r.Id,
                Content = r.Content,
                Coordinates = r.Coordinates
            });

        _scriptEngineMock.Setup(x => x.GetPdfDimensionsAsync(It.IsAny<Domain.Entities.File>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PdfDimensionsDto { Width = 100, Height = 200 });

        _mapperMock.Setup(x => x.Map<Roi>(It.IsAny<Roi>()))
            .Returns((Roi r) => new Roi
            {
                Id = r.Id,
                VariableName = r.VariableName,
                Coordinates = r.Coordinates,
                Type = r.Type
            });

        var result = await _templateService.CloneTemplateAsync(
            parentId,
            newTemplateName,
            fileId,
            CancellationToken.None
        );
        await _dbContext.SaveChangesAsync();

        var clonedTemplate = await _dbContext.Templates
            .FirstOrDefaultAsync(t => t.Name == newTemplateName && t.ParentId == parentId);

        clonedTemplate.Should().NotBeNull();

        var clonedRois = await _dbContext.Rois
            .Where(r => r.TemplateId == clonedTemplate!.Id)
            .ToListAsync();
        clonedRois.Should().HaveCount(1);
        clonedRois[0].VariableName.Should().BeEquivalentTo(roi.VariableName);
        clonedRois[0].Coordinates.Should().BeEquivalentTo(roi.Coordinates);
        clonedRois[0].Id.Should().NotBe(roi.Id);

        var clonedResiduals = await _dbContext.Residuals
            .Where(r => r.TemplateId == clonedTemplate!.Id)
            .ToListAsync();
        clonedResiduals.Should().HaveCount(1);
        clonedResiduals[0].Content.Should().Be(residual.Content);
    }
}