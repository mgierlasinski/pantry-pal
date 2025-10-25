using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PantryPal.Mobile.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    private int _count;

    public string Text => Count switch
    {
        0 => "Click me",
        1 => $"Clicked {Count} time",
        _ => $"Clicked {Count} times"
    };

    [RelayCommand]
    public void IncrementCount()
    {
        Count++;
        SemanticScreenReader.Announce(Text);
    }
}
