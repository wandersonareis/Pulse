using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class WorkingLocationUserProvider : IInfoProvider<WorkingLocationInfo>
    {
        public WorkingLocationInfo Provide()
        {
            using (CommonOpenFileDialog dlg = new("Укажите рабочий каталог..."))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                    throw new OperationCanceledException();

                WorkingLocationInfo result = new(dlg.FileName);
                result.Validate();

                return result;
            }
        }

        public string Title => Lang.InfoProvider.WorkingLocation.UserTitle;

        public string Description => Lang.InfoProvider.WorkingLocation.UserDescription;
    }
}