using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class AsioOutFactory : IOutputAudioDeviceFactory
    {
        AsioOutSettingsPanel _settingsPanel;

        public IWavePlayer CreateDevice(int latency) => new AsioOut(_settingsPanel.SelectedDeviceName);

        public UserControl CreateSettingsPanel()
        {
            _settingsPanel = new();
            return _settingsPanel;
        }

        public string Name => "AsioOut";

        public bool IsAvailable => AsioOut.isSupported();

        public int Priority => 4;
    }
}
