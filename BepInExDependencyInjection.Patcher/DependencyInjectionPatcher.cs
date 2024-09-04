using BepInEx.Preloader.Core.Patching;
using BepInEx.Unity.IL2CPP;
using BepInExDependencyInjection.Core;
using HarmonyLib;

namespace BepInExDependencyInjection.Patcher;

[PatcherAutoPlugin("astral.dependencyinjection.patcher")]
public partial class DependencyInjectionPatcher : BasePatcher
{
    public override void Finalizer()
    {
        var harmony = new Harmony(Id);

        var initializeMethod =
            typeof(IL2CPPChainloader).GetMethod(nameof(IL2CPPChainloader.Initialize), AccessTools.all);

        var loadPluginMethod =
            typeof(IL2CPPChainloader).GetMethod(nameof(IL2CPPChainloader.LoadPlugin), AccessTools.all);

        var chainloaderPatchMethod = ((Func<IL2CPPChainloader, string, bool>) ChainloaderPatch).Method;

        var injectParametersPatchMethod =
            ((Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>) PluginInjectionPatch.Transpiler).Method;

        harmony.Patch(initializeMethod, prefix: new HarmonyMethod(chainloaderPatchMethod));
        harmony.Patch(loadPluginMethod, transpiler: new HarmonyMethod(injectParametersPatchMethod));
    }

    private static bool ChainloaderPatch(IL2CPPChainloader __instance, string gameExePath)
    {
        if (__instance is DependencyInjectionChainloader)
        {
            return true;
        }

        var dependencyInjectionChainloader = new DependencyInjectionChainloader();
        typeof(Preloader).GetProperty("Chainloader", AccessTools.all)!.SetValue(null, dependencyInjectionChainloader);
        dependencyInjectionChainloader.Initialize(gameExePath);

        return false;
    }
}
