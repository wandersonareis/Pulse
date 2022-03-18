using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pulse.UI
{
    public class WrapTextBoxEditor : TypeEditor<TextBox>
    {
        protected override TextBox CreateEditor() => new TextBox {TextWrapping = System.Windows.TextWrapping.WrapWithOverflow};

        protected override void SetValueDependencyProperty() => ValueProperty = TextBox.TextProperty;
    }
}