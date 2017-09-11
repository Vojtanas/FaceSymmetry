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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Evaluator
{
    public class StatisticsViewModel : ObservableObject, IViewModel
    {

        private StatisticsView _view;
        private De.TorstenMandelkow.MetroChart.ClusteredBarChart _barGraph;
        private IList<string> _listGraph = new List<string> { "Mean", "Max", "Min", "Median", "Variance", "Standard Deviation" };


        public Patient SelectedPatient { get; private set; }
        public Examination SelectedExamination { get; private set; }


        private string _selectedInterpolation;
        public string SelectedInterpolation
        {
            get { return _selectedInterpolation; }
            set
            {
                if (_selectedInterpolation != value)
                {
                    _selectedInterpolation = value;
                    OnPropertyChanged("SelectedInterpolation");
                }
            }
        }

        private string _selectedGraph;
        public string SelectedGraph
        {
            get { return _selectedGraph; }
            set
            {
                _selectedGraph = value;
                OnPropertyChanged("SelectedGraph");
            }
        }

        public object Content { get; set; }
        public string Name { get { return this.GetType().Name; } }

        public StatisticsViewModel(Patient selectedPatient, Examination selectedExamination)
        {
            Content = _view = new StatisticsView();
            _view.DataContext = this;
            SelectedPatient = selectedPatient;
            SelectedExamination = selectedExamination;
            PropertyChanged += StatisticsViewModel_PropertyChanged; ;

            Init();
        }

        private void StatisticsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedInterpolation":
                    string interpolation = SelectedInterpolation.Split(' ')[0];
                    if (interpolation == string.Empty)
                    {
                        HideData();
                        break;
                    }

                    string step = SelectedInterpolation.Split(' ')[1];
                    SetAnalysis(interpolation, step);
                    break;

                case "SelectedGraph":
                    SetGraph(SelectedGraph);
                    break;
                default:
                    break;
            }
        }

        private void HideData()
        {
            _view.gridDescription.Visibility = Visibility.Hidden;
            _view.gridExercise.Visibility = Visibility.Hidden;
            RemoveBarGraph();
        }

        public void Init()
        {
            string table = "examination_analysis";
            string[] what = { " analysisID", "step", "interpolation" };
            string where = string.Format("examinationID = {0} group by interpolation,step", SelectedExamination.ID);
            var result = DBAdapter.SelectL(table, what, where);


            IList<string> listAnalysis = new List<string> { string.Empty };

            for (int i = 0; i < result[0].Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(result[2][i] + " ");
                sb.Append(result[1][i].Replace(',', '.').TrimEnd('0'));

                listAnalysis.Add(sb.ToString());
            }

            if (listAnalysis != null && listAnalysis.Count != 0)
                _view.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _view.analysisCmbBx.ItemsSource = listAnalysis;
                }));

        }

        private IList<AnalysisExercise> _analysis;

        private void SetAnalysis(string interpolation, string step)
        {

            string table = "examination_analysis";
            string[] what = { "exercise", "area", "mean", "max", "min", "median", "variance", "std_dev", "count", "description" };
            string where = string.Format("examinationID = {0} and step = {1} and interpolation='{2}'", SelectedExamination.ID, step, interpolation);
            string join = "join analysis_exercise on examination_analysis.analysisID = analysis_exercise.examinationAnalysisID";

            var result = DBAdapter.SelectL(table, what, where, join);

            _analysis = new List<AnalysisExercise>();

            for (int i = 0; i < result[0].Count; i++)
            {
                _analysis.Add(new AnalysisExercise(
                    result[0][i],
                    result[1][i],
                    result[2][i],
                    result[3][i],
                    result[4][i],
                    result[5][i],
                    result[6][i],
                    result[7][i],
                    result[8][i],
                    result[9][i]));
            }


            _view.graphCmbBx.ItemsSource = _listGraph;
            _view.gridDescription.Visibility = Visibility.Visible;
            _view.gridExercise.Visibility = Visibility.Visible;

            CreateDescriptionGrid(_analysis);
            CreateExerciseGrid(_analysis);
            CreateGraph(_analysis);
            SelectedGraph = _listGraph[0];
        }

        private void CreateDescriptionGrid(IList<AnalysisExercise> analysis)
        {
            _view.gridDescription.ItemsSource = analysis.GroupBy(x => x.Exercise);
        }

        private void CreateExerciseGrid(IList<AnalysisExercise> analysis)
        {

            ListCollectionView exercise = new ListCollectionView(analysis.ToList());
            exercise.GroupDescriptions.Add(new PropertyGroupDescription("ExerciseAndCount"));

            DataGrid grid = _view.gridExercise;
            grid.ItemsSource = exercise;

            Style style = (Style)_view.FindResource("GroupHeaderStyle");
            GroupStyle groupStyle = new GroupStyle();
            groupStyle.ContainerStyle = style;

            grid.GroupStyle.Add(groupStyle);
        }

        private void RemoveBarGraph()
        {
            _view.tableStackPanel.Children.Remove(_barGraph);
        }

        private void CreateGraph(IList<AnalysisExercise> analysis)
        {
            double childrenWidth = 0;

            if (_barGraph != null)
            {
                RemoveBarGraph();
            }

            foreach (DataGrid item in _view.tableStackPanel.Children)
            {
                childrenWidth += item.ActualWidth + item.Margin.Left + item.Margin.Right;
            }


            _barGraph = new De.TorstenMandelkow.MetroChart.ClusteredBarChart();
            _barGraph.Width = _view.tableStackPanel.ActualWidth - childrenWidth - 100;
            _barGraph.Height = _view.tableStackPanel.ActualHeight;
            _barGraph.HorizontalAlignment = HorizontalAlignment.Left;
            _barGraph.Margin = new Thickness(0, 0, 10, 10);
            _barGraph.VerticalAlignment = VerticalAlignment.Top;
            _barGraph.ChartTitleVisibility = Visibility.Visible;
            _barGraph.ChartSubTitle = "";


            foreach (var exercise in _analysis.Select(x => x.Exercise).Distinct())
            {
                _barGraph.Series.Add(

               new De.TorstenMandelkow.MetroChart.ChartSeries
               {
                   SeriesTitle = exercise,
                   DisplayMember = "Area",
                   ValueMember = SelectedGraph,
                   ItemsSource = _analysis.Where(x => x.Exercise == exercise).ToList()
               });
            }

            _view.tableStackPanel.Children.Add(_barGraph);
        }




        private void SetGraph(string selectedValue)
        {
            if (_barGraph == null) return;

            _barGraph.ChartTitle = string.Format("Exercise {0} Values", selectedValue);

            if (selectedValue == "Standard Deviation")
                selectedValue = "StdDev";

            int i = 0;
            foreach (var exercise in _analysis.Select(x => x.Exercise).Distinct())
            {
                _barGraph.Series[i].ValueMember = selectedValue;
                _barGraph.Series[i].ItemsSource = _analysis.Where(x => x.Exercise == exercise).ToList();

                i++;
            }
        }




        private DataGrid CreateGrid()
        {
            DataGrid grid = new DataGrid();
            grid.Margin = new Thickness(10);
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Top;
            grid.GridLinesVisibility = DataGridGridLinesVisibility.All;
            grid.BorderThickness = new Thickness(0);
            grid.IsReadOnly = true;
            grid.AutoGenerateColumns = false;
            grid.SelectionMode = DataGridSelectionMode.Single;
            grid.SelectionUnit = DataGridSelectionUnit.FullRow;

            return grid;

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

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StatisticsViewModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
