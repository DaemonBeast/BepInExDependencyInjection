using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Unity.IL2CPP;
using BepInExDependencyInjection.Core.Utilities.Extensions;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;

namespace BepInExDependencyInjection.Patcher;

public static class PluginInjectionPatch
{
    private static readonly MethodInfo CreateInstanceMethodInfo;
    private static readonly MethodInfo CreateInstanceWithInjectedParametersMethodInfo;

    static PluginInjectionPatch()
    {
        CreateInstanceMethodInfo =
            typeof(Activator).GetMethod(nameof(Activator.CreateInstance), AccessTools.all, new[] { typeof(Type) })!;

        CreateInstanceWithInjectedParametersMethodInfo =
            ((Func<Type, object>) CreateInstanceWithInjectedParameters).Method;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(CreateInstanceMethodInfo))
            {
                yield return new CodeInstruction(OpCodes.Call, CreateInstanceWithInjectedParametersMethodInfo);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static object CreateInstanceWithInjectedParameters(Type type)
        => ActivatorUtilities.CreateInstance(IL2CPPChainloader.Instance.GetServiceProvider(), type);
}
