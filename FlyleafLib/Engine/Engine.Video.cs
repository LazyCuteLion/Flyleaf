﻿using System;
using System.Collections.Generic;

using Vortice.DXGI;

using FlyleafLib.MediaFramework.MediaRenderer;

namespace FlyleafLib
{
    public class VideoEngine
    {
        /// <summary>
        /// List of GPU Adpaters <see cref="Config.VideoConfig.GPUAdapteLuid"/>
        /// </summary>
        public Dictionary<long, GPUAdapter>  GPUAdapters { get; internal set; }

        internal IDXGIFactory2 Factory;

        internal VideoEngine()
        {
            if (DXGI.CreateDXGIFactory1(out Factory).Failure)
                throw new InvalidOperationException("Cannot create IDXGIFactory1");

            GPUAdapters = GetAdapters();
        }

        internal Dictionary<long, GPUAdapter> GetAdapters()
        {
            Dictionary<long, GPUAdapter> adapters = new Dictionary<long, GPUAdapter>();

            string dump = "";
            for (int adapterIndex = 0; Factory.EnumAdapters1(adapterIndex, out IDXGIAdapter1 adapter).Success; adapterIndex++)
            {
                dump += $"[#{adapterIndex+1}] {RendererInfo.VendorIdStr(adapter.Description1.VendorId)} {adapter.Description1.Description} (Id: {adapter.Description1.DeviceId} | Luid: {adapter.Description1.Luid}) | DVM: {RendererInfo.GetBytesReadable(adapter.Description1.DedicatedVideoMemory)}\r\n";

                if ((adapter.Description1.Flags & AdapterFlags.Software) != AdapterFlags.None)
                {
                    adapter.Dispose();
                    continue;
                }

                int idx = 0;
                List<GPUOutput> outputs = new List<GPUOutput>();
                while (adapter.EnumOutputs(idx, out IDXGIOutput output).Success)
                {
                    GPUOutput gpout = new GPUOutput();

                    gpout.DeviceName= output.Description.DeviceName;
                    gpout.Left      = output.Description.DesktopCoordinates.Left;
                    gpout.Top       = output.Description.DesktopCoordinates.Top;
                    gpout.Right     = output.Description.DesktopCoordinates.Right;
                    gpout.Bottom    = output.Description.DesktopCoordinates.Bottom;
                    gpout.IsAttached= output.Description.AttachedToDesktop;
                    gpout.Rotation  = (int)output.Description.Rotation;

                    outputs.Add(gpout);
                    output.Dispose();

                    idx++;
                }

                adapters[adapter.Description1.Luid] = new GPUAdapter() { Description = adapter.Description1.Description, Luid = adapter.Description1.Luid, Outputs = outputs };

                adapter.Dispose();
            }

            Engine.Log.Info($"GPU Adapters\r\n{dump}");

            return adapters;
        }
    }
}
