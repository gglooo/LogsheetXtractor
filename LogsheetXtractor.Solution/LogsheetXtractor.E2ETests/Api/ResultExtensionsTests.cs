using FluentAssertions;
using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.Errors;
using Microsoft.AspNetCore.Http;

namespace LogsheetXtractor.E2ETests.Api;

public class ResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_ShouldMapNotFoundErrorTo404()
    {
        var result = Result.Fail(new NotFoundError("Template not found"));

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)httpResult)
            .StatusCode.Should()
            .Be(StatusCodes.Status404NotFound);
        ((IValueHttpResult)httpResult)
            .Value.Should()
            .BeEquivalentTo(new[] { "Template not found" });
    }

    [Fact]
    public void ToHttpResult_ShouldMapValidationErrorTo400()
    {
        var result = Result.Fail(new ValidationError("Name is required"));

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)httpResult)
            .StatusCode.Should()
            .Be(StatusCodes.Status400BadRequest);
        ((IValueHttpResult)httpResult)
            .Value.Should()
            .BeEquivalentTo(new[] { "Name is required" });
    }

    [Fact]
    public void ToHttpResult_ShouldMapInvalidStateErrorTo400()
    {
        var result = Result.Fail(new InvalidStateError("Logsheet is already processing"));

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)httpResult)
            .StatusCode.Should()
            .Be(StatusCodes.Status400BadRequest);
        ((IValueHttpResult)httpResult).Value.Should().BeEquivalentTo(
            new[] { "Logsheet is already processing" }
        );
    }

    [Fact]
    public void ToHttpResult_ShouldMapSuccessfulResultTo200()
    {
        var result = Result.Ok();

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)httpResult).StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void ToHttpResultOfT_ShouldReturnExpectedDtoAndStatusCode()
    {
        var dto = new MappingTestDto("Template A", 3);
        var result = Result.Ok(dto);

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeAssignableTo<IStatusCodeHttpResult>();
        ((IStatusCodeHttpResult)httpResult).StatusCode.Should().Be(StatusCodes.Status200OK);
        ((IValueHttpResult)httpResult).Value.Should().Be(dto);
    }

    private sealed record MappingTestDto(string Name, int RoiCount);
}
