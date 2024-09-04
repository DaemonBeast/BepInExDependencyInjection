using BepInEx.Unity.IL2CPP;

namespace BepInExDependencyInjection.Core.Utilities.Extensions;

public static class ChainloaderExtensions
{
    public static IServiceProvider GetServiceProvider(this IL2CPPChainloader chainloader)
        => ((DependencyInjectionChainloader) chainloader).ServiceProvider!;
}
