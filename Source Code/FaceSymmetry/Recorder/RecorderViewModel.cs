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
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Recorder
{
    public partial class RecorderViewModel : ObservableObject, INotifyPropertyChanged
    {

        public RecorderWindow _view;
        private Emulator _emulator;
        private Thread _writer;
        private Record _record;
        private Patient _selectedPatient;

        private bool _isCapturing = false;
        private object _lock = new object();

        public List<RecordData> Feeder = new List<RecordData>();
        public PreciseDatetime PreciseTime = new PreciseDatetime();
        public ExerciseCollection Exercises { get; set; }

        private string _recordDescription = "Notes...";
        public string RecordDescription
        {
            get
            {
                return _recordDescription;
            }
            set
            {
                _recordDescription = value;
                OnPropertyChanged("RecordDescription");
            }
        }

        private ObservableExercise _selectedExercise;
        public ObservableExercise SelectedExercise
        {
            get { return _selectedExercise; }
            set
            {
                _selectedExercise = value;
                OnPropertyChanged("SelectedExercise");
            }
        }

        public IEnumerable<string> ActiveList
        {
            get
            {
                return Exercises.Where(x => x.Active == true).Select(x => x.Name).Distinct();
            }
        }


        public RecorderViewModel(Patient selectedPatient, bool isEmulator = false)
        {
            Exercises = DBAdapter.GetExercisesFromDB();

            _selectedPatient = selectedPatient;
            _view = new RecorderWindow(this);


            if (isEmulator)
            {
                _emulator = new Emulator(this, "C:\\ProgramData\\FaceSymmetry\\Records\\Emulator\\Emulator.bin");
                _emulator.Start();
            }

            _view.ShowDialog();

        }


        internal void SendDataToFeeder(IReadOnlyList<CameraSpacePoint> vertices)
        {
            lock (_lock)
            {
                Feeder.Add(new RecordData { Date = PreciseTime.Now, PointsFloat = (List<CameraSpacePoint>)vertices });
            }
        }

        internal void UpdateFacePoints(IReadOnlyList<CameraSpacePoint> list)
        {
            _view.UpdateMesh(list);
        }

        internal void StartCapture()
        {
            _isCapturing = true;

            _record = new Record(_selectedPatient, Exercises);
            _writer = new Thread(StartWriter);

            StopFaceCapture();
            faceModelBuilder = null;
            faceModelBuilder = highDefinitionFaceFrameSource.OpenModelBuilder(FaceModelBuilderAttributes.None);

            _record.StartRecording();
            _writer.Start();

            ThreadPool.QueueUserWorkItem(_ => faceModelBuilder.BeginFaceDataCollection());

        }

        private void StartWriter()
        {
            while (_isCapturing)
            {
                if (_record != null && Feeder.Count > 0)
                {
                    lock (_lock)
                    {
                        _record.WriteRawData(Feeder[0].Date, Feeder[0].PointsFloat);
                        Feeder.RemoveAt(0);
                    }
                }
            }
        }

        internal void StopCapture(bool suppressWindow = false)
        {
            try
            {
                this.StopFaceCapture();

                _isCapturing = false;
                while (_writer.IsAlive)
                {
                    Thread.Sleep(100);
                }

                _record.StopRecording(_view.recordDescriptionTxtBx.Text);


                if (!suppressWindow)
                {
                    new RecordSuccess(_record.FullPath).ShowDialog();
                }

            }

            catch (Exception ex)
            {
                new MessageBoxFS("Error", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }
        }

        internal void BeginExercise()
        {
            _record.BeginExercise();
        }

        internal void EndExercise()
        {
            if (SelectedExercise != null)
                _record.EndExercise(SelectedExercise.Name);
        }

        internal void Close()
        {
            if (_isCapturing)
            {
                EndExercise();
                StopCapture(true);

            }

            if (_emulator != null)
            {
                _emulator.Stop();
                _emulator.Dispose();
            }

        }
    }
}
