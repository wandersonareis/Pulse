﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderInjectCommand : ICommand
    {
        private readonly Func<UiArchives> _archivesProvider;

        public event EventHandler CanExecuteChanged;

        public UiGameFileCommanderInjectCommand(Func<UiArchives> archivesProvider)
        {
            _archivesProvider = Exceptions.CheckArgumentNull(archivesProvider, "archivesProvider");
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                UiArchives archives = _archivesProvider();
                if (archives == null)
                    return;

                UiGameFileCommanderSettingsWindow settingsDlg = new(false);
                if (settingsDlg.ShowDialog() != true)
                    return;

                Stopwatch sw = new();
                sw.Start();

                Wildcard wildcard = new(settingsDlg.Wildcard, false);
                bool? compression = settingsDlg.Compression;
                bool? conversion = settingsDlg.Convert;

                UiInjectionManager manager = new();
                FileSystemInjectionSource source = new();
                foreach (IUiLeafsAccessor accessor in archives.AccessToCheckedLeafs(wildcard, conversion, compression))
                    accessor.Inject(source, manager);
                manager.WriteListings();

                if (sw.ElapsedMilliseconds / 1000 > 2)
                    MessageBox.Show(string.Format(Lang.Message.Done.InjectionCompleteFormat, sw.Elapsed), Lang.Message.Done.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Lang.Message.Error.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}