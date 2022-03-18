using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Pulse.Gui
{
	public class Utils
	{
		public void RunLs(string args)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = Application.StartupPath + "\\ls.exe";
			processStartInfo.ErrorDialog = false;
			processStartInfo.Arguments = " " + args;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
			using (Process process = Process.Start(processStartInfo))
			{
				string value = process.StandardOutput.ReadToEnd();
				StreamWriter streamWriter = new StreamWriter(Application.StartupPath + "\\ls.txt");
				streamWriter.WriteLine(value);
				streamWriter.Close();
				process.WaitForExit();
			}
		}

		public void runFfxiiiCrypt(string args)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = Application.StartupPath + "\\ffxiiicrypt.exe";
			processStartInfo.Arguments = " " + args;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			using (Process process = Process.Start(processStartInfo))
			{
				process.WaitForExit();
			}
		}
	}
}
