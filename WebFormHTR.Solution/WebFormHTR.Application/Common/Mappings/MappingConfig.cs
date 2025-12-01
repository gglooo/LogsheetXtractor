using System.Net;
using Mapster;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
using WebFormHTR.Application.Features.Logsheets;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Features.Template.DTOs;
using WebFormHTR.Domain.Entities;
using File = WebFormHTR.Domain.Entities.File;

namespace WebFormHTR.Application.Common.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Template, Template>();

        config.NewConfig<Template, TemplateDetailDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Parent, src => src.Parent)
            .Map(dest => dest.File, src => src.File)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        config.NewConfig<Template, TemplateListDto>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ParentId, src => src.ParentId)
            .Map(dest => dest.FileId, src => src.FileId);

        config.NewConfig<File, FileDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FileName, src => src.OriginalFileName)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ContentType, src => src.ContentType)
            .Map(dest => dest.SizeBytes, src => src.SizeBytes);

        config.NewConfig<Logsheet, LogsheetDetailDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Template, src => src.Template)
            .Map(dest => dest.File, src => src.File)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.ProcessedAt, src => src.ProcessedAt)
            .Map(dest => dest.AlignmentData, src => src.AlignmentData);

        config.NewConfig<CreateLogsheetCommand, Logsheet>()
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.FileId, src => src.FileId)
            .IgnoreNullValues(true);

        config.NewConfig<Logsheet, LogsheetListDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FileId, src => src.FileId)
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.ProcessedAt, src => src.ProcessedAt);

        config.NewConfig<CreateRoiDto, Roi>()
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.VariableName, src => src.VariableName)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<SetRoiDto, Roi>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.VariableName, src => src.VariableName)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<Roi, RoiDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.VariableName, src => src.VariableName)
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<RoiDto, Roi>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.VariableName, src => src.VariableName)
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<UpsertRoiDto, Roi>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.VariableName, src => src.VariableName)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<PatchLogsheetDto, Logsheet>()
            .IgnoreNullValues(true);

        config.NewConfig<SelectRoisOutputDto, DetectRoisResponseDto>();

        config.NewConfig<Residual, ResidualDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<ResidualDto, Residual>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TemplateId, src => src.TemplateId)
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.Coordinates, src => src.Coordinates);

        config.NewConfig<Residual, Residual>()
            .Ignore(dest => dest.Template);

        config.NewConfig<Roi, Roi>()
            .Ignore(dest => dest.Template);
    }
}