using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pfim;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.DirectX
{
    public static class DxTextureReader
    {
        public static DxTexture ReadFromWpd(WpdArchiveListing listing, WpdEntry entry)
        {
            using Stream headers = listing.Accessor.ExtractHeaders();
            using Stream content = listing.Accessor.ExtractContent();
            headers.SetPosition(entry.Offset);

            SectionHeader sectionHeader = headers.ReadContent<SectionHeader>();
            GtexData gtex = sectionHeader.Type switch
            {
                SectionType.Txb => ReadGtexFromTxb(headers),
                SectionType.Vtex => ReadGtexFromVtex(headers),
                _ => throw new NotImplementedException()
            };

            return LoadFromStream(gtex, content);
        }

        public static DxTexture LoadFromStream(GtexData gtex, Stream input)
        {
            if (gtex.Header.LayerCount == 0)
                return null;

            GtexMipMapLocation mipMap = gtex.MipMapData[0];

            DdsHeader header = DdsHeaderDecoder.FromGtexHeader(gtex.Header);
            header.MipMapCount = 1;
            header.Flags &= ~DdsHeaderFlags.MipMapCount;
            header.SurfaceFlags = DdsHeaderSurfaceFlags.Texture;
            header.CubemapFlags = 0;

            byte[] data = new byte[mipMap.Length];
            input.SetPosition(mipMap.Offset);
            input.EnsureRead(data, 0, data.Length);

            using MemoryStream stream = new(DdsHeaderEncoder.HeaderSize + data.Length);
            DdsHeaderEncoder.ToFileStream(header, stream);
            stream.Write(data, 0, data.Length);
            stream.Position = 0;

            using IImage image = Pfimage.FromStream(stream);
            return new DxTexture(CreateBitmap(image), gtex.Header.IsCubeMap);
        }

        private static BitmapSource CreateBitmap(IImage image)
        {
            BitmapSource bitmap;
            switch (image.Format)
            {
                case ImageFormat.Rgba32:
                    // Pfim's Rgba32 image data is laid out as B,G,R,A (the raw
                    // little-endian order of the DDS/DXT source), so it maps
                    // directly to WPF's Bgra32 without any channel swapping.
                    bitmap = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null, image.Data, image.Stride);
                    break;
                case ImageFormat.Rgb24:
                    bitmap = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Rgb24, null, image.Data, image.Stride);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported image format: {image.Format}");
            }

            bitmap.Freeze();
            return bitmap;
        }

        private static GtexData ReadGtexFromTxb(Stream headers)
        {
            TextureHeader textureHeader = headers.ReadContent<TextureHeader>();
            return headers.ReadContent<GtexData>();
        }

        private static GtexData ReadGtexFromVtex(Stream headers)
        {
            VtexHeader textureHeader = headers.ReadContent<VtexHeader>();
            headers.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
            return headers.ReadContent<GtexData>();
        }
    }
}
