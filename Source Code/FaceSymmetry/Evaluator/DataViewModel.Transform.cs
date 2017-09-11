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
using MathWorks.MATLAB.NET.Arrays;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Evaluator
{
    public partial class DataViewModel
    {
        #region Transform
        public void TransformData()
        {
            ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();
            int beginFrame = 0;
            int frames = Record.FrameCount;


            _messageBox.UpdateText("Reading raw data");

            Record.ReadRawData(ref list, beginFrame, frames);

            _messageBox.UpdateText("Transforming data");

            Parallel.For(0, frames, (i) =>
             {
                 TransformData(list[i]);
             });

            _messageBox.UpdateText("Creating transform storage");

            Record.CreateTransformedStorage(list);

        }



        private void TransformData(RecordData recordData)
        {
            TranslateXYZ(recordData);
            RotateXY(recordData);
            Normalize(recordData);
            TranslateXYZ(recordData);
        }

        private void Normalize(RecordData recordData)
        {
            float[] Max = new float[] { recordData.PointsFloat.OrderByDescending(x => x.X).First().X,
                recordData.PointsFloat.OrderByDescending(x => x.Y).First().Y,
                recordData.PointsFloat.OrderByDescending(x => x.Z).First().Z};

            float[] Min = new float[] { recordData.PointsFloat.OrderBy(x => x.X).First().X,
                recordData.PointsFloat.OrderBy(x => x.Y).First().Y,
                recordData.PointsFloat.OrderBy(x => x.Z).First().Z};

            float max = Max.Max();
            float min = Min.Min();


            for (int i = 0; i < recordData.PointsFloat.Count; i++)
            {
                CameraSpacePoint newPoint = new CameraSpacePoint();
                newPoint.X = 2 * (recordData.PointsFloat[i].X - min) / (max - min) - 1;
                newPoint.Y = 2 * (recordData.PointsFloat[i].Y - min) / (max - min) - 1;
                newPoint.Z = 2 * (recordData.PointsFloat[i].Z - min) / (max - min) - 1;

                recordData.PointsFloat[i] = newPoint;
            }
        }

        private void SortByX(RecordData recordData)
        {
            recordData.PointsFloat = recordData.PointsFloat.OrderBy(x => x.X).ToList();
        }



        private void TranslateXYZ(RecordData recordData)
        {
          
            CameraSpacePoint center = recordData.PointsFloat[_centerIndex];

            for (int i = 0; i < recordData.PointsFloat.Count; i++)
            {
                CameraSpacePoint newPoint = new CameraSpacePoint();
                newPoint.X = recordData.PointsFloat[i].X - center.X;
                newPoint.Y = recordData.PointsFloat[i].Y - center.Y;
                newPoint.Z = recordData.PointsFloat[i].Z - center.Z;

                recordData.PointsFloat[i] = newPoint;
            }
        }

        private void RotateXY(RecordData recordData)
        {
            CameraSpacePoint centerForeHead = recordData.PointsFloat[_centerForeheadIndex];
            var alpha = Math.Atan2(centerForeHead.X, centerForeHead.Y);

            for (int i = 0; i < recordData.PointsFloat.Count; i++)
            {
                CameraSpacePoint newPoint = new CameraSpacePoint();
                newPoint.X = (recordData.PointsFloat[i].X * (float)Math.Cos(alpha)) - (recordData.PointsFloat[i].Y * (float)Math.Sin(alpha));
                newPoint.Y = (recordData.PointsFloat[i].X * (float)Math.Sin(alpha)) + (recordData.PointsFloat[i].Y * (float)Math.Cos(alpha));
                newPoint.Z = recordData.PointsFloat[i].Z;

                recordData.PointsFloat[i] = newPoint;
            }
        }

        #endregion

        #region kinect

        private FaceAlignment currentFaceAlignment = null;
        private FaceModel currentFaceModel = null;
        internal void InitializeHDFace()
        {
            currentFaceModel = new FaceModel();
            currentFaceAlignment = new FaceAlignment();
            InitializeMesh();
            UpdateMesh();
        }

        internal void InitializeMesh()
        {

            var vertices = this.currentFaceModel.CalculateVerticesForAlignment(this.currentFaceAlignment);

            var triangleIndices = this.currentFaceModel.TriangleIndices;

            var indices = new Int32Collection(triangleIndices.Count);

            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                uint index01 = triangleIndices[i];
                uint index02 = triangleIndices[i + 1];
                uint index03 = triangleIndices[i + 2];

                indices.Add((int)index03);
                indices.Add((int)index02);
                indices.Add((int)index01);
            }

            _dataView.theGeometry.TriangleIndices = indices;
            _dataView.theGeometry.Normals = null;
            _dataView.theGeometry.Positions = new Point3DCollection();
            _dataView.theGeometry.TextureCoordinates = new PointCollection();

            foreach (var vert in vertices)
            {
                _dataView.theGeometry.Positions.Add(new Point3D(vert.X, vert.Y, -vert.Z));
                _dataView.theGeometry.TextureCoordinates.Add(new Point());
            }
        }

        public void UpdateMesh()
        {
            var vertices = currentFaceModel.CalculateVerticesForAlignment(currentFaceAlignment);

            for (int i = 0; i < vertices.Count; i++)
            {
                var vert = vertices[i];
                _dataView.theGeometry.Positions[i] = new Point3D(vert.X, vert.Y, -vert.Z);
            }

        }

        public void UpdateMesh(ObservableCollection<RecordData> list)
        {
            var vertices = list[0].PointsFloat;
            _dataView.Dispatcher.BeginInvoke((Action)(() =>
           {
               for (int i = 0; i < vertices.Count; i++)
               {
                   var vert = vertices[i];
                   _dataView.theGeometry.Positions[i] = new Point3D(vert.X, vert.Y, -vert.Z);
               }
           }));
        }


        #endregion

        #region Interpolation
      

        private InterpolationMethod _selectedInterpolation;
        public InterpolationMethod? SelectedInterpolation
        {
            get
            {
                return _selectedInterpolation;
            }
            set
            {
                if (value == null) return;

                if (value.GetType() == typeof(string))
                {
                    InterpolationMethod newValue = (InterpolationMethod)Enum.Parse(typeof(InterpolationMethod), value.ToString());
                    if (newValue != _selectedInterpolation)
                    {
                        _selectedInterpolation = newValue;
                        OnPropertyChanged("SelectedInterpolation");
                    }
                }
                else
                {
                    if (value != _selectedInterpolation)
                    {
                        _selectedInterpolation = (InterpolationMethod)value;
                        OnPropertyChanged("SelectedInterpolation");
                    }
                }
            }
        }

        private string _selectedExistingInterpolation;
        public string SelectedExistingInterpolation
        {
            get
            {
                return _selectedExistingInterpolation;
            }
            set
            {
                if (value != null && _selectedExistingInterpolation != value)
                {
                    _selectedExistingInterpolation = value;
                    OnPropertyChanged("SelectedExistingInterpolation");
                }

            }

        }

        private ScatteredInterpolationMethod _selectedMethod;
        public ScatteredInterpolationMethod? SelectedMethod
        {
            get
            {
                return _selectedMethod;
            }
            set
            {
                if (value == null) return;

                if (value.GetType() == typeof(string))
                {
                    ScatteredInterpolationMethod newValue = (ScatteredInterpolationMethod)Enum.Parse(typeof(ScatteredInterpolationMethod), value.ToString());
                    if (newValue != _selectedMethod)
                    {
                        _selectedMethod = newValue;
                        OnPropertyChanged("SelectedMethod");
                    }
                }
                else
                {
                    if (value != _selectedMethod)
                    {
                        _selectedMethod = (ScatteredInterpolationMethod)value;
                        OnPropertyChanged("SelectedMethod");
                    }
                }
            }
        }
        

        private double _selectedStep = 0.005;
        public double SelectedStep
        {
            get
            {
                return _selectedStep;
            }
            set
            {
                if (_selectedStep != value)
                {
                    _selectedStep = value;
                    OnPropertyChanged("SelectedStep");
                }
            }
        }

        private void PreviewInterpolation()
        {
            _messageBox = new MessageBoxFS("Interpolating...", "Starting");
            _messageBox.Show();
            Task init = Task.Factory.StartNew(() => PreviewInterpolationTask());
        }


        private void PreviewInterpolationTask()
        {
            ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();

            lock (_lock)
            {
                Record.ReadTransformData(ref list, _actualValue);
            }


            try
            {
                var vertic = list[0].PointsFloat;

                _messageBox.UpdateText("Interpolating");
                MWArray grid = GetSelectedInterpolation().Interpolate(vertic, SelectedStep, SelectedMethod.ToString());

                _messageBox.UpdateText("Updating mesh");


                _dataView.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (_dataView.canvasInterpol.Children.OfType<Grid>().Count() > 0)
                        RemoveWatermark();

                    PreviewUpdate(grid, SelectedStep);
                    _messageBox.Close(true);
                    _messageBox = null;

                }));


            }
            catch (Exception ex)
            {
                new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
            }
        }

        private Interpolations.Interpolation GetSelectedInterpolation()
        {
            Interpolations.Interpolation interpolation = null;
            switch (SelectedInterpolation)
            {
                case InterpolationMethod.ScatteredData:
                    interpolation = new Interpolations.ScatteredData();
                    break;

            }

            return interpolation;
        }

        private void PreviewUpdate(MWArray grid, double step)
        {
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            Point3DCollection positions = new Point3DCollection();
            PointCollection textureC = new PointCollection();

            var dimensions = grid.Dimensions;

            var lengthX = dimensions[0];
            var lengthY = dimensions[1];

            var corrX = (lengthX / 2) * step;
            var corrY = (lengthY / 2) * step;

            int points = 0;
            int totalPoints = lengthX * lengthY;

            _messageBox.ProgressBar.IsIndeterminate = false;
            _messageBox.ProgressBar.Value = 0;
            _messageBox.ProgressBar.Maximum = totalPoints;

            Array gridArray = grid.ToArray();

            for (int x = 0; x < lengthX; x++)
            {
                for (int y = 0; y < lengthY; y++)
                {

                    try
                    {
                        double z = (double)gridArray.GetValue(x, y);

                        if (double.IsNaN(z))
                        {
                            z = double.MaxValue;
                        }

                        Point3D point = new Point3D((y * step) - corrY, (x * step) - corrX, z);
                        positions.Add(point);
                        textureC.Add(new Point());

                        if (x != 0 && y != lengthY - 1)
                        {
                            ////B        
                            int A = points;
                            int B = points - lengthY;
                            int C = points + 1 - lengthY;
                            indices.Add(A);
                            indices.Add(B);
                            indices.Add(C);

                            if (y != 0)
                            {
                                //A                                    
                                indices.Add(points);
                                indices.Add(points - 1);
                                indices.Add(points - lengthY);
                            }
                        }

                        points++;

                        _messageBox.UpdateProgressBar(points);

                    }
                    catch (Exception ex)
                    {
                        new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
                    }
                }


            }

            _dataView.Dispatcher.BeginInvoke((Action)(() =>
             {
                 _dataView.theGeometryInterpol.Normals = normals;
                 _dataView.theGeometryInterpol.Positions = positions;
                 _dataView.theGeometryInterpol.TextureCoordinates = textureC;
                 _dataView.theGeometryInterpol.TriangleIndices = indices;

             }));
        }


        private Vector3DCollection CreateNormals(Point3DCollection positions, int lengthX, int lengthY)
        {
            bool last = true;
            Vector3DCollection normals = new Vector3DCollection();
            for (int i = 0; i < positions.Count; i++)
            {
                if ((i + 1) % (lengthY * (lengthX - 1) + 1) != 0 && last)// neni posledni radek
                {
                    if ((i + 1) % (lengthY) != 0)  // neni posledni sloupec
                    {
                        //A
                        Vector3D normal = CalculateNormal(positions[i], positions[i + 1], positions[i + lengthY]);
                        normals.Add(normal);
                    }
                    else
                    {
                        //B
                        Vector3D normal = CalculateNormal(positions[i], positions[i - 1], positions[i + lengthY]);
                        normals.Add(normal);
                    }
                }
                else
                {
                    last = false;

                    if ((i + 1) % (lengthY) != 0 && i != positions.Count - 1)  // neni posledni sloupec
                    {
                        //A
                        Vector3D normal = CalculateNormal(positions[i], positions[i + 1], positions[i - lengthY]);
                        normals.Add(normal);

                    }
                    else
                    {
                        //B
                        Vector3D normal = CalculateNormal(positions[i], positions[i - 1], positions[i - lengthY]);
                        normals.Add(normal);
                    }
                }
            }

            return normals;
        }




        #endregion
    }
}
