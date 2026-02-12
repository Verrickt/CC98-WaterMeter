using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WaterMeter;

public partial class StatResultViewModel : ObservableObject
{
    public StatResultViewModel(string ubbcode)
    {
        UBBCode = ubbcode;
    }
    public string UBBCode
    {
        get;
        set => SetProperty(ref field, value);
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        Clipboard.SetText(UBBCode);
    }
}