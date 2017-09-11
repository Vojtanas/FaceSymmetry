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

using MathWorks.MATLAB.NET.Arrays;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace Common
{
    public class Record : IDisposable
    {
        public string FullPath;
        private string _dir;
        private string _storageRawPath;
        private Storage _storageRaw;
        private string _storageTransformPath;
        private Storage _storageTransform;
        private Patient _patient;
        private DateTime _dateTimeStart;
        private TimeSpan? _beginExercise;

        public PreciseDatetime PreciseTime = new PreciseDatetime();
        public IDictionary<string, Storage> InterpolatedStorages = new Dictionary<string, Storage>();
        public Storage SelectedInterpolationStorage { get; set; }
        public ExerciseDictionary RecordExercise;
        public int FrameCount;
        public DateTime FirstFrame;
        public DateTime LastFrame;
        public TimeSpan RecordLength;
        public ExerciseCollection Exercises;
        public string Description;

        public bool HasTransformedData
        {
            get
            {
                return _storageTransform == null ? false : true;
            }
        }

        public double[] FrameTimesMs { get; private set; }


        public Record(Patient patient, ExerciseCollection exercises)
        {
            _patient = patient;
            Exercises = exercises;
            RecordExercise = new ExerciseDictionary();
        }

        public Record(string dir)
        {
            _dir = dir;
            FullPath = Settings.SavePathDir + "\\" + _dir;
            Deserialize(FullPath);

            var storage = Directory.EnumerateFiles(FullPath, string.Format("{0}.bin", _dir)).Single();
            _storageRaw = new Storage(storage, StorageType.RawData);

            try
            {
                storage = Directory.EnumerateFiles(FullPath, string.Format("TransformedData.bin", _dir)).Single();
                _storageTransform = new Storage(storage, StorageType.Transformed);
            }
            catch (InvalidOperationException)
            {
                // do nothing
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                var interpolate = Directory.EnumerateFiles(FullPath, string.Format("Interpolation*.bin", _dir));

                foreach (var item in interpolate)
                {
                    string name = new FileInfo(item).Name.Replace(".bin", "");
                    InterpolatedStorages.Add(name, new Storage(item, StorageType.Interpolated));
                }

                if (InterpolatedStorages.Count != 0)
                {
                    SelectedInterpolationStorage = InterpolatedStorages.First().Value;
                }
            }
            catch (InvalidOperationException)
            {
                // do nothing
            }
            catch (Exception)
            {
                throw;
            }

            GetRecordData();

        }
        

        public void CreateTransformedStorage(ObservableCollection<RecordData> list)
        {
            try
            {
                _storageTransformPath = FullPath + "\\TransformedData.bin";
                _storageTransform = new Storage(_storageTransformPath, StorageType.Transformed);
                _storageTransform.WriteData(list);
            }
            catch (Exception)
            {
                if (File.Exists(_storageTransformPath))
                {
                    _storageTransform.Dispose();
                    File.Delete(_storageTransformPath);
                }
            }
        }

        public void RemoveInterpolatedStorage(string name)
        {
            Storage storage = InterpolatedStorages[name];
            storage.Dispose();
            storage.Delete();
            InterpolatedStorages.Remove(name);
        }

        public void CreateInterpolationStorage(string name)
        {
            string filePath = FullPath + "\\" + name + ".bin";
            Storage storage = new Storage(filePath, StorageType.Interpolated, FrameCount);
            InterpolatedStorages.Add(name, storage);
            SelectedInterpolationStorage = storage;
            SelectedInterpolationStorage.FramePositions = new long[FrameCount];
        }

        private void GetRecordData()
        {
            FrameCount = _storageRaw.MaxFrame;
            ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();

            _storageRaw.ReadData(ref list, 0, FrameCount);
            FirstFrame = list.First().Date;
            LastFrame = list.Last().Date;
            RecordLength = LastFrame - FirstFrame;

            FrameTimesMs = new double[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var time = list[i].Date - FirstFrame;
                FrameTimesMs[i] = time.TotalMilliseconds;
            }
        }

        private void Deserialize(string fullPath)
        {
            DeserializeTXT(fullPath);
            DeserializeXML(fullPath);
        }

        private void DeserializeTXT(string fullPath)
        {
            string descriptionPath = FullPath + "\\Record.txt";

            if (!File.Exists(descriptionPath))
            {
                throw new Exception("Record.txt does note exists!");
            }

            using (TextReader tr = new StreamReader(descriptionPath))
            {
                Description = tr.ReadToEnd();
            }
        }

        private void DeserializeXML(string fullPath)
        {
            string exercisesPath = FullPath + "\\Exercises.xml";

            if (!File.Exists(exercisesPath))
            {
                throw new Exception("Record.txt does note exists!");
            }

            RecordExercise = new ExerciseDictionary();
            var doc = XDocument.Load(exercisesPath);

            foreach (var ex in doc.Descendants("Exercise"))
            {
                string name = ex.Element("Name").Value;
                string description = ex.Element("Description").Value;

                ObservableExercise exercise = new ObservableExercise(name, description);
                RecordExercise.Add(exercise, new List<TimeSpan[]>());

                foreach (var times in ex.Descendants("Measurement"))
                {
                    TimeSpan start = TimeSpan.Parse(times.Element("TimeStart").Value);
                    TimeSpan end = TimeSpan.Parse(times.Element("TimeEnd").Value);

                    RecordExercise[exercise].Add(new TimeSpan[] { start, end });
                }
            }
        }

        public void StartRecording()
        {
            _dateTimeStart = PreciseTime.Now;
            _dir = _dateTimeStart.Year + "_" + _dateTimeStart.Month.ToString("d2") + "_" + _dateTimeStart.Day.ToString("d2") + "_" + _dateTimeStart.Hour.ToString("d2") + "_" + _dateTimeStart.Minute.ToString("d2") + "_" + _dateTimeStart.Second.ToString("d2");
            FullPath = Settings.SavePathDir + "\\" + _dir;
            _storageRawPath = FullPath + "\\" + FullPath.Split(new string[] { "\\" }, StringSplitOptions.None).Last() + ".bin";
            Directory.CreateDirectory(FullPath);

            _storageRaw = new Storage(_storageRawPath, StorageType.RawData);

            if (RecordExercise == null)
                RecordExercise = new ExerciseDictionary();
            else if (RecordExercise.Count != 0)
                RecordExercise.Clear();

        }

        public void WriteRawData(DateTime date, List<CameraSpacePoint> points)
        {
            if (_storageRaw != null)
                _storageRaw.WriteData(date, points);
        }

        public void ReadRawData(ref ObservableCollection<RecordData> list, int beginFrame, int frames)
        {

            _storageRaw?.ReadData(ref list, beginFrame, frames);
        }

        internal void WriteInterpolatedHeader(IList<MWArray> grids)
        {
            long byteSize = 4 + grids.Count * sizeof(long);

            SelectedInterpolationStorage.FramePositions[0] = byteSize;

            for (int i = 0; i < grids.Count - 1; i++)
            {
                var dimensions = grids[i].Dimensions;
                byteSize = byteSize + ((dimensions[0] * dimensions[1]) * sizeof(double) * 3 + 8 + 2 * sizeof(int)); // + date + 2*length
                SelectedInterpolationStorage.FramePositions[i + 1] = byteSize;
            }

            SelectedInterpolationStorage.WriteHeader();
        }

        internal void WriteInterpolateData(DateTime date, MWArray grid, int frame)
        {
            var dimensions = grid.Dimensions;

            var lengthX = dimensions[0];
            var lengthY = dimensions[1];

            IList<Point3D> points = new List<Point3D>();

            Array gridArray = grid.ToArray();

            for (int x = 0; x < lengthX; x++)
            {
                for (int y = 0; y < lengthY; y++)
                {

                    double z = (double)gridArray.GetValue(x, y);

                    if (double.IsNaN(z))
                    {
                        z = double.MaxValue;
                    }

                    Point3D point = new Point3D(x, y, z);
                    points.Add(point);
                }
            }

            SelectedInterpolationStorage.WriteInterpolatedData(date, points, lengthX, lengthY, frame);
        }

        public void ReadInterpolatedData(ref ObservableCollection<RecordData> list, double time)
        {
            int beginFrame = GetActualFrameFromTime(time);
            SelectedInterpolationStorage?.ReadInterpolatedData(ref list, beginFrame, 1);
        }

        public void ReadInterpolatedData(ref ObservableCollection<RecordData> list, int beginFrame, int endFrame)
        {
            int frameCount = endFrame - beginFrame;

            SelectedInterpolationStorage?.ReadInterpolatedData(ref list, beginFrame, frameCount);
        }

        public void ReadRawData(ref ObservableCollection<RecordData> list, double time)
        {
            int beginFrame = GetActualFrameFromTime(time);
            _storageRaw?.ReadData(ref list, beginFrame, 1);
        }

        public void ReadTransformData(ref ObservableCollection<RecordData> list, int beginFrame, int frames)
        {
            _storageTransform?.ReadData(ref list, beginFrame, frames);
        }

        public void ReadTransformData(ref ObservableCollection<RecordData> list, double time)
        {
            int beginFrame = GetActualFrameFromTime(time);
            _storageTransform?.ReadData(ref list, beginFrame, 1);
        }

        internal void ReconnectStorage(string name)
        {
            string filePath = FullPath + "\\" + name + ".bin";
            InterpolatedStorages.Remove(name);
            SelectedInterpolationStorage.Dispose();
            SelectedInterpolationStorage = new Storage(filePath, StorageType.Interpolated);
            InterpolatedStorages.Add(name, SelectedInterpolationStorage);
        }

        public int GetActualFrameFromTime(double timeMs)
        {
            int frame = 0;

            for (frame = 0; frame < FrameTimesMs.Length; frame++)
            {
                if (timeMs < FrameTimesMs[frame])
                {
                    if (frame == 0)
                        return frame;

                    double currentDif = Math.Abs(FrameTimesMs[frame] - timeMs);
                    double previousDif = Math.Abs(FrameTimesMs[frame - 1] - timeMs);

                    if (currentDif > previousDif)
                    {
                        frame--;
                    }

                    break;
                }
            }

            if (frame >= FrameTimesMs.Length)
                frame = FrameTimesMs.Length - 1;

            return frame;
        }

        public void StopRecording(string recordDescription)
        {
            try
            {
                SaveExerciseXML();
                SaveRecordTXT(recordDescription);
                _storageRaw.Dispose();

                SaveToDB(recordDescription);
            }
            catch (DataBaseException ex)
            {
                new MessageBoxFS("Error", "Database error\n" + ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }
            catch (Exception ex)
            {
                new MessageBoxFS("Error", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }

        }

        private void SaveToDB(string recordDescription)
        {
            string exercisesAr = string.Join(";", RecordExercise.Select(x => x.Key.Name).ToArray());

            string table = "examination";
            string[] column = { "dir", "patientID", "guid", "created", "notes", "exercises" };
            string[] values = { _dir, _patient.ID.ToString(), Guid.NewGuid().ToString(), _dateTimeStart.ToString("yyyy-MM-dd HH:mm:ss"), recordDescription, exercisesAr };

            DBAdapter.Insert(table, column, values);
        }

        private void SaveRecordTXT(string recordDescription)
        {
            string path = FullPath + "\\Record.txt";
            File.Create(path).Close();
            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(recordDescription);
                tw.Close();
            }
        }

        public void SaveExerciseXML()
        {
            XDocument document = new XDocument(
            new XComment("Face Symmetry Recorder Exercises"),
            new XElement("Exercises",
                 from exercise in RecordExercise
                 select
                 new XElement("Exercise",
                 new XElement("Name", exercise.Key.Name),
                 new XElement("Description", exercise.Key.Description),
                 new XElement("Times",
                 from time in exercise.Value
                 select
                    new XElement("Measurement",
                    new XElement("TimeStart", time[0].ToString()),
                    new XElement("TimeEnd", time[1].ToString())
                )))));

            document.Save(FullPath + "\\Exercises.xml");
        }

        public void EndExercise(string exercise)
        {
            if (_beginExercise != null)
            {
                TimeSpan endExercise = PreciseTime.Now - _dateTimeStart;
                string key = exercise;

                if (!RecordExercise.Keys.Any(x => x.Name == key))
                {
                    RecordExercise.Add(Exercises.Single(x => x.Name == key), new List<TimeSpan[]>());
                }

                RecordExercise[Exercises.Single(x => x.Name == key)].Add(new TimeSpan[] { (TimeSpan)_beginExercise, endExercise });
                _beginExercise = null;
            }
        }

        public void BeginExercise()
        {
            _beginExercise = PreciseTime.Now - _dateTimeStart;
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _storageRaw?.Dispose();
                    _storageTransform?.Dispose();

                    foreach (var item in InterpolatedStorages.Values)
                    {
                        item.Dispose();
                    }
                }

                _storageRaw = null;
                _storageTransform = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
