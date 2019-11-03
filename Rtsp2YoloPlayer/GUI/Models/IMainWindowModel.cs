using System;
using System.Collections.Generic;
using RtspClientSharp;

namespace Rtsp2YoloPlayer.GUI.Models
{
    interface IMainWindowModel
    {
        event EventHandler<string> StatusChanged;
        event EventHandler<List<BboxItem>> ObjectsOut;
        event EventHandler<Tuple<double, double>> FrameSizeChanged;

        IVideoSource VideoSource { get; }

        void Start(ConnectionParameters connectionParameters);
        void Stop();
    }
}