using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Rtsp2YoloPlayer
{
    public class BboxItem : DependencyObject, INotifyPropertyChanged
    {
        #region ViewModel stuff
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool setIfChanged<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (((field != null) && !field.Equals(value)) || ((value != null) && !value.Equals(field)))
            {
                field = value;
                raisePropertyChanged(name);
                return true;
            }
            return false;
        }

        protected bool setIfChanged<T>(ref T field, T value, params string[] names)
        {
            if (((field != null) && !field.Equals(value)) || ((value != null) && !value.Equals(field)))
            {
                field = value;
                raisePropertiesChanged(names);
                return true;
            }
            return false;
        }

        protected void raisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void raisePropertiesChanged(params string[] names)
        {
            foreach (string name in names)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private double x;
        public double X { get { return x; } private set { setIfChanged(ref x, value); } }

        private double y;
        public double Y { get { return y; } private set { setIfChanged(ref y, value); } }

        private double width;
        public double Width { get { return width; } private set { setIfChanged(ref width, value); } }

        private double height;
        public double Height { get { return height; } private set { setIfChanged(ref height, value); } }

        private Brush stroke;
        public Brush Stroke { get { return stroke; } private set { setIfChanged(ref stroke, value); } }

        private Guid id;
        public Guid Id { get { return id; } private set { setIfChanged(ref id, value, "Id", "IdStr"); } }
        public string IdStr { get { return $"{Id.ToString().Substring(0, 6)} {Name}"; } }

        private string name;
        public string Name { get { return name; } private set { setIfChanged(ref name, value, "Name", "IdStr"); } }

        public BboxItem()
        {
            Id = Guid.NewGuid();
        }

        internal BboxItem(BboxT bt) : this()
        {
            X = bt.x;
            Y = bt.y;
            Width = bt.w;
            Height = bt.h;
            Stroke = YoloWrapper.GetObjectColor(bt.obj_id);
            Name = YoloWrapper.GetObjectName(bt.obj_id);
        }

        public void CopyTo(BboxItem dest, double scaleX = 1.0, double scaleY = 1.0)
        {
            dest.X = (uint)(scaleX * X);
            dest.Y = (uint)(scaleY * Y);
            dest.Width = (uint)(scaleX * Width);
            dest.Height = (uint)(scaleY * Height);
            dest.Stroke = Stroke;
            dest.Name = Name;
        }

        public BboxItem ScaleTo(double scaleX = 1.0, double scaleY = 1.0)
        {
            X = (uint)(scaleX * X);
            Y = (uint)(scaleY * Y);
            Width = (uint)(scaleX * Width);
            Height = (uint)(scaleY * Height);
            
            return this;
        }

        public double DistanceTo2(BboxItem b)
        {
            return Math.Pow(X + Width / 2.0 - b.X - b.Width / 2.0, 2) + Math.Pow(Y + Height / 2.0 - b.Y - b.Height / 2.0, 2);
        }

        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}
