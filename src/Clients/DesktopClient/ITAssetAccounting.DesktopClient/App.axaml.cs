using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ITAssetAccounting.DesktopClient.Services;
using ITAssetAccounting.DesktopClient.ViewModels;
using ITAssetAccounting.DesktopClient.Views;

namespace ITAssetAccounting.DesktopClient;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var api = new ApiClient(new HttpClient { BaseAddress = new Uri("http://localhost:5100/") });
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(api)
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
