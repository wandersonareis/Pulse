﻿using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Pulse.DirectX
{
    public sealed class DepthBuffer : IDisposable
    {
        private readonly Dx11ChainedDevice _device;

        private Texture2D _depthBuffer;
        private DepthStencilView _depthView;

        public Texture2D Buffer => _depthBuffer;
        public DepthStencilView View => _depthView;

        public DepthBuffer(Dx11ChainedDevice device, int width, int height)
        {
            try
            {
                _device = device;
                _depthBuffer = new(_device.Device, new()
                {
                    Format = Format.D32_Float_S8X24_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = width,
                    Height = height,
                    SampleDescription = new(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

                _depthView = new(device.Device, _depthBuffer);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _depthView);
            Utilities.Dispose(ref _depthBuffer);
        }

        public void Clear()
        {
            if (_depthView == null)
                return;

            _device.Device.ImmediateContext.ClearDepthStencilView(_depthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }
    }
}