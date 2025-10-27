using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace PantryPal.Mobile.Models;

public partial class PantryItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isFavorite;

    public ICommand? ToggleFavoriteCommand { get; set; }
    public ICommand? DeleteItemCommand { get; set; }
    public ICommand? EditItemCommand { get; set; }
}

