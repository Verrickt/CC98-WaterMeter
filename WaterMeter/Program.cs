using SimpleInjector;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.Messaging;
using WaterMeter.API;
using WaterMeter.Config;
using WaterMeter.Stat;
using Wpf.Ui;

namespace WaterMeter;

public static class Program
{

    [STAThread]
    static void Main()
    {
        var container = Bootstrap();
        RunApplication(container);
    }
    private static void RunApplication(Container container)
    {
        try
        {

            var app = new App();
            var mainWindow = container.GetInstance<MainWindow>();
            app.Run(mainWindow);
        }
        catch (Exception ex)
        {
            //Log the exception and exit
        }
    }
    private static Container Bootstrap()
    {
        Container container = new Container();
        container.RegisterSingleton<OverWatcher>();
        container.RegisterSingleton<CacheManager>();
        container.RegisterSingleton<OverWatchContext>(() =>
        {
            var cache = container.GetInstance<CacheManager>();
            var context = new OverWatchContext(container.GetInstance<WaterMetterConfig>(),
                container.GetInstance<ConfigReader>(),cache);
            context.InitAsync().Wait();
            return context;
        });
        container.RegisterSingleton<CC98API>();
        container.RegisterSingleton<ConfigReader>();
        container.RegisterSingleton<ISnackbarService,SnackbarService>();
        container.RegisterSingleton<WaterMetterConfig>(() =>
        {
            var reader = container.GetInstance<ConfigReader>();
            return reader.ReadConfig();
        });
        container.RegisterSingleton<MainViewModel>();
        container.RegisterSingleton<RefreshTokenHttpMessageHandler>();
        container.Register<MainWindow>();
        container.RegisterSingleton(() => new HttpClient(container.GetInstance<RefreshTokenHttpMessageHandler>()));
        container.RegisterSingleton<StatGenerator>();
        container.RegisterSingleton<UBBFormatter>();
        container.Verify();
        return container;
    }
}