using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Pulse.Core;
using Pulse.FS;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pulse.UI
{
    public sealed class UiGameFilePreviewYkd : UiGrid
    {
        private readonly Tree _treeView;
        private readonly PropertyGrid _propertyGrid;
        private UiButton _rollbackButton;
        private UiButton _injectButton;
        private UiButton _extractButton;

        public UiGameFilePreviewYkd()
        {
            #region Constructor

            _treeView = new Tree();
            _treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;

            _propertyGrid = new PropertyGrid {AutoGenerateProperties = false};
            
            _rollbackButton = UiButtonFactory.Create("��������");
            _rollbackButton.Width = 200;
            _rollbackButton.Margin = new Thickness(5);
            _rollbackButton.HorizontalAlignment = HorizontalAlignment.Left;
            _rollbackButton.VerticalAlignment = VerticalAlignment.Center;
            _rollbackButton.Click += OnRolbackButtonClick;

            _injectButton = UiButtonFactory.Create("��������");
            _injectButton.Width = 200;
            _injectButton.Margin = new Thickness(5, 5, 210, 5);
            _injectButton.HorizontalAlignment = HorizontalAlignment.Right;
            _injectButton.VerticalAlignment = VerticalAlignment.Center;
            _injectButton.Click += OnInjectButtonClick;

            _extractButton = UiButtonFactory.Create("�������");
            _extractButton.Width = 200;
            _extractButton.Margin = new Thickness(5);
            _extractButton.HorizontalAlignment = HorizontalAlignment.Right;
            _extractButton.VerticalAlignment = VerticalAlignment.Center;
            _extractButton.Click += OnExtractButtonClick;

            SetCols(2);
            SetRows(2);
            RowDefinitions[1].Height = new GridLength();
            AddUiElement(_treeView, 0, 0);
            AddUiElement(_propertyGrid, 0, 1);
            AddUiElement(_rollbackButton, 1, 0, 0, 2);
            AddUiElement(_injectButton, 1, 0, 0, 2);
            AddUiElement(_extractButton, 1, 0, 0, 2);

            #endregion
        }

        private WpdArchiveListing _listing;
        private WpdEntry _entry;

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            _listing = listing;
            _entry = entry;

            if (listing == null || entry == null)
            {
                _treeView.ItemsSource = null;
                return;
            }
            
            using (Stream headers = listing.Accessor.ExtractHeaders())
                _treeView.ItemsSource = new[] {new YkdFileView(new StreamSegment(headers, entry.Offset, entry.Length, FileAccess.Read).ReadContent<YkdFile>())};
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _propertyGrid.SelectedObject = _treeView.SelectedItem as View;
        }

        private void OnRolbackButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IsEnabled = false;
                Show(_listing, _entry);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void OnInjectButtonClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnExtractButtonClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        

        // =======================================================================

        private sealed class Tree : UiTreeView
        {
            public Tree()
            {
                ItemTemplateSelector = new TemplateSelector();
            }

            private sealed class TemplateSelector : DataTemplateSelector
            {
                public override DataTemplate SelectTemplate(object item, DependencyObject container)
                {
                    View view = item as View;
                    return view == null ? null : view.TreeViewTemplate;
                }
            }
        }

        private abstract class View
        {
            public abstract DataTemplate TreeViewTemplate { get; }

            // Binding
            public IEnumerable<View> BindableChilds
            {
                get { return EnumerateChilds(); }
            }

            protected abstract IEnumerable<View> EnumerateChilds();
        }

        private abstract class View<TView, TNative> : View
        {
            protected readonly TNative Native;

            protected View(TNative native)
            {
                Native = native;
            }

            // Binding
            public virtual String Title
            {
                get { return TypeCache<TNative>.Type.Name; }
            }

            public override DataTemplate TreeViewTemplate
            {
                get { return LazyTreeViewTemplate.Value; }
            }

            private static readonly Lazy<DataTemplate> LazyTreeViewTemplate = new Lazy<DataTemplate>(CreateTemplate);

            private static DataTemplate CreateTemplate()
            {
                HierarchicalDataTemplate template = new HierarchicalDataTemplate
                {
                    DataType = TypeCache<TView>.Type,
                    ItemsSource = new Binding("BindableChilds")
                };

                FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
                textBlock.SetBinding(TextBlock.TextProperty, new Binding("Title"));

                template.VisualTree = textBlock;
                return template;
            }
        }

        private sealed class YkdFileView : View<YkdFileView, YkdFile>
        {
            public YkdFileView(YkdFile native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield return new YkdBlockView(Native.Background);
                foreach (YkdBlock block in Native.Blocks)
                    yield return new YkdBlockView(block);
                yield return new YkdResourcesView(Native.Resources);
            }

            [Category("���������")]
            [DisplayName("���������� 1")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HeaderUnknown1
            {
                get { return Native.Header.Unknown1; }
                set { Native.Header.Unknown1 = value; }
            }

            [Category("���������")]
            [DisplayName("���������� 2")]
            [Description("����������� ��������.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown2
            {
                get { return Native.Header.Unknown2; }
                set { Native.Header.Unknown2 = value; }
            }

            [Category("���������")]
            [DisplayName("���������� 3")]
            [Description("����������� ��������.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown3
            {
                get { return Native.Header.Unknown3; }
                set { Native.Header.Unknown3 = value; }
            }

            [Category("���������")]
            [DisplayName("���������� 4")]
            [Description("����������� ��������.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown4
            {
                get { return Native.Header.Unknown4; }
                set { Native.Header.Unknown4 = value; }
            }

            [Category("���������")]
            [DisplayName("���������� 5")]
            [Description("����������� ��������.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown5
            {
                get { return Native.Header.Unknown5; }
                set { Native.Header.Unknown5 = value; }
            }
        }

        private sealed class YkdBlockView : View<YkdBlockView, YkdBlock>
        {
            public YkdBlockView(YkdBlock native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdBlockEntry entry in Native.Entries)
                    yield return new YkdBlockEntryView(entry);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Entries.Length); }
            }
        }

        private sealed class YkdBlockEntryView : View<YkdBlockEntryView, YkdBlockEntry>
        {
            public YkdBlockEntryView(YkdBlockEntry native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdFrames frame in Native.Frames)
                    yield return new YkdFramesView(frame);
            }

            public override string Title
            {
                get { return String.IsNullOrEmpty(Native.Name) ? "<empty>" : Native.Name; }
            }

            [Category("��������")]
            [DisplayName("���")]
            [Description("��� �������. ��������, �� ���� ��������� ��������.")]
            [Editor(typeof(TextBoxEditor), typeof(TextBoxEditor))]
            public String Name
            {
                get { return Native.Name; }
                set { Native.Name = value; }
            }

            [Category("�������")]
            [DisplayName("���������� 1")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown1
            {
                get { return Native.Offsets.Unknown1; }
                set { Native.Offsets.Unknown1 = value; }
            }

            [Category("�������")]
            [DisplayName("���������� 2")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown2
            {
                get { return Native.Offsets.Unknown2; }
                set { Native.Offsets.Unknown2 = value; }
            }

            [Category("�������")]
            [DisplayName("��������?")]
            [Description("�����-�� ������� ������ �� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown3
            {
                get { return Native.Offsets.Unknown3; }
                set { Native.Offsets.Unknown3 = value; }
            }
        }

        private sealed class YkdFramesView : View<YkdFramesView, YkdFrames>
        {
            public YkdFramesView(YkdFrames native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdFrame frame in Native.Frames)
                    yield return new YkdFrameView(frame);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Count); }
            }

            [Category("���������")]
            [DisplayName("���������� 1")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HUnknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("���������")]
            [DisplayName("���������� 2")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HUnknown2
            {
                get { return Native.Unknown2; }
                set { Native.Unknown2 = value; }
            }

            [Category("���������")]
            [DisplayName("���������� 3")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HUnknown3
            {
                get { return Native.Unknown3; }
                set { Native.Unknown3 = value; }
            }
        }

        private sealed class YkdFrameView : View<YkdFrameView, YkdFrame>
        {
            public YkdFrameView(YkdFrame native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            public override string Title
            {
                get { return String.Format("{0} (X: {1}, Y:{2})", base.Title, Native.X, Native.Y); }
            }

            [Category("������������")]
            [DisplayName("X")]
            [Description("���������� �� ��� OX.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float X
            {
                get { return Native.X; }
                set { Native.X = value; }
            }

            [Category("������������")]
            [DisplayName("Y")]
            [Description("���������� �� ��� OY.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Y
            {
                get { return Native.Y; }
                set { Native.Y = value; }
            }

            [Category("����������")]
            [DisplayName("���������� 1")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("����������")]
            [DisplayName("���������� 2")]
            [Description("����������� ��������.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Unknown2
            {
                get { return Native.Unknown2; }
                set { Native.Unknown2 = value; }
            }

            [Category("����������")]
            [DisplayName("���������� 3")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown3
            {
                get { return Native.Unknown3; }
                set { Native.Unknown3 = value; }
            }

            [Category("����������")]
            [DisplayName("���������� 4")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown4
            {
                get { return Native.Unknown4; }
                set { Native.Unknown4 = value; }
            }

            [Category("����������")]
            [DisplayName("���������� 5")]
            [Description("����������� ��������.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Unknown5
            {
                get { return Native.Unknown5; }
                set { Native.Unknown5 = value; }
            }

            [Category("����������")]
            [DisplayName("���������� 6")]
            [Description("����������� ��������.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Unknown6
            {
                get { return Native.Unknown6; }
                set { Native.Unknown6 = value; }
            }
        }

        private sealed class YkdResourcesView : View<YkdResourcesView, YkdResources>
        {
            public YkdResourcesView(YkdResources native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdResource resource in Native.Resources)
                    yield return new YkdResourceView(resource);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Count); }
            }

            [Category("�������")]
            [DisplayName("���������� 1")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown1
            {
                get { return Native.Offsets.Unknown1; }
                set { Native.Offsets.Unknown1 = value; }
            }

            [Category("�������")]
            [DisplayName("���������� 2")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown2
            {
                get { return Native.Offsets.Unknown2; }
                set { Native.Offsets.Unknown2 = value; }
            }

            [Category("�������")]
            [DisplayName("��������?")]
            [Description("�����-�� ������� ������ �� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown3
            {
                get { return Native.Offsets.Unknown3; }
                set { Native.Offsets.Unknown3 = value; }
            }
        }

        private sealed class YkdResourceView : View<YkdResourceView, YkdResource>
        {
            public YkdResourceView(YkdResource native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            public override string Title
            {
                get { return String.Format("{0:D3} {1} (Size: {2})", Native.Index, String.IsNullOrEmpty(Native.Name) ? "<empty>" : Native.Name, Native.Size); }
            }

            [Category("��������")]
            [DisplayName("��������")]
            [Description("�������� ����� �������� � ����������� .txbh �� ����� �� ������.")]
            [Editor(typeof(TextBoxEditor), typeof(TextBoxEditor))]
            public String Name
            {
                get { return Native.Name; }
                set { Native.Name = value; }
            }

            [Category("�����������")]
            [DisplayName("���������� 1")]
            [Description("����������� ��������.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }
        }
    }
}