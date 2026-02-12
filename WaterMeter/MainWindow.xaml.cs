using CommunityToolkit.Mvvm.Messaging;
using SimpleInjector;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WaterMeter.API;
using WaterMeter.Messages;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace WaterMeter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow,IRecipient<LogArrivedMessage>
{

    public MainWindow(MainViewModel viewModel,ISnackbarService snackbarService)
    {
        InitializeComponent();
        ApplicationThemeManager.Apply(this);
        base.DataContext = viewModel;
        WeakReferenceMessenger.Default.Register(this);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
    }

    public void Receive(LogArrivedMessage message)
    {
        Dispatcher.Invoke(() =>
        {
            if (LogTextBlock.Text.Length >= 10000)
            {
                LogTextBlock.Text = string.Empty;
            }
            var log = message.Value;
            var logMessage = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}|{log.Level}|{log.Name}|{log.Content}{Environment.NewLine}";
            LogTextBlock.AppendText(logMessage);
            this.ScrollViewer.ScrollToBottom();
        });
    }
}