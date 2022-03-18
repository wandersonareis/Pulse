using System.Drawing;
using System.Windows.Forms;

namespace Pulse.Gui.Common
{
    internal static class Dialogs
    {
        public static string GetFile(string title, string filters = null, string initial = "")
        {
            using OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = initial,
                Filter = filters ?? "Strings files (*.strings)|*.strings",
                RestoreDirectory = true,
                Multiselect = false,
                Title = title
            };

            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return null;
            //Get the path of specified file
            string filePath = openFileDialog.FileName;

            return filePath;
        }
    }
}
