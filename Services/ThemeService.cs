using Microsoft.AspNetCore.Components;

namespace DotNetProjectForAntigravity.Services;

public class ThemeService
{
    public event Action? OnThemeChanged;
    private string _currentTheme = "light";

    public string CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                OnThemeChanged?.Invoke();
            }
        }
    }

    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == "dark" ? "light" : "dark";
    }
}

