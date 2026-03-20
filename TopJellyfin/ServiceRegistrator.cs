using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using TopJellyfin.Services;

namespace TopJellyfin;

public class ServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<TraktService>();
        serviceCollection.AddScoped<LibraryMatchingService>();
        serviceCollection.AddScoped<RecentlyReleasedService>();
        serviceCollection.AddHostedService<ClientInjector>();
    }
}
