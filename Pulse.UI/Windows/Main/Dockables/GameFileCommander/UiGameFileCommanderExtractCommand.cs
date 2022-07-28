using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderExtractCommand : ICommand
    {
        private readonly Func<UiArchives> _archivesProvider;

        public event EventHandler CanExecuteChanged;

        public UiGameFileCommanderExtractCommand(Func<UiArchives> archivesProvider) => _archivesProvider = Exceptions.CheckArgumentNull(archivesProvider, "archivesProvider");

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            try
            {
                UiArchives archives = _archivesProvider();
                if (archives == null)
                    return;

                UiGameFileCommanderSettingsWindow settingsDlg = new(true);
                if (settingsDlg.ShowDialog() != true)
                    return;

                Stopwatch sw = new();
                sw.Start();

                Wildcard wildcard = new(settingsDlg.Wildcard, false);
                bool? conversion = settingsDlg.Convert;

                FileSystemExtractionTarget target = new();

                foreach (IUiLeafsAccessor accessor in archives.AccessToCheckedLeafs(wildcard, conversion, null))
                    accessor.Extract(target);
                
                sw.Stop();
                if (sw.ElapsedMilliseconds / 1000 > 2)
                    MessageBox.Show(string.Format(Lang.Message.Done.ExtractionCompleteFormat, sw.Elapsed), Lang.Message.Done.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Lang.Message.Error.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}