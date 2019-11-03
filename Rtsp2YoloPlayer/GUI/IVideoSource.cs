using System;
using Rtsp2YoloPlayer.RawFramesDecoding.DecodedFrames;

namespace Rtsp2YoloPlayer.GUI
{
    public interface IVideoSource
    {
        event EventHandler<IDecodedVideoFrame> FrameReceived;
    }
}