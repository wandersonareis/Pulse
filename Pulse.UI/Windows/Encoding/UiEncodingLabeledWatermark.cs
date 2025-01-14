﻿using System.Windows;
using System.Windows.Controls;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingLabeledWatermark : UiGrid
    {
        private readonly UiWatermarkTextBox _textControl;

        public UiEncodingLabeledWatermark(string label, string watermark, int width, TextChangedEventHandler onValueChanged)
        {
            ColumnDefinitions.Add(new() {Width = GridLength.Auto});
            ColumnDefinitions.Add(new());

            Margin = new(5);

            UiTextBlock labelControl = UiTextBlockFactory.Create(label);
            {
                labelControl.Margin = new(5, 5, 2, 5);
                labelControl.VerticalAlignment = VerticalAlignment.Center;
                AddUiElement(labelControl, 0, 0);
            }

            _textControl = UiWatermarkTextBoxFactory.Create(watermark);
            {
                _textControl.Width = width;
                _textControl.Margin = new(2, 5, 5, 5);
                _textControl.TextChanged += onValueChanged;
                AddUiElement(_textControl, 0, 1);
            }
        }

        public string Text
        {
            get => _textControl.Text;
            set => _textControl.Text = value;
        }
    }
}