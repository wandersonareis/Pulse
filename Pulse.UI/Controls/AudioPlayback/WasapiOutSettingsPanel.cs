using System.Collections.Generic;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace NAudioDemo.AudioPlaybackDemo
{
    public partial class WasapiOutSettingsPanel : UserControl
    {
        public WasapiOutSettingsPanel()
        {
            InitializeComponent();
            InitialiseWasapiControls();
        }

        class WasapiDeviceComboItem
        {
            public string Description { get; set; }
            public MMDevice Device { get; set; }
        }

        private void InitialiseWasapiControls()
        {
            MMDeviceEnumerator enumerator = new();
            MMDeviceCollection endPoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            List<WasapiDeviceComboItem> comboItems = new();
            foreach (MMDevice endPoint in endPoints)
            {
                WasapiDeviceComboItem comboItem = new()
                {
                    Description = $"{endPoint.FriendlyName} ({endPoint.DeviceFriendlyName})",
                    Device = endPoint
                };
                comboItems.Add(comboItem);
            }
            comboBoxWaspai.DisplayMember = "Description";
            comboBoxWaspai.ValueMember = "Device";
            comboBoxWaspai.DataSource = comboItems;
        }

        public MMDevice SelectedDevice => (MMDevice)comboBoxWaspai.SelectedValue;

        public AudioClientShareMode ShareMode =>
            checkBoxWasapiExclusiveMode.Checked ?
                AudioClientShareMode.Exclusive :
                AudioClientShareMode.Shared;

        public bool UseEventCallback => checkBoxWasapiEventCallback.Checked;
    }
}
