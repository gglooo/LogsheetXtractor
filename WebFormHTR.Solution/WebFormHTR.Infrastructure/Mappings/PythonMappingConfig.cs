using Mapster;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;

namespace WebFormHTR.Infrastructure.Mappings;

public class PythonMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Residual, PythonResidualDto>()
            .Map(dest => dest.Content, src => src.Content)
            .Map(dest => dest.Coords, src => new List<int>
            {
                src.Coordinates.X,
                src.Coordinates.Y,
                src.Coordinates.Width + src.Coordinates.X,
                src.Coordinates.Height + src.Coordinates.Y
            });
        config.NewConfig<Roi, PythonRoiDto>()
            // We map ID to VarName to then map it back to the ROI more easily
            .Map(dest => dest.VarName, src => src.Id)
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.Coords, src => new List<int>
            {
                src.Coordinates.X,
                src.Coordinates.Y,
                src.Coordinates.Width + src.Coordinates.X,
                src.Coordinates.Height + src.Coordinates.Y
            });

        config.NewConfig<PythonResidualDto, Residual>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Content, src => src.Content ?? string.Empty)
            .Map(dest => dest.Coordinates, src => new Coordinates
            {
                X = src.Coords[0],
                Y = src.Coords[1],
                Width = src.Coords[2] - src.Coords[0],
                Height = src.Coords[3] - src.Coords[1]
            });

        config.NewConfig<PythonRoiDto, Roi>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Type, src => Enum.Parse<ERoiType>(src.Type, true))
            .Map(dest => dest.Coordinates, src => new Coordinates
            {
                X = src.Coords[0],
                Y = src.Coords[1],
                Width = src.Coords[2] - src.Coords[0],
                Height = src.Coords[3] - src.Coords[1]
            })
            .Map(dest => dest.VariableName, src => src.VarName);


        config.NewConfig<Template, PythonTemplateConfig>()
            .Map(dest => dest.Residuals, src => src.Residuals)
            .Map(dest => dest.Rois, src => src.Rois)
            .Map(dest => dest.Width, src => src.Width)
            .Map(dest => dest.Height, src => src.Height);

        config.NewConfig<PythonTemplateConfig, Template>()
            .Map(dest => dest.Residuals, src => src.Residuals)
            .Map(dest => dest.Rois, src => src.Rois)
            .Map(dest => dest.Width, src => src.Width)
            .Map(dest => dest.Height, src => src.Height);
    }
}