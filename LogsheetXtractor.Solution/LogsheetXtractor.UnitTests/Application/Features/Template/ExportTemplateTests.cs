using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.DTOs;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.Interfaces;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class ExportTemplateTests
{
    [Fact]
    public async Task Handle_ShouldPassIncludeRoiValidationsFlag_ToTemplateService()
    {
        var templateId = Guid.NewGuid();
        var query = new ExportTemplateConfigQuery(templateId, IncludeRoiValidations: false);
        var templateServiceMock = new Mock<ITemplateService>();
        var fileServiceMock = new Mock<IFileService>();

        templateServiceMock
            .Setup(x =>
                x.ExportTemplateConfigAsync(
                    templateId,
                    false,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok("{\"content\":[]}"));

        fileServiceMock
            .Setup(x =>
                x.GetFileFromContentAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new GetFileDto
                {
                    Stream = new MemoryStream([1, 2, 3]),
                    ContentType = "application/json",
                    FileName = "template.json",
                }
            );

        var result = await ExportTemplateHandler.Handle(
            query,
            templateServiceMock.Object,
            fileServiceMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        templateServiceMock.Verify(
            x =>
                x.ExportTemplateConfigAsync(
                    templateId,
                    false,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
