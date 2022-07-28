using System.Windows;
using Ookii.Dialogs.Wpf;

namespace Yusnaan.Common;

internal class TaskDialogs
{
    public static bool skip = false;
    public void ShowTaskDialog(string title, string instruction, string content, string path)
    {
        if (TaskDialog.OSSupportsTaskDialogs)
        {
            using TaskDialog dialog = new()
            {
                WindowTitle = title,
                MainInstruction = instruction,
                Content = content
            };
            dialog.FooterIcon = TaskDialogIcon.Information;
            TaskDialogButton yesButton = new(ButtonType.Yes);
            TaskDialogButton noButton = new(ButtonType.No);
            TaskDialogButton skipButton = new(ButtonType.Cancel);
            dialog.Buttons.Add(yesButton);
            dialog.Buttons.Add(noButton);
            dialog.Buttons.Add(skipButton);
            TaskDialogButton button = dialog.ShowDialog();
            if (button == yesButton && !string.IsNullOrEmpty(path))
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
            if (button == skipButton && !string.IsNullOrEmpty(path))
                skip = true;
        }
        else
        {
            MessageBox.Show("This operating system does not support task dialogs.", "Task Dialog Sample");
        }
    }
    public void ShowSkipDialog(string title, string instruction, string content)
    {
        if (TaskDialog.OSSupportsTaskDialogs)
        {
            using TaskDialog dialog = new()
            {
                WindowTitle = title,
                MainInstruction = instruction,
                Content = content
            };
            dialog.FooterIcon = TaskDialogIcon.Information;
            TaskDialogButton yesButton = new(ButtonType.Yes);
            dialog.Buttons.Add(yesButton);
            TaskDialogButton button = dialog.ShowDialog();
        }
        else
        {
            MessageBox.Show("This operating system does not support task dialogs.", "Task Dialog Sample");
        }
    }
    public void ShowWarningDialog(string title, string instruction, string content)
    {
        if (TaskDialog.OSSupportsTaskDialogs)
        {
            using TaskDialog dialog = new()
            {
                WindowTitle = title,
                MainInstruction = instruction,
                Content = content
            };
            dialog.FooterIcon = TaskDialogIcon.Information;
            TaskDialogButton yesButton = new(ButtonType.Yes);
            dialog.Buttons.Add(yesButton);
            TaskDialogButton button = dialog.ShowDialog();
        }
        else
        {
            MessageBox.Show("This operating system does not support task dialogs.", "Task Dialog Sample");
        }
    }
}