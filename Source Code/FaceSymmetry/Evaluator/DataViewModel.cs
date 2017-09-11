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
using Evaluator.Interpolations;

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Evaluator
{
    public partial class DataViewModel : ObservableObject, IDisposable, IViewModel
    {
        public object Content { get; set; }

        private DataView _dataView;
        private List<Ellipse> _pointsProfile = new List<Ellipse>();
        private List<Ellipse> _pointsFront = new List<Ellipse>();
        private MessageBoxFS _messageBox;
        private object _lock = new object();
        private double _actualValue = 0;
        private readonly int _centerIndex = 17;
        private readonly int _centerForeheadIndex = 29;
        private readonly int _rightSideIndex = 697;
        private readonly SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush blue = new SolidColorBrush(Colors.Blue);
        private readonly SolidColorBrush green = new SolidColorBrush(Colors.Green);
        private Timer _resizeTimer = new Timer(100) { Enabled = false };


        public ObservableCollection<string> ExistingInterpolations { get; set; }
        public Examination Examination;
        public Record Record;
        public string Name { get { return this.GetType().Name; } }
        public List<string> ActiveExercises { get; private set; }
        public TimeSpan MaxTimeSpan { get { return TimeSpan.FromMilliseconds(MaxTime); } }


        private int _selectedTab = 0;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                OnPropertyChanged("SelectedTab");
            }
        }

        public double MaxTime { get; private set; }
        public ExerciseDictionary RecordExercises { get; private set; }
        public DateTime LastFrame { get; private set; }
        public int FrameCount { get; private set; }
        public bool IsTimerEnabled { get { return _dataView.slider.Timer.Enabled; } }



        public DataViewModel(Examination selectedExamination)
        {

            _messageBox = new MessageBoxFS("Opening", "Opening record");
            Content = _dataView = new DataView();

            Task init = Task.Factory.StartNew(() => PreInitialize(selectedExamination));

            _messageBox.ShowDialog();
            _messageBox = null;

            _dataView.DataContext = _dataView.slider.DataContext = this;

            _dataView.slider.PropertyChanged += Slider_PropertyChanged;

            if (selectedExamination.RecordLocation == RecordLocation.Local)
            {
                Initialize();
            }

            RecordExercises.KeyChanged += RecordExercises_KeyChanged;
        }

        internal void SliderAddValue(int timeMs)
        {
            _dataView.slider.Value += timeMs;
        }

        private void RecordExercises_KeyChanged(object sender, PropertyChangedEventArgs e)
        {
            _dataView.slider.SetExercisesRectangles();
        }

        private void PreInitialize(Examination selectedExamination)
        {
            _resizeTimer.Elapsed += new ElapsedEventHandler(ResizingDone);
            PropertyChanged += DataViewModel_PropertyChanged;

            if (selectedExamination.RecordLocation == RecordLocation.Local)
            {
                SetRecord(selectedExamination);
            }

            _messageBox.UpdateText("Done");
            Examination = selectedExamination;

            var collection = DBAdapter.GetExercisesFromDB().ToList(); ;
            var list = collection.Where(x => x.Active == true).Select(x => x.Name).ToList();
            list.Sort();
            ActiveExercises = list;
            _messageBox.Dispatcher.BeginInvoke(new Action(() => _messageBox.Close(true)));
        }

        private void Initialize()
        {
            ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();
            int beginFrame = 0;
            int frames = 1;

            Record.ReadTransformData(ref list, beginFrame, frames);

            InitializeHDFace();
            PrepareData(list);
            UpdateCanvas(list);

            RotateTransform3D myRotateTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 1));
            _dataView.GeometryModel.Transform = myRotateTransform;

            RotateTransform3D myRotateTransform2 = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 1));
            _dataView.GeometryModelInterpol.Transform = myRotateTransform2;


            _dataView.interpolationCmbBox.ItemsSource = Enum.GetValues(typeof(InterpolationMethod)).Cast<InterpolationMethod>();
            _dataView.methodCmbBox.ItemsSource = Enum.GetValues(typeof(ScatteredInterpolationMethod)).Cast<ScatteredInterpolationMethod>();

            ExistingInterpolations = new ObservableCollection<string> { string.Empty };

            foreach (var item in Record.InterpolatedStorages.Keys)
            {
                ExistingInterpolations.Add(GetInterpolationStorageName(item));
            }

            _dataView.existingInterpolationCmbBox.ItemsSource = ExistingInterpolations;
            _dataView.SizeChanged += _window_SizeChanged;
        }

        private string GetInterpolationStorageName(string name)
        {
            string[] selected = name.Split('_');
            string nameSplit = selected[1] + " " + selected[2] + " " + selected[3].Replace("-", ",");
            return nameSplit;
        }


        private void GetRecordData()
        {
            MaxTime = Record.RecordLength.TotalMilliseconds;
            LastFrame = Record.LastFrame;
            FrameCount = Record.FrameCount;
            RecordExercises = Record.RecordExercise;
        }

        private void SetRecord(Examination selectedExamination)
        {
            Record = new Record(selectedExamination.Dir);

            if (!Record.HasTransformedData)
            {
                TransformData();
            }
            if (Record.SelectedInterpolationStorage != null)
            {
                string[] name = Record.InterpolatedStorages.Single(s => s.Value == Record.SelectedInterpolationStorage).Key.Split('_');
                ScatteredInterpolationMethod sim = ScatteredInterpolationMethod.None;
                InterpolationMethod iMethod = InterpolationMethod.None;
                double step = 0;

                GetInterpolationStorageType(name, out iMethod, out sim, out step);
                SetInterpolationStorage(iMethod, sim, step);

                _selectedExistingInterpolation = GetInterpolationStorageName(Record.InterpolatedStorages.Keys.First());
            }

            GetRecordData();

        }

        private void SetInterpolationStorage(InterpolationMethod iMethod, ScatteredInterpolationMethod sim, double step)
        {
            _selectedInterpolation = iMethod;
            if (sim != ScatteredInterpolationMethod.None)
                _selectedMethod = sim;
            if (step != 0)
                _selectedStep = step;
        }

        private void GetInterpolationStorageType(string[] name, out InterpolationMethod iMethod, out ScatteredInterpolationMethod sim, out double step)
        {
            int i = 1;
            sim = ScatteredInterpolationMethod.None;
            iMethod = (InterpolationMethod)Enum.Parse(typeof(InterpolationMethod), name[i]);
            i++;
            if (iMethod == InterpolationMethod.ScatteredData)
            {
                sim = (ScatteredInterpolationMethod)Enum.Parse(typeof(ScatteredInterpolationMethod), name[i]);
                i++;
            }

            string stepS = name[i].Replace('-', ',');
            step = double.Parse(stepS);
        }

        private Ellipse CreateEllipse(SolidColorBrush brush)
        {
            return CreateEllipse(brush, 3, 3);
        }

        private Ellipse CreateEllipse(SolidColorBrush brush, double width, double height)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = width,
                Height = height,
                Fill = brush
            };

            return ellipse;
        }

        private void PrepareData(ObservableCollection<RecordData> list)
        {
            var vertices = list[0].PointsFloat;

            if (vertices.Count > 0)
            {
                if (_pointsFront.Count == 0)
                {

                    for (int index = 0; index < _rightSideIndex; index++)
                    {
                        Ellipse ellipse2 = CreateEllipse(blue);

                        _pointsFront.Add(ellipse2);
                    }

                    for (int index = _rightSideIndex; index < vertices.Count; index++)
                    {
                        Ellipse ellipse2 = CreateEllipse(blue);
                        Ellipse ellipse3 = CreateEllipse(blue);
                        _pointsFront.Add(ellipse2);
                        _pointsProfile.Add(ellipse3);

                    }

                    foreach (Ellipse ellipse in _pointsProfile)
                    {
                        _dataView.canvasProfile.Children.Add(ellipse);
                    }
                    foreach (Ellipse ellipse in _pointsFront)
                    {
                        _dataView.canvasFront.Children.Add(ellipse);
                    }

                }
            }
        }


        private void DataViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedTab":
                    Pause();
                    UpdateData(_actualValue);
                    break;
                case "SelectedInterpolation":
                case "SelectedMethod":
                case "SelectedStep":
                    _dataView.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        PreviewInterpolationCommand.RaiseCanExecuteChanged();
                        InterpolateCommand.RaiseCanExecuteChanged();
                        IndexCommand.RaiseCanExecuteChanged();
                    }));
                    SetInterpolationStorage();
                    UpdateData(_actualValue);
                    break;

                case "SelectedExistingInterpolation":
                    if (SelectedExistingInterpolation == string.Empty) break;
                    string[] selected = SelectedExistingInterpolation.Split(' ');
                    SelectedInterpolation = (InterpolationMethod)Enum.Parse(typeof(InterpolationMethod), selected[0]);
                    SelectedMethod = (ScatteredInterpolationMethod)Enum.Parse(typeof(ScatteredInterpolationMethod), selected[1]);
                    SelectedStep = double.Parse(selected[2]);
                    break;

                default:
                    break;
            }
        }


        private void Slider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Value":
                    UpdateData(sender.GetType().GetProperty(e.PropertyName).GetValue(sender));
                    break;
                case "CurrentExercise":
                    _dataView.Dispatcher.BeginInvoke((Action)(() => RemoveExerciseCommand.RaiseCanExecuteChanged()));
                    break;
                default:
                    break;
            }
        }


        private void UpdateData(object value)
        {
            lock (_lock)
            {
                _actualValue = (double)value;

                switch (SelectedTab)
                {
                    case 0:
                        ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();
                        Record.ReadTransformData(ref list, _actualValue);
                        UpdateCanvas(list);
                        break;
                    case 1:
                        UpdateDataInterp();
                        break;
                    default:
                        break;
                }
            }

        }


        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private void UpdateCanvas(ObservableCollection<RecordData> list)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ => UpdateMesh(list));
            System.Threading.ThreadPool.QueueUserWorkItem(_ => UpdateCanvasProfile(list));
            System.Threading.ThreadPool.QueueUserWorkItem(_ => UpdateCanvasFront(list));
        }


        private void UpdateCanvasFront(ObservableCollection<RecordData> list)
        {
            var vertices = list[0].PointsFloat;

            if (vertices.Count > 0)
            {
                for (int index = 0; index < vertices.Count; index++)
                {
                    CameraSpacePoint vertice = vertices[index];

                    Ellipse ellipse = _pointsFront[index];

                    var canvasWidth = _dataView.canvasFront.ActualWidth;
                    var canvasHeight = _dataView.border3D.ActualHeight;

                    double moveX = canvasWidth / 2;
                    double moveY = canvasHeight / 4;
                    double multiply = canvasHeight / 5;

                    SolidColorBrush color = blue;

                    if (vertice.X < -0.01)
                    {
                        color = red;

                    }
                    else if (vertice.X > 0.01)
                    {
                        color = blue;
                    }
                    else
                    {
                        color = green;
                    }

                    _dataView.Dispatcher.BeginInvoke((Action)(() =>
                   {
                       ellipse.Fill = color;
                       Canvas.SetLeft(ellipse, vertice.X * multiply + moveX);
                       Canvas.SetTop(ellipse, (-vertice.Y * multiply) + moveY);

                   }));
                }
            }
        }


        private void UpdateCanvasProfile(ObservableCollection<RecordData> list)
        {
            var vertices = list[0].PointsFloat;
            int indexEllipse = 0;
            if (vertices.Count > 0)
            {
                for (int index = 0; index < vertices.Count; index++)
                {
                    CameraSpacePoint vertice = vertices[index];

                    if (vertice.X < 0.01)
                        continue;
                    if (indexEllipse == _pointsProfile.Count)
                        break;

                    Ellipse ellipse = _pointsProfile[indexEllipse];
                    indexEllipse++;

                    var canvasWidth = _dataView.canvasProfile.ActualWidth;
                    var canvasHeight = _dataView.border3D.ActualHeight / 2;

                    double moveY = (canvasHeight / 2);
                    double moveZ = (canvasWidth / 3);
                    double multiply = canvasHeight / 3;


                    _dataView.Dispatcher.BeginInvoke((Action)(() =>
                   {
                       Canvas.SetLeft(ellipse, vertice.Z * multiply + moveZ);
                       Canvas.SetTop(ellipse, -(vertice.Y * multiply) + moveY);

                   }));
                }

                if (indexEllipse != _pointsProfile.Count)
                {
                    for (int i = indexEllipse; i < _pointsProfile.Count; i++)
                    {
                        Ellipse ellipse = _pointsProfile[indexEllipse];
                        _dataView.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Canvas.SetLeft(ellipse, 0);
                            Canvas.SetTop(ellipse, 0);

                        }));

                    }
                }
            }
        }



        private void _window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void ResizingDone(object sender, ElapsedEventArgs e)
        {
            _resizeTimer.Stop();

            switch (SelectedTab)
            {
                case 0:
                    UpdateData(_actualValue);
                    break;

                case 1:
                    UpdateDataInterp();
                    break;
                default:
                    break;
            }
        }

        private void UpdateDataInterp()
        {

            if (!IsInterpolationCreated())
            {
                _dataView.Dispatcher.BeginInvoke((Action)(() =>
                {

                    if (_dataView.canvasInterpol.Children.OfType<Grid>().Count() == 0)
                        InsertWatermark();
                    else
                    {
                        Grid grid = _dataView.canvasInterpol.Children.OfType<Grid>().First();
                        grid.Width = _dataView.border3DInterpol.ActualWidth - _dataView.border3DInterpol.BorderThickness.Bottom * 2;
                        grid.Height = _dataView.border3DInterpol.ActualHeight - _dataView.border3DInterpol.BorderThickness.Bottom * 2;
                    }
                }));
            }
            else
            {
                _dataView.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (_dataView.canvasInterpol.Children.OfType<Grid>().Count() > 0)
                        RemoveWatermark();
                }));



                Int32Collection indices = new Int32Collection();
                Vector3DCollection normals = new Vector3DCollection();
                Point3DCollection positions = new Point3DCollection();
                PointCollection textureC = new PointCollection();



                //update mesh and canvas
                ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();
                Record.ReadInterpolatedData(ref list, _actualValue);



                double step = Record.SelectedInterpolationStorage.Step;
                var corrX = (list[0].LengthX / 2) * step;
                var corrY = (list[0].LengthY / 2) * step;

                foreach (var item in list[0].PointsDouble)
                {
                    var x = item.X;
                    var y = item.Y;
                    var p = item.Z;

                    Point3D point = new Point3D((y * step) - corrY, (x * step) - corrX, p);
                    positions.Add(point);
                    textureC.Add(new Point());
                }

                normals.Freeze();
                positions.Freeze();
                textureC.Freeze();

                //normals = CreateNormals(positions, list[0].LengthX, list[0].LengthY);
                indices = CreateIndices(list[0].LengthX, list[0].LengthY);
                indices.Freeze();

                _dataView.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _dataView.theGeometryInterpol.Normals = normals;
                    _dataView.theGeometryInterpol.Positions = positions;
                    _dataView.theGeometryInterpol.TriangleIndices = indices;
                    _dataView.theGeometryInterpol.TextureCoordinates = textureC;

                }));
            }
        }

        private Int32Collection CreateIndices(int lengthX, int lengthY)
        {

            Int32Collection indices = new Int32Collection();

            int points = 0;
            for (int x = 0; x < lengthX; x++)
            {
                for (int y = 0; y < lengthY; y++)
                {
                    if (x != 0 && y != lengthY - 1)
                    {
                        indices.Add(points);
                        indices.Add(points - lengthY);
                        indices.Add(points + 1 - lengthY);

                        if (y != 0)
                        {

                            indices.Add(points);
                            indices.Add(points - 1);
                            indices.Add(points - lengthY);
                        }
                    }

                    points++;
                }
            }

            return indices;
        }

        private void RemoveWatermark()
        {
            _dataView.canvasInterpol.Children.RemoveAt(1);
            _dataView.viewport3DInterpol.Visibility = Visibility.Visible;
        }

        private void InsertWatermark()
        {
            string message = "NO DATA";
            var width = _dataView.border3DInterpol.ActualWidth - _dataView.border3DInterpol.BorderThickness.Bottom * 2;
            var height = _dataView.border3DInterpol.ActualHeight - _dataView.border3DInterpol.BorderThickness.Bottom * 2;


            Grid grid = new Grid();
            grid.Width = width;
            grid.Height = height;
            grid.Background = Brushes.White;


            TextBlock text = new TextBlock();
            text.Text = message;
            text.Foreground = Brushes.Gray;
            text.FontSize = width / 10;
            text.FontWeight = FontWeights.Heavy;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.HorizontalAlignment = HorizontalAlignment.Center;


            grid.Children.Add(text);
            _dataView.canvasInterpol.Children.Add(grid);
            Canvas.SetTop(grid, 0);
            Canvas.SetLeft(grid, 0);

            _dataView.viewport3DInterpol.Visibility = Visibility.Hidden;
        }

        private bool IsInterpolationCreated()
        {
            StringBuilder name = new StringBuilder("Interpolation_" + SelectedInterpolation.ToString() + "_" + SelectedMethod.ToString());
            name.Append("_" + SelectedStep.ToString().Replace(",", "-"));

            bool isCreated = Record.InterpolatedStorages.ContainsKey(name.ToString());
            return isCreated;
        }

        private void SetInterpolationStorage()
        {
            if (IsInterpolationCreated())
            {
                StringBuilder name = new StringBuilder("Interpolation_" + SelectedInterpolation.ToString() + "_" + SelectedMethod.ToString());
                name.Append("_" + SelectedStep.ToString().Replace(",", "-"));

                var storage = Record.InterpolatedStorages.Single(x => x.Key == name.ToString());

                if (Record.SelectedInterpolationStorage != storage.Value)
                    Record.SelectedInterpolationStorage = storage.Value;

                SelectedExistingInterpolation = GetInterpolationStorageName(storage.Key);
            }
            else
            {

                SelectedExistingInterpolation = string.Empty;
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
                    Record.Dispose();
                    _resizeTimer.Dispose();
                }

                disposedValue = true;
                GC.SuppressFinalize(this);
            }
        }


        public void Dispose()
        {
            Dispose(true);

        }
        #endregion
    }
}
