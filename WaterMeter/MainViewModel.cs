using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using WaterMeter.Config;
using WaterMeter.Messages;
using WaterMeter.Stat;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace WaterMeter;

public partial class MainViewModel : ObservableRecipient, IRecipient<ConfigChangedMessage>
{
    private readonly OverWatcher _overWatcher;
    private readonly WaterMetterConfig _config;
    private readonly ConfigReader _reader;
    private readonly StatGenerator _statGenerator;
    private readonly UBBFormatter _formatter;
    private readonly ISnackbarService _snackbarService;
    private Dispatcher Dispatcher => Application.Current.Dispatcher;

    public MainViewModel(OverWatcher overWatcher, WaterMetterConfig config, ConfigReader reader, StatGenerator statGenerator, UBBFormatter formatter,ISnackbarService snackbarService)
    {
        _overWatcher = overWatcher;
        _config = config;
        _reader = reader;
        _statGenerator = statGenerator;
        _formatter = formatter;
        TopicId = config.TopicId;
        RefreshToken = config.RefreshToken;
        Interval = config.OverWatchInterval.ToString();
        CurrentFloor = (Math.Min(int.Parse(_config.CurrentFloor), int.Parse(_config.MaxFloors))).ToString(); ;
        MaxFloor = _config.MaxFloors;
        _snackbarService = snackbarService;
        WeakReferenceMessenger.Default.Register(this);
        IsConfigInputValid = CheckConfig(TopicId,RefreshToken,Interval);
        IsConfigValid = CheckConfig(config.TopicId, config.RefreshToken, config.OverWatchInterval.ToString());
        NotOverwatching = true;
    }

    private bool CheckConfig(string topicId,string refreshToken,string interval)
    {
        if (int.TryParse(interval, out var intervalResult)&&intervalResult>=3)
        {
            return !string.IsNullOrWhiteSpace(topicId) && !string.IsNullOrWhiteSpace(refreshToken);
        }

        return false;
    }

    public string MaxFloor
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool CanStop => !NotOverwatching && IsConfigValid;

    public bool IsConfigValid
    {
        get => field;
        private set
        {
            SetProperty(ref field,value);
            OnPropertyChanged(nameof(IsConfigInputValid));
        }
    }

    public bool IsConfigInputValid
    {
        get => field;
        private set
        {
            SetProperty(ref field, value);
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
            SaveConfigCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanSaveConfig => IsConfigInputValid && NotOverwatching;
    public bool NotOverwatching
    {
        get => field;
        private set
        {
            SetProperty(ref field, value);
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanSaveConfig));
            StartOverWatchCommand.NotifyCanExecuteChanged();
            ResetCurrentFloorCommand.NotifyCanExecuteChanged();
            StopOverWatchCommand.NotifyCanExecuteChanged();
            GenerateStatCommand.NotifyCanExecuteChanged();
            SaveConfigCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanStart
    {
        get => NotOverwatching && IsConfigValid;
    }

    public string TopicId
    {
        get => field;
        set
        {
            SetProperty(ref field, value);
            IsConfigInputValid = CheckConfig(TopicId, RefreshToken, Interval);
        }
    }

    public string RefreshToken
    {
        get => field;
        set
        {
            SetProperty(ref field, value);
            IsConfigInputValid = CheckConfig(TopicId, RefreshToken, Interval);
        }
    }

    public string Interval
    {
        get => field;
        set { SetProperty(ref field, value); IsConfigInputValid = CheckConfig(TopicId, RefreshToken, Interval);
        }
    }

    public string CurrentFloor
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task ResetCurrentFloor()
    {
        _overWatcher.StopOverWatch();
        CurrentFloor = "1";
        _config.CurrentFloor = "1";
        await _reader.SaveConfigAsync(_config);
    }

    [RelayCommand(CanExecute = nameof(CanSaveConfig))]
    private async Task SaveConfig()
    {
        if (String.IsNullOrEmpty(RefreshToken))
        {
            _snackbarService.Show("配置保存失败", "RefreshToken不能为空", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
            return;
        }
        if (String.IsNullOrEmpty(TopicId))
        {
            _snackbarService.Show("配置保存失败", "TopicId不能为空", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
            return;
        }
        if (int.TryParse(Interval, out var interval)&&interval>=3)
        {
            if (TopicId != _config.TopicId)
            {
                _config.CurrentFloor = "1";
                _config.MaxFloors = "0";
                _config.TopicId = TopicId;
            }
            else
            {
                _config.TopicId = TopicId;
            }
            _config.OverWatchInterval = interval;
            _config.RefreshToken = RefreshToken;
            WeakReferenceMessenger.Default.Send(new ConfigChangedMessage(_config));
            await _reader.SaveConfigAsync(_config);
            IsConfigInputValid = true;
            _snackbarService.Show("配置保存成功","配置已保存成功",ControlAppearance.Success,new SymbolIcon(SymbolRegular.Checkmark24),TimeSpan.FromSeconds(3));
        }
        else
        {
            _snackbarService.Show("配置保存失败", "间隔必须是大于3的正整数", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
        }
    }


    [RelayCommand(CanExecute = nameof(CanStart))]
    private void StartOverWatch()
    {
        if (CheckConfig(TopicId, RefreshToken, Interval))
        {
            NotOverwatching = false;
            _overWatcher.StartOverWatch();
        }
        else
        {
            _snackbarService.Show("配置无效", "请检查配置", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(3));
        }

    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void StopOverWatch()
    {
        NotOverwatching = true;
        _overWatcher.StopOverWatch();
    }

    [RelayCommand]
    private void OpenConfigDir()
    {
        var path = ConfigReader.BasePath;
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true, // 必须为 true 才能直接打开路径
            Verb = "open"
        });
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task GenerateStatAsync()
    {
        
        var result = await _statGenerator.RunStatsAsync(_config.TopicId,500);
        var ans = _formatter.Format(result);
        var w = new StatResultWindow
        {
            DataContext = new StatResultViewModel(ans)
        };
        w.ShowDialog();
    }
    public void Receive(ConfigChangedMessage message)
    {
        Dispatcher.Invoke(() =>
        {
            CurrentFloor = (Math.Min(int.Parse(message.Value.CurrentFloor), int.Parse(message.Value.MaxFloors))).ToString();
            MaxFloor = message.Value.MaxFloors;
        });
    }
}