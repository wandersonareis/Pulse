using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class WaveOutFactory : IOutputAudioDeviceFactory
    {
        private WaveOutSettingsPanel _waveOutSettingsPanel;

        public IWavePlayer CreateDevice(int latency)
        {
            WaveCallbackStrategy strategy = _waveOutSettingsPanel.CallbackStrategy;

            if (strategy == WaveCallbackStrategy.NewWindow)
            {
                return new WaveOutWindow
                {
                    DeviceNumber = _waveOutSettingsPanel.SelectedDeviceNumber,
                    DesiredLatency = latency
                };
            }

            return new WaveOut
            {
                DeviceNumber = _waveOutSettingsPanel.SelectedDeviceNumber,
                BufferMilliseconds = latency
            };
        }

        public UserControl CreateSettingsPanel()
        {
            _waveOutSettingsPanel = new();
            return _waveOutSettingsPanel;
        }

        public string Name => "WaveOut";

        public bool IsAvailable => WaveOut.DeviceCount > 0;

        public int Priority => 1;
    }
}
