/* 
Application for evaluation of facial symmetry using Microsoft Kinect v2.
Copyright(C) 2017  Sedlák Vojtěch (Vojta.sedlak@gmail.com)

This file is part of FaceSymmetry. 

FaceSymmetry is free software: you can redistribute it and/or modify 
it under the terms of the GNU General Public License as published by 
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version. 

FaceSymmetry is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU General Public License for more details. 

You should have received a copy of the GNU General Public License 
along with Application for evaluation of facial symmetry using Microsoft Kinect v2.. If not, see <http://www.gnu.org/licenses/>.
 */ 

using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Evaluator
{

    public partial class SliderFS : UserControl, INotifyPropertyChanged, IDisposable
    {

        private static Brush[] _colors = {
            new SolidColorBrush(Color.FromArgb(150,255,0,0)),
            new SolidColorBrush(Color.FromArgb(150, 0, 255, 0)),
            new SolidColorBrush(Color.FromArgb(150, 0, 0, 255)),
            new SolidColorBrush(Color.FromArgb(150, 255, 0, 255)),
            new SolidColorBrush(Color.FromArgb(150, 255, 150, 50)),
            new SolidColorBrush(Color.FromArgb(150, 100, 50, 75)),
            new SolidColorBrush(Color.FromArgb(150, 50, 255, 255))
        };

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public static readonly DependencyProperty RecordExercisesProperty =
             DependencyProperty.Register("RecordExercises", typeof(ExerciseDictionary),
             typeof(SliderFS), new FrameworkPropertyMetadata(new ExerciseDictionary()));


        public ExerciseDictionary RecordExercises
        {
            get { return (ExerciseDictionary)GetValue(RecordExercisesProperty); }
            set
            {
                SetValue(RecordExercisesProperty, value);
                NotifyPropertyChanged("RecordExercises");

            }
        }


        public static readonly DependencyProperty MaxTimeProperty =
            DependencyProperty.Register("MaxTime", typeof(double),
            typeof(SliderFS), new FrameworkPropertyMetadata(1d));


        public double MaxTime
        {
            get { return (double)GetValue(MaxTimeProperty); }
            set { SetValue(MaxTimeProperty, value); }
        }


        private Dictionary<ObservableExercise, List<Rectangle>> _exercisesRectangles;
        private Dictionary<ObservableExercise, List<Rectangle>> ExercisesRectangles
        {
            get
            {
                return _exercisesRectangles;
            }
            set
            {
                _exercisesRectangles = value;
                NotifyPropertyChanged("ExercisesRectangles");
            }
        }

        Line _slider = new Line();

        private bool _isSizing = false;
        private bool _isDragging = false;
        private bool _isLeft = false;

        private Rectangle _selectedRectangle;
        private object _lock = new object();


        public Timer Timer { get; private set; }
        private double _value;
        public double TickFrequency { get; private set; }
        public TimeSpan ValueSpan
        {
            get
            {
                return TimeSpan.FromMilliseconds(Value);
            }

        }

        public double Value // time in ms
        {
            get { return _value; }
            set
            {
                if (value < 0) return;

                _value = value;
                NotifyPropertyChanged("Value");
                NotifyPropertyChanged("ValueSpan");
            }
        } // position

        private ObservableExercise _currentExercise;
        public ObservableExercise CurrentExercise
        {
            get { return _currentExercise; }
            set
            {
                _currentExercise = value;
                NotifyPropertyChanged("CurrentExercise");
            }
        }

        public SliderFS()
        {
            InitializeComponent();
            Initialize();
        }

        private void SliderFS_MouseLeave(object sender, MouseEventArgs e)
        {
            popupTextTime.Text = string.Empty;
            floatingTipTime.IsOpen = false;
        }

        public void Initialize()
        {
            canvas.Children.Clear();
            TickFrequency = 100;
            _value = 0;

            _slider.StrokeThickness = 3;
            _slider.Stroke = Brushes.Black;

            canvas.Children.Add(_slider);

            if (Timer != null)
            {
                Timer.Stop();
                Timer.Elapsed -= _timer_Elapsed;
                Timer = null;
            }



            Timer = new Timer(TickFrequency);
            Timer.Elapsed += _timer_Elapsed;

            PropertyChanged += SliderFS_PropertyChanged;
            canvas.MouseDown += SliderFS_MouseDown;
            canvas.MouseUp += SliderFS_MouseUp;
            canvas.MouseMove += SliderFS_MouseMove;
            canvas.MouseLeave += SliderFS_MouseLeave;

        }



        private string CanvasToTimeString(double value)
        {
            var ele = MaxTime / canvas.ActualWidth;
            double ret = (value * ele);
            var time = TimeSpan.FromMilliseconds(ret);

            string timeString = string.Format("{0}:{1:00}.{2:00}", time.Minutes, time.Seconds, time.Milliseconds);

            return timeString;
        }

        private void SliderFS_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(grid);

            if (!floatingTipTime.IsOpen) floatingTipTime.IsOpen = true;

            Point currentPos = e.GetPosition(canvas);
            popupTextTime.Text = CanvasToTimeString(currentPos.X);

            floatingTipTime.HorizontalOffset = currentPos.X + 20;
            floatingTipTime.VerticalOffset = currentPos.Y;

            if (_isDragging && _isSizing)
            {
                if (_isLeft)
                {
                    double right = Canvas.GetLeft(_selectedRectangle) + _selectedRectangle.Width;
                    var left = position.X;

                    var newWidth = right - left;
                    if (newWidth <= 10)
                    {
                        _isDragging = false;
                        _isSizing = false;
                        return;
                    }

                    _selectedRectangle.Width = newWidth;

                    int index = -1;
                    var item = GetExercise(_selectedRectangle, out index);

                    if (item != null && index != -1)
                    {
                        var ele = MaxTime / canvas.ActualWidth;
                        int ticks = (int)(left * ele);
                        RecordExercises[item][index][0] = new TimeSpan(0, 0, 0, 0, ticks);
                    }

                    Canvas.SetLeft(_selectedRectangle, left);

                }
                else
                {
                    double left = Canvas.GetLeft(_selectedRectangle);
                    var right = position.X;

                    var newWidth = right - left;
                    if (newWidth <= 10)
                    {
                        _isDragging = false;
                        _isSizing = false;
                        return;
                    }

                    _selectedRectangle.Width = newWidth;

                    int index = -1;
                    var item = GetExercise(_selectedRectangle, out index);

                    if (item != null && index != -1)
                    {
                        var ele = MaxTime / canvas.ActualWidth;
                        int ticks = (int)(right * ele);
                        RecordExercises[item][index][1] = new TimeSpan(0, 0, 0, 0, ticks);
                    }

                }
            }
        }

        private void SliderFS_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging && !_isSizing)
            {
                var ele = MaxTime / canvas.ActualWidth;
                var position = e.GetPosition(canvas).X * ele;
                Value = position;
            }

            if (_isDragging && _isSizing)
            {
                _isDragging = false;
                _isSizing = false;
                floatingTip.IsOpen = false;
                _isSizing = false;
                Mouse.OverrideCursor = null;
            }
        }

        private void SliderFS_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSizing)
                _isDragging = true;
        }

        private void Refresh()
        {
            Dispatcher.BeginInvoke((Action)(() => { InvalidateVisual(); }));
        }

        public void Stop()
        {
            Timer?.Stop();
            Value = 0;
        }

        public void Start()
        {
            Timer?.Start();
        }

        public void Pause()
        {
            Timer?.Stop();
        }

        public void PauseUnpause()
        {
            if (Timer != null)
            {
                if (Timer.Enabled)
                {
                    Pause();
                }
                else
                {
                    Start();
                }
            }
        }


        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                Value += TickFrequency;

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    var maxTime = MaxTime;

                    if (Value == maxTime || Value > maxTime)
                    {
                        Pause();
                    }
                }));

            }
        }



        private void SliderFS_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RecordExercises":
                    ExercisesRectangles = new Dictionary<ObservableExercise, List<Rectangle>>();
                    foreach (var item in RecordExercises)
                    {
                        _exercisesRectangles.Add(item.Key, new List<Rectangle>());
                    }
                    break;             
                case "Value":
                    Refresh();
                    break;
                default:
                    break;
            }
        }


        public void SetExercisesRectangles()
        {
            canvas.Children.Clear();
            canvas.Children.Add(_slider);

            _exercisesRectangles = new Dictionary<ObservableExercise, List<Rectangle>>();

            foreach (var item in RecordExercises)
            {
                _exercisesRectangles.Add(item.Key, new List<Rectangle>());
            }
        }


        private void Rect_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                floatingTip.IsOpen = false;
                _isSizing = false;
                Mouse.OverrideCursor = null;
                _selectedRectangle = null;
            }
        }


        private ObservableExercise GetExercise(Rectangle rect, out int index)
        {
            index = -1;

            foreach (var item in ExercisesRectangles.Keys)
            {
                index = ExercisesRectangles[item].IndexOf(rect);
                if (index != -1)
                {
                    return item;
                }
            }

            return null;
        }




        private void Rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (!floatingTip.IsOpen) { floatingTip.IsOpen = true; }

            Rectangle rect = sender as Rectangle;
            Point currentPos = e.GetPosition(canvas);

            int index = -1;
            var item = GetExercise(rect, out index);

            if (item != null)
            {
                popupText.Text = item.Name;
            }

            floatingTip.HorizontalOffset = currentPos.X + 20;
            floatingTip.VerticalOffset = currentPos.Y - 20;


            Point pos = e.GetPosition(rect);
            if (pos.X < 2 || pos.X > rect.Width - 2 || _isDragging)
            {
                _isSizing = true;
                _selectedRectangle = rect;
                Mouse.OverrideCursor = Cursors.SizeWE;
                if (pos.X < 2)
                {
                    _isLeft = true;
                }
                else if (pos.X > rect.Width - 2)
                {
                    _isLeft = false;
                }
            }
            else
            {
                _isSizing = false;
                Mouse.OverrideCursor = null;

            }

        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawExercises(drawingContext);
            base.OnRender(drawingContext);
            DrawTicker(drawingContext);
        }
        
        private void DrawTicker(DrawingContext drawingContext)
        {
            var ele = MaxTime / canvas.ActualWidth;

            var position = (Value / ele);

            if (position >= canvas.ActualWidth - 1)
            {
                position = canvas.ActualWidth - 1;
            }

            _slider.X1 = position;
            _slider.X2 = position;
            _slider.Y1 = 0;
            _slider.Y2 = this.canvas.ActualHeight;

            if (ExercisesRectangles != null)
            {

                ObservableExercise exercise = null;

                foreach (var key in ExercisesRectangles.Keys)
                {
                    foreach (var rect in ExercisesRectangles[key])
                    {
                        var x1 = Canvas.GetLeft(rect);
                        var x2 = rect.ActualWidth + x1;

                        if (position >= x1 && position <= x2)
                        {
                            exercise = key;
                            break;
                        }
                    }

                }

                CurrentExercise = exercise;
            }
        }

        public bool FirstDrawOrChange = false;

        private void DrawExercises(DrawingContext drawingContext)
        {


            if (ExercisesRectangles == null || ExercisesRectangles.Count < 1)
            {
                if (RecordExercises != null && RecordExercises.Count > 0)
                {
                    SetExercisesRectangles();
                    FirstDrawOrChange = true;
                }
                else
                    return;
            }


            var ele = MaxTime / canvas.ActualWidth;
            int i = 0;

            if (FirstDrawOrChange)
            {
                foreach (var ex in RecordExercises.Keys)
                {
                    ExercisesRectangles[ex].Clear();

                    foreach (var time in RecordExercises[ex])
                    {
                        var start = time[0].TotalMilliseconds / ele;
                        var end = time[1].TotalMilliseconds / ele;

                        Rectangle rect = new Rectangle();
                        rect.Width = end - start;
                        rect.Height = this.canvas.ActualHeight;
                        rect.Fill = _colors[i];

                        rect.MouseMove += Rect_MouseMove;
                        rect.MouseLeave += Rect_MouseLeave;


                        canvas.Children.Add(rect);
                        Canvas.SetTop(rect, 0);
                        Canvas.SetLeft(rect, start);


                        ExercisesRectangles[ex].Add(rect);
                    }
                    i++;
                    if (i >= _colors.Length) i = 0;
                }

                FirstDrawOrChange = false;
            }
            else
            {
                foreach (var ex in RecordExercises.Keys)
                {
                    int times = 0;
                    foreach (var time in RecordExercises[ex])
                    {
                        var start = time[0].TotalMilliseconds / ele;
                        var end = time[1].TotalMilliseconds / ele;

                        ExercisesRectangles[ex][times].Width = end - start;


                        Canvas.SetTop(ExercisesRectangles[ex][times], 0);
                        Canvas.SetLeft(ExercisesRectangles[ex][times], start);

                        times++;
                    }

                    i++;
                    if (i >= _colors.Length) i = 0;
                }

            }

        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Timer != null)
                    {
                        Timer.Stop();
                        Timer.Dispose();
                        Timer = null;
                    }

                }


                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);

        }
        #endregion

    }
}
