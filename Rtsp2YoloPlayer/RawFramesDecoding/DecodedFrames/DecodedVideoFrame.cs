using System;

namespace Rtsp2YoloPlayer.RawFramesDecoding.DecodedFrames
{
    class DecodedVideoFrame : IDecodedVideoFrame
    {
        private readonly Action<IntPtr, int, TransformParameters> _transformAction;

        public DateTime Timestamp { get; }
        public DecodedVideoFrameParameters FrameParameters { get; }

        public DecodedVideoFrame(DateTime timestamp, DecodedVideoFrameParameters decodedVideoFrameParameters, Action<IntPtr, int, TransformParameters> transformAction)
        {
            this.FrameParameters = decodedVideoFrameParameters;
            this.Timestamp = timestamp;
            _transformAction = transformAction;
        }

        public void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParameters)
        {
            _transformAction(buffer, bufferStride, transformParameters);
        }
    }
}