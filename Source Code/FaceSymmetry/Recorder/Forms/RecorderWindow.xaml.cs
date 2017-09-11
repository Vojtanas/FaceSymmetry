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
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Recorder
{

    public partial class RecorderWindow : Window, INotifyPropertyChanged, IDisposable
    {
        private RecorderViewModel _viewModel;
        private string SelectedItem { get; set; }
        private bool _isRecording = false;

        private readonly SolidColorBrush BrushRed = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush BrushGreen = new SolidColorBrush(Color.FromRgb(68, 185, 50));

        private Brush _captureBrush;
        public Brush CaptureBrush
        {
            get
            {
                return _captureBrush;
            }
            set
            {
                _captureBrush = value;
                OnPropertyChanged("CaptureBrush");
            }
        }

        private Style _buttonStyle;
        public Style ButtonStyle
        {
            get
            {
                return _buttonStyle;
            }
            set
            {
                _buttonStyle = value;
                OnPropertyChanged("ButtonStyle");
            }
        }


        double thresholdYellowX = 0.05;
        double thresholdYellowY = 0.05;
        double thresholdRedX = 0.1;
        double thresholdRedY = 0.1;


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public RecorderWindow(RecorderViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();

            CaptureBrush = BrushGreen;
            ButtonStyle = FindResource("ButtonStyleGreen") as Style;

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.InitializeHDFace();
        }


        private void EnableDisableButtons()
        {
            if (_isRecording)
            {
                beginExBtn.IsEnabled = true;
            }
            else
            {
                beginExBtn.IsEnabled = false;

            }
        }


        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            _isRecording = !_isRecording;
            EnableDisableButtons();

            if (StartCaptureButton.Content.ToString() == "Start Capture")
            {
                _viewModel.StartCapture();

                StartCaptureButton.Content = "Stop Capture";
                CaptureBrush = BrushRed;
                ButtonStyle = FindResource("ButtonStyleRed") as Style;


            }
            else
            {
                _viewModel.StopCapture();
                StartCaptureButton.Content = "Start Capture";
                CaptureBrush = BrushGreen;
                ButtonStyle = FindResource("ButtonStyleGreen") as Style;
            }

        }


        private void BeginExercise_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedExercise != null)
            {

                if (beginExBtn.Content.ToString() == "Begin Exercise")
                {
                    exerciseListBox.IsEnabled = false;
                    beginExBtn.Content = "End Exercise";
                    beginExBtn.Background = new SolidColorBrush(Colors.Red);
                    _viewModel.BeginExercise();
                }
                else
                {
                    beginExBtn.Content = "Begin Exercise";
                    beginExBtn.Background = new SolidColorBrush(Color.FromRgb(68, 185, 50));

                    _viewModel.EndExercise();
                    exerciseListBox.IsEnabled = true;

                }

            }
            else
            {
                new MessageBoxFS("Warning", "Select Exercise", MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
            }
        }




        public void UpdateMesh(IReadOnlyList<CameraSpacePoint> list = null)
        {
            IReadOnlyList<CameraSpacePoint> vertices = null;

            if (list == null)
            {
                vertices = _viewModel.currentFaceModel.CalculateVerticesForAlignment(_viewModel.currentFaceAlignment);
                SwitchFaceColor(_viewModel.currentFaceAlignment.FaceOrientation);
            }
            else
            {
                vertices = list.ToList();
            }

            if (_isRecording)
            {
                _viewModel.SendDataToFeeder(vertices);
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {

                for (int i = 0; i < vertices.Count; i++)
                {
                    var vert = vertices[i];
                    theGeometry.Positions[i] = new Point3D(vert.X, vert.Y, -vert.Z);
                }

            }));

        }



        private void SwitchFaceColor(Vector4 faceOrientation)
        {
            if ((faceOrientation.X > thresholdYellowX + 0.05 || faceOrientation.X < -thresholdYellowX) || (faceOrientation.Y > thresholdYellowY || faceOrientation.Y < -thresholdYellowY))
            {
                if ((faceOrientation.X > thresholdRedX + 0.05 || faceOrientation.X < -thresholdRedX) || (faceOrientation.Y > thresholdRedY || faceOrientation.Y < -thresholdRedY))
                {
                    if (theMaterial.Brush != Brushes.Red)
                        theMaterial.Brush = Brushes.Red;
                }
                else
                {
                    if (theMaterial.Brush != Brushes.Yellow)
                        theMaterial.Brush = Brushes.Yellow;
                }
            }
            else
            {
                if (theMaterial.Brush != Brushes.Green)
                    theMaterial.Brush = Brushes.Green;
            }
        }

        private void exerciseListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            string selectedItem = listBox.SelectedItem.ToString();

            _viewModel.SelectedExercise = _viewModel.Exercises.Where(x => x.Name == selectedItem).Single();
        }



        private void recordDescriptionTxtBx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (recordDescriptionTxtBx.Text == "Notes...")
            {
                recordDescriptionTxtBx.Text = "";
                recordDescriptionTxtBx.Foreground = Brushes.Black;
            }

        }

        private void recordDescriptionTxtBx_LostFocus(object sender, RoutedEventArgs e)
        {
            recordDescriptionTxtBx.BorderBrush = Brushes.Black;
            if (recordDescriptionTxtBx.Text == "")
            {
                recordDescriptionTxtBx.Text = "Notes...";
                recordDescriptionTxtBx.Foreground = Brushes.LightGray;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _viewModel.Close();
        }


        #region Dispose


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_viewModel.currentFaceModel != null)
                {
                    _viewModel.currentFaceModel.Dispose();
                    _viewModel.currentFaceModel = null;
                }
            }
        }

        #endregion


    }
}
