using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class WaveOutFactory : IOutputAudioDeviceFactory
    {
        private WaveOutSettingsPanel _waveOutSettingsPanel;

        public IWavePlayer CreateDevice(int latency)
        {
            IWavePlayer device;
            WaveCallbackStrategy strategy = _waveOutSettingsPanel.CallbackStrategy;
            if (strategy == WaveCallbackStrategy.Event)
            {
                WaveOutEvent waveOut = new()
                {
                    DeviceNumber = _waveOutSettingsPanel.SelectedDeviceNumber,
                    DesiredLatency = latency
                };
                device = waveOut;
            }
            else
            {
                WaveCallbackInfo callbackInfo = strategy == WaveCallbackStrategy.NewWindow ? WaveCallbackInfo.NewWindow() : WaveCallbackInfo.FunctionCallback();
                WaveOut outputDevice = new(callbackInfo)
                {
                    DeviceNumber = _waveOutSettingsPanel.SelectedDeviceNumber,
                    DesiredLatency = latency
                };
                device = outputDevice;
            }
            // TODO: configurable number of buffers

            return device;
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
