using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Pulse.Core;
using Pulse.UI;
using SimpleLogger;
using SimpleLogger.Logging.Handlers;

namespace Yusnaan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public FFXIIIGamePart Result { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        Logger.LoggerHandlerManager
            .AddHandler(new FileLoggerHandler());
    }

    #region PulseButton

    private void OnPart1ButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("Pulse", "-ff13");
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex);
            Environment.Exit(1);
        }
    }
    private void OnPart2ButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("Pulse", "-ff132");
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex);
            Environment.Exit(1);
        }
    }
    private void OnPart3ButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("Pulse", "-ff133");
        }
        catch (Exception ex)
        {
            UiHelper.ShowError(this, ex);
            Environment.Exit(1);
        }
    }

    #endregion

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        Environment.Exit(1);
    }
}