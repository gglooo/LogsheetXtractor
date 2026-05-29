using System.Reflection;
using System.Reflection.Emit;
using LogsheetXtractor.API.Endpoints;
using LogsheetXtractor.Application;
using LogsheetXtractor.Domain.Entities.Base;
using LogsheetXtractor.Infrastructure.Persistence;
using NetArchTest.Rules;
using Xunit;

namespace LogsheetXtractor.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "LogsheetXtractor.Domain";
    private const string ApplicationNamespace = "LogsheetXtractor.Application";
    private const string InfrastructureNamespace = "LogsheetXtractor.Infrastructure";
    private const string ApiNamespace = "LogsheetXtractor.API";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnOtherLayers()
    {
        var assembly = typeof(BaseEntity).Assembly;

        AssertLayerDoesNotDependOn(assembly, ApplicationNamespace, "Application");
        AssertLayerDoesNotDependOn(assembly, InfrastructureNamespace, "Infrastructure");
        AssertLayerDoesNotDependOn(assembly, ApiNamespace, "API");
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOnInfrastructure_Or_Api()
    {
        var assembly = typeof(ApplicationAssemblyReference).Assembly;

        AssertLayerDoesNotDependOn(assembly, InfrastructureNamespace, "Infrastructure");
        AssertLayerDoesNotDependOn(assembly, ApiNamespace, "API");
    }

    [Fact]
    public void Infrastructure_Should_Not_HaveDependencyOnApi()
    {
        var assembly = typeof(AppDbContext).Assembly;

        AssertLayerDoesNotDependOn(assembly, ApiNamespace, "API");
    }

    [Fact]
    public void ApiEndpoints_Should_Not_HaveDependencyOnInfrastructure()
    {
        var result = Types
            .InAssembly(typeof(TemplateEndpoints).Assembly)
            .That()
            .ResideInNamespace("LogsheetXtractor.API.Endpoints")
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            "API endpoints should not depend on Infrastructure"
        );
    }

    [Fact]
    public void InfrastructureServices_Should_Not_CallSaveChangesAsync()
    {
        var assembly = typeof(AppDbContext).Assembly;
        var offenders = assembly
            .GetTypes()
            .Where(t =>
                t.Namespace is not null
                && t.Namespace.StartsWith(
                    "LogsheetXtractor.Infrastructure.Services",
                    StringComparison.Ordinal
                )
            )
            .SelectMany(GetMethods)
            .Where(CallsSaveChangesAsync)
            .Select(m => $"{m.DeclaringType?.FullName}.{m.Name}")
            .Order()
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "Infrastructure services should not call SaveChangesAsync. Offenders: "
                + string.Join(", ", offenders)
        );
    }

    private void AssertLayerDoesNotDependOn(
        System.Reflection.Assembly assembly,
        string dependencyNamespace,
        string dependencyName
    )
    {
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(dependencyNamespace)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"{assembly.GetName().Name} should not depend on {dependencyName}"
        );
    }

    private static IEnumerable<MethodBase> GetMethods(Type type)
    {
        const BindingFlags flags =
            BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

        foreach (var method in type.GetMethods(flags))
        {
            yield return method;
        }

        foreach (var constructor in type.GetConstructors(flags))
        {
            yield return constructor;
        }
    }

    private static bool CallsSaveChangesAsync(MethodBase method)
    {
        var body = method.GetMethodBody();
        if (body is null)
        {
            return false;
        }

        var module = method.Module;
        var bytes = body.GetILAsByteArray();
        if (bytes is null)
        {
            return false;
        }

        for (var offset = 0; offset < bytes.Length;)
        {
            var opcode = ReadOpCode(bytes, ref offset);
            var operandOffset = offset;
            offset += GetOperandSize(opcode, bytes, offset);

            if (opcode.OperandType != OperandType.InlineMethod)
            {
                continue;
            }

            var token = BitConverter.ToInt32(bytes, operandOffset);
            try
            {
                if (module.ResolveMethod(token)?.Name == "SaveChangesAsync")
                {
                    return true;
                }
            }
            catch (ArgumentException)
            {
                // Some generic method tokens cannot be resolved without full generic context.
            }
        }

        return false;
    }

    private static OpCode ReadOpCode(byte[] bytes, ref int offset)
    {
        var value = bytes[offset++];
        if (value != 0xFE)
        {
            return SingleByteOpCodes[value];
        }

        return MultiByteOpCodes[bytes[offset++]];
    }

    private static int GetOperandSize(OpCode opcode, byte[] bytes, int offset)
    {
        return opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget => 1,
            OperandType.ShortInlineI => 1,
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI => 4,
            OperandType.InlineBrTarget => 4,
            OperandType.InlineField => 4,
            OperandType.InlineMethod => 4,
            OperandType.InlineSig => 4,
            OperandType.InlineString => 4,
            OperandType.InlineTok => 4,
            OperandType.InlineType => 4,
            OperandType.ShortInlineR => 4,
            OperandType.InlineI8 => 8,
            OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4
                + BitConverter.ToInt32(bytes, offset) * 4,
            _ => throw new NotSupportedException(
                $"Unsupported operand type {opcode.OperandType}"
            ),
        };
    }

    private static readonly OpCode[] SingleByteOpCodes = new OpCode[0x100];
    private static readonly OpCode[] MultiByteOpCodes = new OpCode[0x100];

    static ArchitectureTests()
    {
        foreach (
            var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        )
        {
            if (field.GetValue(null) is not OpCode opcode)
            {
                continue;
            }

            var value = (ushort)opcode.Value;
            if (value < 0x100)
            {
                SingleByteOpCodes[value] = opcode;
            }
            else if ((value & 0xFF00) == 0xFE00)
            {
                MultiByteOpCodes[value & 0xFF] = opcode;
            }
        }
    }
}
