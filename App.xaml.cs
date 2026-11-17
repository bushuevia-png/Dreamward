using System.Windows;

namespace Dreamward;

/// <summary>
/// Точка входа приложения Dreamward (Somnium) с защитой от необработанных сбоев (п. 4.2 ТЗ)
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                $"Перехвачено системное исключение: {args.Exception.Message}\nРабота приложения продолжена.",
                "Dreamward — Модуль надёжности (п. 4.2 ТЗ)",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            args.Handled = true;
        };
    }
}
