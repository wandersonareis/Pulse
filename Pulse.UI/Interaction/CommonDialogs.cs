using System;
using Ookii.Dialogs.Wpf;

namespace Pulse.UI
{
    internal enum CommonFileDialogResult
    {
        Ok,
        Cancel
    }

    internal sealed class CommonOpenFileDialog : IDisposable
    {
        private readonly string _title;

        public CommonOpenFileDialog(string title) => _title = title;

        public bool IsFolderPicker { get; set; }

        public string FileName { get; private set; }

        public CommonFileDialogResult ShowDialog()
        {
            VistaFolderBrowserDialog dialog = new()
            {
                Description = _title,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = IsFolderPicker
            };

            if (dialog.ShowDialog() != true)
                return CommonFileDialogResult.Cancel;

            FileName = dialog.SelectedPath;
            return CommonFileDialogResult.Ok;
        }

        public void Dispose()
        {
        }
    }

    internal sealed class CommonSaveFileDialog : IDisposable
    {
        private readonly VistaSaveFileDialog _dialog;

        public CommonSaveFileDialog(string title)
        {
            _dialog = new() { Title = title };
        }

        public string DefaultFileName
        {
            set => _dialog.FileName = value;
        }

        public string FileName => _dialog.FileName;

        public CommonFileDialogResult ShowDialog()
        {
            return _dialog.ShowDialog() == true ? CommonFileDialogResult.Ok : CommonFileDialogResult.Cancel;
        }

        public void Dispose()
        {
        }
    }
}
