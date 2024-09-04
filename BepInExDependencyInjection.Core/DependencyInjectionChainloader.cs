using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Microsoft.Extensions.DependencyInjection;

namespace BepInExDependencyInjection.Core;

public class DependencyInjectionChainloader : IL2CPPChainloader
{
    public IServiceProvider? ServiceProvider { get; private set; }

    internal IList<PluginInfo>? PluginsCache { get; private set; }

    protected override IList<PluginInfo> ModifyLoadOrder(IList<PluginInfo> plugins)
    {
        var orderedPlugins = base.ModifyLoadOrder(plugins);
        if (ServiceProvider != null!)
        {
            // Dependency injection only works for plugins discovered by BepInEx
            return orderedPlugins;
        }

        PluginsCache = orderedPlugins;

        var services = new ServiceCollection();

        foreach (var pluginInfo in orderedPlugins)
        {
            var pluginAssembly = Assembly.LoadFrom(pluginInfo.Location);
            var pluginType = pluginAssembly.GetType(pluginInfo.TypeName)!;

            RuntimeHelpers.RunModuleConstructor(pluginType.Module.ModuleHandle);

            var pluginStartupType = typeof(PluginStartup<>).MakeGenericType(pluginType);

            var pluginStartupTypes = pluginAssembly
                .GetTypes()
                .Where(t => pluginStartupType.IsAssignableFrom(t))
                .ToArray();

            switch (pluginStartupTypes.Length)
            {
                case 0:
                {
                    break;
                }
                case 1:
                {
                    ((PluginStartup) Activator.CreateInstance(pluginStartupTypes[0])!).ConfigureServices(services);

                    break;
                }
                default:
                {
                    throw new Exception(
                        $"A plugin can only have 0 or 1 plugin startups. Found {pluginStartupTypes.Length}");
                }
            }
        }

        ServiceProvider = services.BuildServiceProvider();

        return orderedPlugins;
    }
}
