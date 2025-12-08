using Mapster;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;

namespace WebFormHTR.Infrastructure.Mappings;

public class PythonMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Residual, PythonResidualDto>()
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.Coords, src => new List<float>
            {
                src.Coordinates.X,
                src.Coordinates.Y,
                src.Coordinates.Width,
                src.Coordinates.Height
            });
        config.NewConfig<Roi, PythonRoiDto>()
            .Map(dest => dest.VarName, src => src.VariableName)
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.Coords, src => new List<float>
            {
                src.Coordinates.X,
                src.Coordinates.Y,
                src.Coordinates.Width,
                src.Coordinates.Height
            });


        config.NewConfig<Template, PythonTemplateConfig>()
            .Map(dest => dest.Residuals, src => src.Residuals)
            .Map(dest => dest.Rois, src => src.Rois)
            .Map(dest => dest.Width, src => src.Width)
            .Map(dest => dest.Height, src => src.Height);
    }
}