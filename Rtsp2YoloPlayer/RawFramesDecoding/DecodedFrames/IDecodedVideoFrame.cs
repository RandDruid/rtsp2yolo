using System;

namespace Rtsp2YoloPlayer.RawFramesDecoding.DecodedFrames
{
    public interface IDecodedVideoFrame
    {
        DateTime Timestamp { get; }
        DecodedVideoFrameParameters FrameParameters { get; }
        void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParameters);
    }
}