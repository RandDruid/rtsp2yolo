using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace Rtsp2YoloPlayer
{
    internal struct BboxT
    {
        internal uint x, y, w, h;         // (x,y) - top-left corner, (w, h) - width & height of bounded box
        internal float prob;              // confidence - probability that the object was found correctly
        internal uint obj_id;             // class of object - from range [0, classes-1]
        internal uint track_id;           // tracking id for video (0 - untracked, 1 - inf - tracked object)
        internal uint frames_counter;
        internal float x_3d, y_3d, z_3d;  // 3-D coordinates, if there is used 3D-stereo camera
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct BboxContainer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
        internal BboxT[] candidates;
    }

    class YoloWrapper : IDisposable
    {
        private const string YoloLibraryGpu = @"x64\yolo_cpp_dll.dll";

        [DllImport(YoloLibraryGpu, EntryPoint = "detect_mat_raw")]
        internal static extern int DetectImageGpu(int rows, int cols, int type, IntPtr data, ref BboxContainer container);

        [DllImport(YoloLibraryGpu, EntryPoint = "init")]
        internal static extern int InitializeYoloGpu(string configurationFilename, string weightsFilename, int gpu);

        [DllImport(YoloLibraryGpu, EntryPoint = "dispose")]
        internal static extern int DisposeYoloGpu();

        [DllImport(YoloLibraryGpu, EntryPoint = "get_device_count")]
        internal static extern int GetDeviceCount();

        [DllImport(YoloLibraryGpu, EntryPoint = "get_device_name")]
        internal static extern int GetDeviceName(int gpu, StringBuilder deviceName);

        private Mat _frame;
        private static readonly Dictionary<uint, string> _objectNames = new Dictionary<uint, string>();
        private Thread _thread;
        private ThreadStart _threadStart;
        private ManualResetEvent _gotNewFrameEvent = new ManualResetEvent(false);
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private ManualResetEvent _readyEvent = new ManualResetEvent(false);

        private int _statFrameCounter = 0;
        private int _statFramesTotal = 0;
        private DateTime _statPrevCall = DateTime.Now;

        public event Action<object, IEnumerable<BboxItem>> ObjectsOut;

        public YoloWrapper()
        {
            int gpu = 0;

            int deviceCount = GetDeviceCount();

            var deviceName = new StringBuilder(); // allocate memory for string
            GetDeviceName(gpu, deviceName);

            string configurationFilename = @"Configs/yolov3/yolov3.cfg";
            string weightsFilename = @"Configs/yolov3/yolov3.weights";
            string namesFilename = @"Configs/yolov3/coco.names";

            int r = InitializeYoloGpu(configurationFilename, weightsFilename, gpu);

            string[] names = File.ReadAllLines(namesFilename);
            _objectNames.Clear();
            for (uint i = 0; i < names.Length; i++)
            {
                _objectNames.Add(i, names[i]);
            }

            _threadStart = new ThreadStart(ProcessFrames);
            _thread = new Thread(_threadStart);
            _thread.Priority = ThreadPriority.Highest;
            // _thread.IsBackground = true;
            _thread.Start();
            _readyEvent.Set();
        }

        public void Stop()
        {
            _stopEvent.Set();
            if (!_thread.Join(100))
            {
                _thread.Abort();
            }
        }

        public static string GetObjectName(uint objectId)
        {
            if (!_objectNames.TryGetValue(objectId, out var objectName))
            {
                return "unknown object";
            }

            return objectName;
        }

        public static Brush GetObjectColor(uint objectId)
        {
            string name = GetObjectName(objectId);
            switch (name)
            {
                case "car": return Brushes.Red;
                case "person": return Brushes.Green;
                case "bus": return Brushes.Blue;
                case "truck": return Brushes.Brown;
                default: return Brushes.Black;
            }
        }

        public bool IsYoloReady()
        {
            _statFramesTotal++;
            return _readyEvent.WaitOne(0);
        }

        public void FrameIn(object sender, Mat frame)
        {
            _frame = frame;
            _gotNewFrameEvent.Set();
        }

        private void ProcessFrames()
        {
            WaitHandle[] exitOrRetry = new WaitHandle[] { _stopEvent, _gotNewFrameEvent };

            while (!_stopEvent.WaitOne(0))
            {
                if (_gotNewFrameEvent.WaitOne(0))
                {
                    _readyEvent.Reset();
                    _gotNewFrameEvent.Reset();

                    BboxContainer container = new BboxContainer();
                    int CV_8UC3 = 16;  // OpenCV constant
                    int count = DetectImageGpu(_frame.Rows, _frame.Cols, CV_8UC3, _frame.DataPointer, ref container);
                    _statFrameCounter++;

                    if (_statFrameCounter > 24)
                    {
                        DateTime now = DateTime.Now;
                        double fps = _statFrameCounter / now.Subtract(_statPrevCall).TotalSeconds;
                        Debug.WriteLine($"FPS: {fps:F1} skipped: {100.0 * _statFrameCounter / _statFramesTotal:F0}%");
                        _statFrameCounter = 0;
                        _statFramesTotal = 0;
                        _statPrevCall = now;
                    }

                    ObjectsOut?.Invoke(this, container.candidates.Where(b => (b.w > 0) && (b.h > 0)).Select(bt => new BboxItem(bt)));

                    _readyEvent.Set();
                }
                else
                {
                    WaitHandle.WaitAny(exitOrRetry, 100);
                }
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                DisposeYoloGpu();

                disposedValue = true;
            }
        }

        ~YoloWrapper()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
