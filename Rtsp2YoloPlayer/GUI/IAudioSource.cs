using System;
using Rtsp2YoloPlayer.RawFramesDecoding.DecodedFrames;

namespace Rtsp2YoloPlayer.GUI
{
    interface IAudioSource
    {
        event EventHandler<IDecodedAudioFrame> FrameReceived;
    }
}
