using System.Windows.Forms;
using NAudio.Wave;

namespace NAudioDemo.AudioPlaybackDemo
{
    public partial class WaveOutSettingsPanel : UserControl
    {
        

        public WaveOutSettingsPanel()
        {
            InitializeComponent();
            InitialiseDeviceCombo();
            InitialiseStrategyCombo();
        }

        class CallbackComboItem
        {
            public CallbackComboItem(string text, WaveCallbackStrategy strategy) => Strategy = strategy;

            public WaveCallbackStrategy Strategy { get; }
        }

        private void InitialiseDeviceCombo()
        {
            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                WaveOutCapabilities capabilities = WaveOut.GetCapabilities(deviceId);
                comboBoxWaveOutDevice.Items.Add($"Device {deviceId} ({capabilities.ProductName})");
            }
            if (comboBoxWaveOutDevice.Items.Count > 0)
            {
                comboBoxWaveOutDevice.SelectedIndex = 0;
            }
        }

        private void InitialiseStrategyCombo()
        {
            comboBoxCallback.DisplayMember = "Text";
            comboBoxCallback.ValueMember = "Strategy";
            comboBoxCallback.Items.Add(new CallbackComboItem("Window", WaveCallbackStrategy.NewWindow));
            comboBoxCallback.Items.Add(new CallbackComboItem("Function", WaveCallbackStrategy.FunctionCallback));
            comboBoxCallback.Items.Add(new CallbackComboItem("Event", WaveCallbackStrategy.Event));
            comboBoxCallback.SelectedIndex = 0;
        }

        public int SelectedDeviceNumber => comboBoxWaveOutDevice.SelectedIndex;

        public WaveCallbackStrategy CallbackStrategy => ((CallbackComboItem)comboBoxCallback.SelectedItem).Strategy;
    }
}
