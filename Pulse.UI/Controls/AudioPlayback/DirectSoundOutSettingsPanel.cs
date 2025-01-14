﻿using System;
using System.Windows.Forms;
using NAudio.Wave;

namespace NAudioDemo.AudioPlaybackDemo
{
    public partial class DirectSoundOutSettingsPanel : UserControl
    {
        public DirectSoundOutSettingsPanel()
        {
            InitializeComponent();
            InitialiseDirectSoundControls();
        }

        private void InitialiseDirectSoundControls()
        {
            comboBoxDirectSound.DisplayMember = "Description";
            comboBoxDirectSound.ValueMember = "Guid";
            comboBoxDirectSound.DataSource = DirectSoundOut.Devices;
        }

        public Guid SelectedDevice => (Guid)comboBoxDirectSound.SelectedValue;
    }
}
