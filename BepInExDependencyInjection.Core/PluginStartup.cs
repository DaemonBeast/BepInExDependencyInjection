using BepInEx.Unity.IL2CPP;
using Microsoft.Extensions.DependencyInjection;

namespace BepInExDependencyInjection.Core;

public abstract class PluginStartup<TPlugin> : PluginStartup
    where TPlugin : BasePlugin
{
}

public abstract class PluginStartup
{
    private protected PluginStartup()
    {
    }

    public abstract void ConfigureServices(IServiceCollection services);
}
