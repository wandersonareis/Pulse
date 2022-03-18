using System;
using NAudio.Wave;
using System.Windows.Forms;

namespace NAudioDemo.AudioPlaybackDemo
{
    class WasapiOutFactory : IOutputAudioDeviceFactory
    {
        WasapiOutSettingsPanel _settingsPanel;

        public IWavePlayer CreateDevice(int latency)
        {
            WasapiOut wasapi = new WasapiOut(
                _settingsPanel.SelectedDevice,
                _settingsPanel.ShareMode,
                _settingsPanel.UseEventCallback,
                latency);
            return wasapi;
        }

        public UserControl CreateSettingsPanel()
        {
            _settingsPanel = new WasapiOutSettingsPanel();
            return _settingsPanel;
        }

        public string Name => "WasapiOut";

        public bool IsAvailable =>
            // supported on Vista and above
            Environment.OSVersion.Version.Major >= 6;

        public int Priority => 3;
    }
}
