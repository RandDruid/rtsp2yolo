using System;
using System.Linq;
using RtspClientSharp;
using Rtsp2YoloPlayer.RawFramesReceiving;
using Emgu.CV;
using Emgu.CV.Structure;
using Rtsp2YoloPlayer.RawFramesDecoding.DecodedFrames;
using Rtsp2YoloPlayer.RawFramesDecoding;
using System.Collections.Generic;

namespace Rtsp2YoloPlayer.GUI.Models
{
    class MainWindowModel : IMainWindowModel
    {
        private readonly RealtimeVideoSource _realtimeVideoSource = new RealtimeVideoSource();
        private readonly RealtimeAudioSource _realtimeAudioSource = new RealtimeAudioSource();

        private IRawFramesSource _rawFramesSource;

        public event EventHandler<string> StatusChanged;
        public event EventHandler<List<BboxItem>> ObjectsOut;
        public event EventHandler<Tuple<double, double>> FrameSizeChanged;

        public IVideoSource VideoSource => _realtimeVideoSource;

        private Image<Bgr, byte> _cvBitmap;
        private YoloWrapper _yoloWrapper;
        private DecodedVideoFrameParameters _frameParameters = new DecodedVideoFrameParameters(0, 0, RawFramesDecoding.FFmpeg.FFmpegPixelFormat.None);
        private TransformParameters _transformParameters;

        List<BboxItem> previousGeneration = null;
        List<BboxItem> previousGeneration2 = null;

        public void Start(ConnectionParameters connectionParameters)
        {
            if (_rawFramesSource != null)
                return;

            _yoloWrapper = new YoloWrapper();
            _yoloWrapper.ObjectsOut += YoloWrapper_ObjectsOut;

            _rawFramesSource = new RawFramesSource(connectionParameters);
            _rawFramesSource.ConnectionStatusChanged += ConnectionStatusChanged;

            _realtimeVideoSource.SetRawFramesSource(_rawFramesSource);
            _realtimeAudioSource.SetRawFramesSource(_rawFramesSource);

            _realtimeVideoSource.FrameReceived += OnFrameReceived;

            _rawFramesSource.Start();
        }

        public void Stop()
        {
            _yoloWrapper?.Stop();

            if (_rawFramesSource == null)
                return;

            _rawFramesSource.Stop();
            _realtimeVideoSource.SetRawFramesSource(null);
            _rawFramesSource = null;
        }

        private void ConnectionStatusChanged(object sender, string s)
        {
            StatusChanged?.Invoke(this, s);
        }

        private void OnFrameReceived(object sender, IDecodedVideoFrame decodedFrame)
        {
            if (!decodedFrame.FrameParameters.Equals(_frameParameters))
            {
                _frameParameters = decodedFrame.FrameParameters;
                _transformParameters = new TransformParameters(System.Drawing.RectangleF.Empty,
                    new System.Drawing.Size(_frameParameters.Width, _frameParameters.Height),
                    ScalingPolicy.Stretch, PixelFormat.Bgr24, ScalingQuality.FastBilinear);
                _cvBitmap = new Image<Bgr, byte>(_frameParameters.Width, _frameParameters.Height);
                FrameSizeChanged?.Invoke(this, new Tuple<double, double>(_frameParameters.Width, _frameParameters.Height));
            }
            if (_yoloWrapper.IsYoloReady())
            {
                decodedFrame.TransformTo(_cvBitmap.Mat.DataPointer, _cvBitmap.Mat.Cols * _cvBitmap.Mat.ElementSize, _transformParameters);
                _yoloWrapper.FrameIn(this, _cvBitmap.Mat);
            }
        }

        private void YoloWrapper_ObjectsOut(object source, IEnumerable<BboxItem> objects)
        {
            double maxDistance = 60;
            double maxDistanceSquare = maxDistance * maxDistance;
            List<BboxItem> newGeneration = new List<BboxItem>(objects);
            if (previousGeneration != null)
            {
                foreach (BboxItem newItem in newGeneration)
                {
                    double distance = double.MaxValue;
                    double distanceMyType = double.MaxValue;
                    Guid id = Guid.Empty;
                    Guid idMyType = Guid.Empty;
                    foreach (BboxItem oldItem in previousGeneration)
                    {
                        double curDistance = newItem.DistanceTo2(oldItem);
                        if (distance > curDistance)
                        {
                            distance = curDistance;
                            id = oldItem.Id;
                        }
                        if (distanceMyType > curDistance)
                        {
                            distanceMyType = curDistance;
                            idMyType = oldItem.Id;
                        }
                    }
                    if (distanceMyType < maxDistanceSquare)
                        newItem.SetId(idMyType);
                    else if (distance < maxDistanceSquare)
                        newItem.SetId(id);  // Type change
                    else  // let's give it another chance, go deeper into the past
                    {
                        if (previousGeneration2 != null)
                        {
                            double distance2 = double.MaxValue;
                            double distanceMyType2 = double.MaxValue;
                            Guid id2 = Guid.Empty;
                            Guid idMyType2 = Guid.Empty;
                            foreach (BboxItem oldItem2 in previousGeneration2)
                            {
                                double curDistance2 = newItem.DistanceTo2(oldItem2);
                                if (distance2 > curDistance2)
                                {
                                    distance2 = curDistance2;
                                    id2 = oldItem2.Id;
                                }
                                if (distanceMyType2 > curDistance2)
                                {
                                    distanceMyType2 = curDistance2;
                                    idMyType2 = oldItem2.Id;
                                }
                            }
                            if (distanceMyType2 < maxDistanceSquare)
                                newItem.SetId(idMyType2);
                            else if (distance2 < maxDistanceSquare)
                                newItem.SetId(id2);  // Type change
                        }
                    }
                }
            }
            previousGeneration2 = previousGeneration;
            previousGeneration = newGeneration;
            ObjectsOut(this, newGeneration);
        }

    }
}