using System.Collections.ObjectModel;
using PasswordGenerator.Services;

namespace PasswordGenerator;

public partial class MainPage : ContentPage
{
    // Список последних паролей (хранение в памяти)
    private readonly ObservableCollection<string> _history = new();
    private string _currentPassword = string.Empty;

    public MainPage()
    {
        InitializeComponent();
        HistoryList.ItemsSource = _history;
    }

    // Двигаю ползунок длины показывает число
    private void OnLengthChanged(object sender, ValueChangedEventArgs e)
    {
        int len = (int)Math.Round(e.NewValue);
        LengthLabel.Text = len.ToString();
    }

    // Генерация пароля
    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        var options = new PasswordOptions
        {
            Length = (int)LengthSlider.Value,
            IncludeUppercase = UpperCheck.IsChecked,
            IncludeLowercase = LowerCheck.IsChecked,
            IncludeDigits = DigitCheck.IsChecked,
            IncludeSymbols = SymbolCheck.IsChecked,
            ExcludeAmbiguous = AmbiguousCheck.IsChecked
        };

        // Хотя бы один тип символов должен быть выбран
        if (!options.IncludeUppercase && !options.IncludeLowercase
            && !options.IncludeDigits && !options.IncludeSymbols)
        {
            await DisplayAlert("Внимание", "Выбери хотя бы один тип символов.", "Ок");
            return;
        }

        string password = PasswordService.Generate(options);
        if (string.IsNullOrEmpty(password)) return;

        _currentPassword = password;
        PasswordLabel.Text = password;
        UpdateStrength(PasswordService.CalculateStrength(password));

        // Добавляем в историю, храним максимум 5 паролей
        _history.Insert(0, password);
        while (_history.Count > 5) _history.RemoveAt(_history.Count - 1);
        HistorySection.IsVisible = true;
    }

    // Копирование пароля в буфер обмена
    private async void OnCopyClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPassword)) return;

        await Clipboard.SetTextAsync(_currentPassword);

        var btn = (Button)sender;
        btn.Text = "Скопировано";
        await Task.Delay(1200);
        btn.Text = "Копировать";
    }

    // Очистка истории
    private void OnClearHistory(object sender, TappedEventArgs e)
    {
        _history.Clear();
        HistorySection.IsVisible = false;
    }

    // Заполнение полосы надёжности и подписи
    private void UpdateStrength(PasswordStrength strength)
    {
        (string label, double progress) = strength switch
        {
            PasswordStrength.VeryWeak => ("Очень слабый", 0.20),
            PasswordStrength.Weak => ("Слабый", 0.40),
            PasswordStrength.Medium => ("Средний", 0.60),
            PasswordStrength.Strong => ("Сильный", 0.80),
            PasswordStrength.VeryStrong => ("Очень сложный", 1.00),
            _ => ("—", 0.0)
        };

        StrengthBar.Progress = progress;
        StrengthLabel.Text = label;
    }
}

