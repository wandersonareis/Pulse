using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using ShapePath = System.Windows.Shapes.Path;

namespace Pulse.UI
{
    public static class Icons
    {
        public static DrawingImage OkIcon => LazyOkIcon.Value;

        public static DrawingImage CrossIcon => LazyCrossIcon.Value;

        public static DrawingImage PendingIcon => LazyPendingIcon.Value;

        public static DrawingImage PlayIcon => LazyPlayIcon.Value;

        public static DrawingImage PauseIcon => LazyPauseIcon.Value;

        public static DrawingImage StopIcon => LazyStopIcon.Value;

        public static DrawingImage EnabledMusicIcon => LazyEnabledMusicIcon.Value;

        public static DrawingImage DisabledMusicIcon => LazyDisabledMusicIcon.Value;

        public static DrawingImage EnabledSwitchIcon => LazyEnabledSwitchIcon.Value;

        public static DrawingImage DisabledSwitchIcon => LazyDisabledSwitchIcon.Value;

        public static DrawingImage PackageIcon => LazyPackageIcon.Value;

        public static BitmapSource DiskIcon => LazyDiskIcon.Value;

        public static BitmapSource FolderIcon => LazyFolderIcon.Value;

        public static BitmapSource TxtFileIcon => LazyTxtFileIcon.Value;

        private static readonly Lazy<DrawingImage> LazyOkIcon = new(CreateGreenOkIcon);
        private static readonly Lazy<DrawingImage> LazyCrossIcon = new(CreateRedCrossIcon);
        private static readonly Lazy<DrawingImage> LazyPendingIcon = new(CreatePendingIcon);

        private static readonly Lazy<DrawingImage> LazyPlayIcon = new(CreatePlayIcon);
        private static readonly Lazy<DrawingImage> LazyPauseIcon = new(CreatePauseIcon);
        private static readonly Lazy<DrawingImage> LazyStopIcon = new(CreateStopIcon);
        
        private static readonly Lazy<DrawingImage> LazyEnabledMusicIcon = new(CreateEnabledMusicIcon);
        private static readonly Lazy<DrawingImage> LazyDisabledMusicIcon = new(CreateDisabledMusicIcon);
        private static readonly Lazy<DrawingImage> LazyEnabledSwitchIcon = new(CreateEnabledSwitchIcon);
        private static readonly Lazy<DrawingImage> LazyDisabledSwitchIcon = new(CreateDisabledSwitchIcon);

        private static readonly Lazy<DrawingImage> LazyPackageIcon = new(CreatePackageIcon);
        private static readonly Lazy<BitmapSource> LazyDiskIcon = new(CreateDiskIcon);
        private static readonly Lazy<BitmapSource> LazyFolderIcon = new(CreateDirectoryIcon);
        private static readonly Lazy<BitmapSource> LazyTxtFileIcon = new(() => CreateFileIcon(".txt"));

        private static DrawingImage CreateGreenOkIcon()
        {
            Pen pen = new(Brushes.DarkGreen, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("CheckmarkIconGeometry");
            GeometryDrawing drawning = new(Brushes.ForestGreen, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateRedCrossIcon()
        {
            Pen pen = new(Brushes.DarkRed, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("CrossIconGeometry");
            GeometryDrawing drawning = new(Brushes.Red, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePendingIcon()
        {
            Pen pen = new(Brushes.DimGray, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("ClockIconGeometry");
            GeometryDrawing drawning = new(Brushes.DarkGray, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePlayIcon()
        {
            Pen pen = new(Brushes.Green, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("PlayIconGeometry");
            GeometryDrawing drawning = new(Brushes.LimeGreen, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePauseIcon()
        {
            Pen pen = new(Brushes.Black, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("PauseIconGeometry");
            GeometryDrawing drawning = new(Brushes.OrangeRed, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateStopIcon()
        {
            Pen pen = new(Brushes.Black, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("StopIconGeometry");
            GeometryDrawing drawning = new(Brushes.Black, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateEnabledMusicIcon()
        {
            Pen pen = new(Brushes.Black, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("MusicIconGeometry");
            GeometryDrawing drawning = new(Brushes.Black, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateDisabledMusicIcon()
        {
            Pen pen = new(Brushes.LightGray, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("MusicIconGeometry");
            GeometryDrawing drawning = new(Brushes.LightGray, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateEnabledSwitchIcon()
        {
            Pen pen = new(Brushes.Black, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("SwitchIconGeometry");
            GeometryDrawing drawning = new(Brushes.Black, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateDisabledSwitchIcon()
        {
            Pen pen = new(Brushes.LightGray, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("SwitchIconGeometry");
            GeometryDrawing drawning = new(Brushes.LightGray, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePackageIcon()
        {
            Pen pen = new(Brushes.Black, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("PackageIconGeometry");
            GeometryDrawing drawning = new(Brushes.Blue, pen, geometry);
            DrawingImage imageSource = new(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static BitmapSource CreateDiskIcon()
        {
            string disk = Path.GetPathRoot(Path.GetTempPath());
            return ShellHelper.ExtractAssociatedIcon(disk, false);
        }

        private static BitmapSource CreateDirectoryIcon()
        {
            string folder = Path.GetTempPath();
            return ShellHelper.ExtractAssociatedIcon(folder, false);
        }

        private static BitmapSource CreateFileIcon(string extension)
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
            File.Create(path).Close();
            try
            {
                return ShellHelper.ExtractAssociatedIcon(path, false);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}