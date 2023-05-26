using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Knd.Printer.Common.Controls
{
    public class MyScrollPanel : StackPanel, IScrollInfo, INotifyPropertyChanged
    {
        public Action<Size, Size> ViewPortChanged;

        private double _verticalOffset;

        private TranslateTransform _translate;

        private Size _screenSize;

        private Size _totalSize;

        private const double _lineOffset = 20.0;

        private const double _wheelOffset = 50.0;

        public new bool CanVerticallyScroll { get; set; }

        public new bool CanHorizontallyScroll { get; set; }

        public new double ExtentWidth => _totalSize.Width;

        public new double ExtentHeight => _totalSize.Height;

        public new double ViewportWidth => _screenSize.Width;

        public new double ViewportHeight => _screenSize.Height;

        public new double HorizontalOffset { get; private set; }

        public new double VerticalOffset
        {
            get
            {
                return _verticalOffset;
            }
            private set
            {
                _verticalOffset = value;
                OnPropertyChanged("VerticalOffset");
            }
        }

        public new ScrollViewer ScrollOwner { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MyScrollPanel()
        {
            _translate = new TranslateTransform();
            base.RenderTransform = _translate;
            ViewPortChanged = delegate (Size size, Size size1)
            {
                double verticalOffset = VerticalOffset;
                if (!(size.Height + VerticalOffset <= size1.Height))
                {
                    if (size1.Height - size.Height > 0.0)
                    {
                        SetVertiOffeset(size1.Height - size.Height);
                    }
                    else
                    {
                        SetVertiOffeset(0.0);
                    }
                }
            };
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _screenSize = constraint;
            constraint = ((base.Orientation != 0) ? new Size(double.PositiveInfinity, constraint.Height) : new Size(constraint.Width, double.PositiveInfinity));
            _totalSize = base.MeasureOverride(constraint);
            ViewPortChanged?.Invoke(_screenSize, _totalSize);
            return _totalSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            if (ScrollOwner != null)
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation();
                doubleAnimation1.To = 0.0 - VerticalOffset;
                doubleAnimation1.Duration = TimeSpan.FromSeconds(0.9);
                DoubleAnimation doubleAnimation2 = doubleAnimation1;
                _translate.BeginAnimation(TranslateTransform.YProperty, doubleAnimation2, HandoffBehavior.SnapshotAndReplace);
                DoubleAnimation doubleAnimation3 = new DoubleAnimation();
                doubleAnimation3.To = 0.0 - HorizontalOffset;
                doubleAnimation3.Duration = TimeSpan.FromSeconds(0.9);
                doubleAnimation3.AccelerationRatio = 0.25;
                doubleAnimation3.DecelerationRatio = 0.25;
                DoubleAnimation doubleAnimation4 = doubleAnimation3;
                _translate.BeginAnimation(TranslateTransform.XProperty, doubleAnimation4);
                ScrollOwner.InvalidateScrollInfo();
            }
            ViewPortChanged?.Invoke(_screenSize, _totalSize);
            return _screenSize;
        }

        private void Appendoffset(double x, double y)
        {
            Vector vector = new Vector(HorizontalOffset + x, VerticalOffset + y);
            vector.Y = range(vector.Y, 0.0, _totalSize.Height - _screenSize.Height);
            vector.X = range(vector.X, 0.0, _totalSize.Width - _screenSize.Width);
            HorizontalOffset = vector.X;
            VerticalOffset = vector.Y;
            InvalidateArrange();
        }

        private double range(double value, double value1, double value2)
        {
            if (value2 < 0.0)
            {
                return 0.0;
            }
            double val2_1 = Math.Min(value1, value2);
            double val2_2 = Math.Max(value1, value2);
            value = Math.Max(value, val2_1);
            value = Math.Min(value, val2_2);
            return value;
        }

        public void VerticalChanged(double y)
        {
            Appendoffset(0.0, y);
        }

        public new void LineDown()
        {
            Appendoffset(0.0, 20.0);
        }

        public new void LineLeft()
        {
            Appendoffset(-20.0, 0.0);
        }

        public new void LineRight()
        {
            Appendoffset(20.0, 0.0);
        }

        public new void LineUp()
        {
            Appendoffset(0.0, -20.0);
        }

        public new void MouseWheelDown()
        {
            Appendoffset(0.0, 50.0);
        }

        public new void MouseWheelLeft()
        {
            Appendoffset(-50.0, 0.0);
        }

        public new void MouseWheelRight()
        {
            Appendoffset(50.0, 0.0);
        }

        public new void MouseWheelUp()
        {
            Appendoffset(0.0, -50.0);
        }

        public new void PageDown()
        {
            Appendoffset(0.0, _screenSize.Height);
        }

        public new void PageLeft()
        {
            Appendoffset(0.0 - _screenSize.Width, 0.0);
        }

        public new void PageRight()
        {
            Appendoffset(_screenSize.Width, 0.0);
        }

        public new void PageUp()
        {
            Appendoffset(0.0, 0.0 - _screenSize.Height);
        }

        public new void SetHorizontalOffset(double offset)
        {
            Appendoffset(offset - HorizontalOffset, VerticalOffset);
        }

        public new void SetVerticalOffset(double offset)
        {
            Appendoffset(HorizontalOffset, offset - VerticalOffset);
        }

        public void SetVertiOffeset(double offf)
        {
            VerticalOffset = offf;
            InvalidateArrange();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
