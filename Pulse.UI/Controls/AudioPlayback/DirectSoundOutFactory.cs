using System.Linq;
using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class DirectSoundOutFactory : IOutputAudioDeviceFactory
    {
        private DirectSoundOutSettingsPanel _settingsPanel;

        public DirectSoundOutFactory() => IsAvailable = DirectSoundOut.Devices.Any();

        public IWavePlayer CreateDevice(int latency) => new DirectSoundOut(_settingsPanel.SelectedDevice, latency);

        public UserControl CreateSettingsPanel()
        {
            _settingsPanel = new();
            return _settingsPanel;
        }

        public string Name => "DirectSound";
        public bool IsAvailable { get; }
        public int Priority => 2;
    }
}
