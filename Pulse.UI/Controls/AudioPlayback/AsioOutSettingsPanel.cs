using System;
using System.Windows.Forms;
using NAudio.Wave;

namespace NAudioDemo.AudioPlaybackDemo
{
    public partial class AsioOutSettingsPanel : UserControl
    {
        public AsioOutSettingsPanel()
        {
            InitializeComponent();
            InitialiseAsioControls();
        }

        private void InitialiseAsioControls()
        {
            // Just fill the comboBox AsioDriver with available driver names
            string[] asioDriverNames = AsioOut.GetDriverNames();
            foreach (string driverName in asioDriverNames)
            {
                comboBoxAsioDriver.Items.Add(driverName);
            }
            comboBoxAsioDriver.SelectedIndex = 0;
        }

        public string SelectedDeviceName => (string)comboBoxAsioDriver.SelectedItem;

        private void buttonControlPanel_Click(object sender, EventArgs args)
        {
            try
            {
                using (AsioOut asio = new AsioOut(SelectedDeviceName))
                {
                    asio.ShowControlPanel();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
