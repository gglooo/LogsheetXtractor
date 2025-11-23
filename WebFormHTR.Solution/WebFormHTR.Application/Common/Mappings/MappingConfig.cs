using System.Net;
using Mapster;
using WebFormHTR.Application.DTOs;
using WebFormHTR.Application.Features.File.DTOs;
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

    }
}
