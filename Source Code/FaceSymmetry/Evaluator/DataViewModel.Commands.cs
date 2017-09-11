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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public partial class DataViewModel
    {

        #region Play
        private CommandHandler _playCommand;
        public CommandHandler PlayCommand
        {
            get
            {
                if (_playCommand == null)
                {
                    _playCommand = new CommandHandler(
                        param => Play(),
                        param => CanPlay()
                    );
                }
                return _playCommand;
            }
        }

        private bool CanPlay()
        {
            return Record != null ? true : false;
        }

        public void Play()
        {
            _dataView?.slider?.Start();
        }

        #endregion

        #region Pause

        private CommandHandler _pauseCommand;
        public CommandHandler PauseCommand
        {
            get
            {
                if (_pauseCommand == null)
                {
                    _pauseCommand = new CommandHandler(
                        param => Pause(),
                        param => CanPlay()
                    );
                }
                return _pauseCommand;
            }
        }

        public bool IsPlaying
        {
            get { return (bool)_dataView?.slider?.Timer.Enabled; }
        }
        private bool CanPause()
        {
            return Record != null && IsPlaying ? true : false;
        }

        public void Pause()
        {
            _dataView?.slider?.Pause();
        }

        #endregion

        #region RemoveExercise
        private CommandHandler _removeExerciseCommand;
        public CommandHandler RemoveExerciseCommand
        {
            get
            {
                if (_removeExerciseCommand == null)
                {
                    _removeExerciseCommand = new CommandHandler(
                        param => RemoveExercise(),
                        param => CanRemoveExercise()
                    );
                }
                return _removeExerciseCommand;
            }
        }

        private bool CanRemoveExercise()
        {
            bool isExercise = _dataView?.slider?.CurrentExercise != null ? true : false;
            return (isExercise && !IsPlaying);
        }

        private void RemoveExercise()
        {
            if (!isRecordChanged) isRecordChanged = true;

            var exercise = _dataView?.slider?.CurrentExercise;
            foreach (var item in RecordExercises[exercise])
            {
                var a = item[0].TotalMilliseconds;
                var b = item[1].TotalMilliseconds;

                if (_actualValue >= a && _actualValue <= b)
                {
                    RecordExercises[exercise].Remove(item);
                    RecordExercises.OnKeyChanged(exercise.Name);
                    _dataView.slider.FirstDrawOrChange = true;
                    _dataView?.slider.InvalidateVisual();
                    break;
                }

            }

        }
        #endregion

        #region InsertExercise
        private CommandHandler _insertExerciseCommand;
        public CommandHandler InsertExerciseCommand
        {
            get
            {
                if (_insertExerciseCommand == null)
                {
                    _insertExerciseCommand = new CommandHandler(
                        param => InsertExercise(),
                        param => CanInsertExercise()
                    );
                }
                return _insertExerciseCommand;
            }
        }

        private bool CanInsertExercise()
        {
            bool canInsert = _dataView?.ActiveListBox.SelectedIndex != -1;
            return canInsert;
        }


        private bool isRecordChanged = false;
        private void InsertExercise()
        {
            if (!isRecordChanged) isRecordChanged = true;

            string exercise = _dataView?.ActiveListBox?.SelectedItem.ToString();
            TimeSpan[] times = new TimeSpan[] { TimeSpan.FromMilliseconds(_actualValue), TimeSpan.FromMilliseconds(_actualValue + 3000) };
            var selectedExercise = RecordExercises.Keys.SingleOrDefault(x => x.Name == exercise);

            if (selectedExercise == null)
            {
                var list = DBAdapter.GetExercisesFromDB();
                selectedExercise = list.Single(x => x.Name == exercise);
                RecordExercises.Add(selectedExercise, new List<TimeSpan[]>());
            }

            RecordExercises[selectedExercise].Add(times);
            RecordExercises.OnKeyChanged(selectedExercise.Name);
            _dataView.slider.FirstDrawOrChange = true;
            _dataView.slider.InvalidateVisual();
            _dataView.tabControl.Focus();


        }

        #endregion

        #region Save
        private CommandHandler _saveCommand;
        public CommandHandler SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new CommandHandler(
                        param => Save(),
                       null
                    );
                }
                return _saveCommand;
            }
        }



        private void Save()
        {
            Record.SaveExerciseXML();
        }

        #endregion

        #region PreviewInterpolation
        private CommandHandler _previewInterpolationCommand;
        public CommandHandler PreviewInterpolationCommand
        {
            get
            {
                if (_previewInterpolationCommand == null)
                {
                    _previewInterpolationCommand = new CommandHandler(
                        param => PreviewInterpolation(),
                        param => CanInterpolate()
                    );
                }
                return _previewInterpolationCommand;
            }
        }


        #endregion

        #region Interpolation
        private CommandHandler _interpolateCommand;
        public CommandHandler InterpolateCommand
        {
            get
            {
                if (_interpolateCommand == null)
                {
                    _interpolateCommand = new CommandHandler(
                        param => Interpolate(),
                        param => CanInterpolate()
                    );
                }
                return _interpolateCommand;
            }
        }


        private void Interpolate()
        {
            StringBuilder name = new StringBuilder("Interpolation_" + SelectedInterpolation.ToString() + "_" + SelectedMethod.ToString());



            name.Append("_" + SelectedStep.ToString().Replace(",", "-"));

            bool exists = File.Exists(Record.FullPath + "\\" + name + ".bin");
            if (exists)
            {
                _messageBox = new MessageBoxFS("Warning", "Selected interpolation already exists, do you wish to overwrite?", MessageBoxFSButton.YESNO, MessageBoxFSImage.Question);
                bool result = (bool)_messageBox.ShowDialog();

                if (result == true)
                {
                    Record.RemoveInterpolatedStorage(name.ToString());
                }
                else
                {
                    return;
                }
            }

            Task.Factory.StartNew(() => InterpolateWork(name.ToString()));

        }
        private void InterpolateWork(string name)
        {
            _dataView.Dispatcher.BeginInvoke((Action)(() =>
            {
                _messageBox = new MessageBoxFS("Interpolating...", "Starting");
                _messageBox.ShowDialog();
            }));


            Record.CreateInterpolationStorage(name.ToString());
            Interpolations.Interpolation interpolation = GetSelectedInterpolation();


            ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();

            lock (_lock)
            {
                Record.ReadTransformData(ref list, 0, Record.FrameCount);
            }


            _dataView.Dispatcher.BeginInvoke((Action)(() =>
            {
                _messageBox.ProgressBar.IsIndeterminate = false;
                _messageBox.UpdateProgressBar(0);
                _messageBox.ProgressBar.Maximum = list.Count;

            }));

            IList<MWArray> grids = new List<MWArray>(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                grids.Add(new MWNumericArray());
            }

            object addLock = new object();


            try
            {
                _dataView.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _messageBox.UpdateText("Interpolating");
                }));


                object mylock = new object();
                int sum = 0;


                Parallel.For(0, list.Count, (i) =>
                {


                    var vertic = list[i].PointsFloat;
                    MWArray grid = interpolation.Interpolate(vertic, SelectedStep, SelectedMethod.ToString());

                    lock (addLock)
                    {
                        grids[i] = grid;
                    }


                    _dataView.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        sum++;

                        if (i > _messageBox.ProgressBar.Value)
                            _messageBox.UpdateProgressBar(sum);
                    }));
                });

                IList<MWArray> gridList = grids.ToList();
                Record.WriteInterpolatedHeader(gridList);

                _dataView.Dispatcher.BeginInvoke((Action)(() =>
                {
                    sum = 0;
                    _messageBox.UpdateText("Creating storage");
                    _messageBox.UpdateProgressBar(0);
                    _messageBox.ProgressBar.Maximum = gridList.Count;
                }));

                Parallel.For(0, gridList.Count, (i) =>
                {
                    Record.WriteInterpolateData(list[i].Date, gridList[i], i);

                    _dataView.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        sum++;
                        if (i > _messageBox.ProgressBar.Value)
                            _messageBox.UpdateProgressBar(sum);

                    }));

                });

            }
            catch (Exception)
            {
                throw;
            }


            _dataView.Dispatcher.BeginInvoke((Action)(() =>
            {
                _messageBox.Close(true);
            }));

            Record.ReconnectStorage(name);
            UpdateDataInterp();


            _dataView.Dispatcher.BeginInvoke((Action)(() =>
            {
                string storageName = GetInterpolationStorageName(name);
                ExistingInterpolations.Add(storageName);
                SelectedExistingInterpolation = storageName;
                IndexCommand.RaiseCanExecuteChanged();
            }));


        }

        private bool CanInterpolate()
        {
            bool can = true;
            bool method = SelectedInterpolation == InterpolationMethod.None ? false : true;

            if (method && SelectedInterpolation == InterpolationMethod.ScatteredData && SelectedMethod == ScatteredInterpolationMethod.None)
                can = false;

            return (can && method);
        }

        #endregion

        #region CalculateIndex
        private CommandHandler _indexCommand;
        public CommandHandler IndexCommand
        {
            get
            {
                if (_indexCommand == null)
                {
                    _indexCommand = new CommandHandler(
                        param => Task.Factory.StartNew(() => CalculateIndex()),
                        param => CanCalculateIndex()
                    );
                }
                return _indexCommand;
            }
        }

        private void CalculateIndex()
        {
            _dataView.Dispatcher.BeginInvoke((Action)(() =>
            {
                _messageBox = new MessageBoxFS("", "Calculating index");
                _messageBox.ShowDialog();

            }));


            double step = SelectedStep;

            Dictionary<string, List<double[]>> indexes = new Dictionary<string, List<double[]>>();
            IList<Task> calculate = new List<Task>();

            foreach (var key in RecordExercises.Keys)
            {
                if (RecordExercises[key].Count < 1) continue;

                indexes.Add(key.Name, new List<double[]>());

                foreach (var item in RecordExercises[key])
                {
                    var start = item[0];
                    var end = item[1];

                    var frameStart = Record.GetActualFrameFromTime(start.TotalMilliseconds);
                    var frameEnd = Record.GetActualFrameFromTime(end.TotalMilliseconds);

                    ObservableCollection<RecordData> list = new ObservableCollection<RecordData>();

                    Record.ReadInterpolatedData(ref list, frameStart, frameEnd);

                    foreach (var points in list)
                    {
                        points.PointsDouble.Freeze();
                    }

                    calculate.Add(Task.Factory.StartNew(() => CalculateIndexes(key.Name, list, indexes, step)));

                }

            }

            Task.WaitAll(calculate.ToArray());

            string where = string.Format("examinationID = {0} and step={1} and interpolation='{2}'", Examination.ID, step.ToString(System.Globalization.CultureInfo.InvariantCulture), SelectedMethod.ToString());
            var ds = DBAdapter.SelectL("examination_analysis", null, where);


            bool delete = false;

            if (ds[0].Count != 0)
                delete = true;

            DBAdapter.Transaction((transaction) =>
            {
                //if exam exist DELETE
                if (delete)
                {
                    DBAdapter.Delete("examination_analysis", where, transaction);
                }

                //continue create

                foreach (var exercise in indexes.Keys)
                {
                    try
                    {
                        var AVGeye = indexes[exercise].Average(x => x[0]);
                        var AVGmouth = indexes[exercise].Average(x => x[1]);
                        var AVGcheek = indexes[exercise].Average(x => x[2]);

                        var MINeye = indexes[exercise].Min(x => x[0]);
                        var MINmouth = indexes[exercise].Min(x => x[1]);
                        var MINcheek = indexes[exercise].Min(x => x[2]);

                        var MAXeye = indexes[exercise].Max(x => x[0]);
                        var MAXmouth = indexes[exercise].Max(x => x[1]);
                        var MAXcheek = indexes[exercise].Max(x => x[2]);

                        var VAReye = MathNet.Numerics.Statistics.Statistics.Variance(indexes[exercise].Select(x => x[0]));
                        var VARmouth = MathNet.Numerics.Statistics.Statistics.Variance(indexes[exercise].Select(x => x[1]));
                        var VARcheek = MathNet.Numerics.Statistics.Statistics.Variance(indexes[exercise].Select(x => x[2]));

                        var STDeye = MathNet.Numerics.Statistics.Statistics.StandardDeviation(indexes[exercise].Select(x => x[0]));
                        var STDmouth = MathNet.Numerics.Statistics.Statistics.StandardDeviation(indexes[exercise].Select(x => x[1]));
                        var STDcheek = MathNet.Numerics.Statistics.Statistics.StandardDeviation(indexes[exercise].Select(x => x[2]));

                        var MEDeye = MathNet.Numerics.Statistics.Statistics.Median(indexes[exercise].Select(x => x[0]));
                        var MEDmouth = MathNet.Numerics.Statistics.Statistics.Median(indexes[exercise].Select(x => x[1]));
                        var MEDcheek = MathNet.Numerics.Statistics.Statistics.Median(indexes[exercise].Select(x => x[2]));

                        string description = RecordExercises.Where(x => x.Key.Name == exercise).Select(y => y.Key.Description).First();
                        string[] columns = { "examinationID", "interpolation", "step", "exercise", "count", "description" };
                        string[] values = { Examination.ID.ToString(), SelectedMethod.ToString(), step.ToString(System.Globalization.CultureInfo.InvariantCulture), exercise, indexes[exercise].Count.ToString(), description };
                        DBAdapter.Insert("examination_analysis", columns, values, transaction);

                        var analysisID = DBAdapter.LastInsertID(transaction);

                        columns = new string[] { "examinationAnalysisID", "area", "mean", "max", "min", "median", "variance", "std_dev" };
                        values = new string[] { analysisID.ToString(), "eye",
                        AVGeye.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MAXeye.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MINeye.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MEDeye.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        VAReye.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        STDeye.ToString(System.Globalization.CultureInfo.InvariantCulture) };
                        DBAdapter.Insert("analysis_exercise", columns, values, transaction);

                        values = new string[] { analysisID.ToString(), "mouth",
                        AVGmouth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MAXmouth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MINmouth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MEDmouth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        VARmouth.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        STDmouth.ToString(System.Globalization.CultureInfo.InvariantCulture) };
                        DBAdapter.Insert("analysis_exercise", columns, values, transaction);

                        values = new string[] { analysisID.ToString(), "cheek",
                        AVGcheek.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MAXcheek.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MINcheek.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        MEDcheek.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        VARcheek.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        STDcheek.ToString(System.Globalization.CultureInfo.InvariantCulture) };
                        DBAdapter.Insert("analysis_exercise", columns, values, transaction);
                    }
                    catch (Exception ex)
                    {
                        new MessageBoxFS("Error", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
                    }
                }

            });

            _dataView.Dispatcher.BeginInvoke((Action)(() =>
            {
                _messageBox.Close(true);

            }));

            OnIndexCalculate();
        }

        public delegate void IndexHandler();
        public event IndexHandler OnIndexCalculate;
        object _index = new object();
        object _copy = new object();

        private void CalculateIndexes(string exercise, ObservableCollection<RecordData> data, Dictionary<string, List<double[]>> indexes, double step)
        {
            int i = -1;
            foreach (var frame in data)
            {
                i++;

                var lengthX = frame.LengthX;
                var lengthY = frame.LengthY;



                double[,] array = new double[lengthX, lengthY];

                foreach (var item in frame.PointsDouble)
                {
                    array[(int)item.X, (int)item.Y] = item.Z;
                }




                MWArray matrix = new MWNumericArray(array);

                //MHelper.MHelper help = new MHelper.MHelper();
                //help.Save(matrix, "F:\\Development\\V4.mat");

                try
                {

                    SymmetryIndex.SymmetryIndex si = new SymmetryIndex.SymmetryIndex();
                    MWArray results = si.CalculateSymmetryIndex(matrix, step);

                    Array resultsArray = results.ToArray();
                    double[] ind = new double[] { (double)resultsArray.GetValue(0, 0), (double)resultsArray.GetValue(0, 1), (double)resultsArray.GetValue(0, 2) };

                    lock (_index)
                        indexes[exercise].Add(ind);
                }
                catch (Exception)
                {
                    // do nothing, skip
                }
            }



        }

        private bool CanCalculateIndex()
        {
            return IsInterpolationCreated();
        }

        #endregion

    }
}
