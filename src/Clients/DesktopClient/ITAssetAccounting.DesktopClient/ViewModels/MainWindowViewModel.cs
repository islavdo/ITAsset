using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ITAssetAccounting.DesktopClient.Models;
using ITAssetAccounting.DesktopClient.Services;

namespace ITAssetAccounting.DesktopClient.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _api;
    private string _email = "admin@it.local";
    private string _password = "Admin123!";
    private string _search = string.Empty;
    private string _status = "Не выполнен вход";

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ViewEquipment> Equipment { get; } = new();
    public ICommand LoginCommand { get; }
    public ICommand LoadCommand { get; }

    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string Search { get => _search; set { _search = value; OnPropertyChanged(); } }
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    public MainWindowViewModel(ApiClient api)
    {
        _api = api;
        LoginCommand = new AsyncCommand(LoginAsync);
        LoadCommand = new AsyncCommand(LoadAsync);
    }

    private async Task LoginAsync()
    {
        Status = await _api.LoginAsync(Email, Password) ? "Вход выполнен" : "Ошибка входа";
        if (_api.AccessToken is not null) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var page = await _api.GetEquipmentAsync(Search);
        Equipment.Clear();
        foreach (var item in page?.Items ?? Array.Empty<ITAssetAccounting.Shared.Dto.EquipmentDto>())
        {
            Equipment.Add(new ViewEquipment(item.Id, item.InventoryNumber, item.Name, item.Model, item.LocationName, item.Status));
        }
        Status = $"Загружено: {Equipment.Count}";
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public AsyncCommand(Func<Task> execute) { _execute = execute; }
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await _execute();
}
