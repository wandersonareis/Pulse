
namespace Pulse.UI
{
    public sealed class GamePathUserProvider : IGamePathProvider
    {
        public void Provide()
        {
            using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Select the Final Fantasy XIII directory..."))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                    GamePath = dlg.FileName;
            }
        }

        public string GamePath { get; private set; }
    }
}