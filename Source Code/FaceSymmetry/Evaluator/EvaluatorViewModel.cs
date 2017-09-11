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
using System.Linq;
using System.Windows.Input;
using static Evaluator.DataViewModel;

namespace Evaluator
{
    public interface IViewModel : IDisposable
    {
        object Content { get; set; }
        string Name { get; }
       
    }


    public partial class EvaluatorViewModel : ObservableObject, IDisposable
    {

        private EvaluatorWindow _window;
        private RecordLocation _recordLocation;
        private IViewModel _currentViewModel;
        private List<IViewModel> _viewModels;

        public event EventHandler OnClosing;

        public List<IViewModel> ViewModels
        {
            get
            {
                if (_viewModels == null)
                    _viewModels = new List<IViewModel>();

                return _viewModels;
            }
        }

        public IViewModel CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged("CurrentViewModel");
                }
            }
        }

        internal void Focus()
        {
            _window.Focus();
        }

        private Examination _examination;
        public Examination Examination { get { return _examination; } }

        public EvaluatorViewModel(Patient selectedPatient, Examination selectedExamination)
        {
            _recordLocation = selectedExamination.RecordLocation;

            if (_recordLocation == RecordLocation.Local)
            {
                DataViewModel dataVM = new DataViewModel(selectedExamination);
                dataVM.OnIndexCalculate += new IndexHandler(AddIndex);
                ViewModels.Add(dataVM);
            }

            _examination = selectedExamination;

            _window = new EvaluatorWindow();
            _window.DataContext = this;

            _window.Closing += _window_Closing;
            _window.KeyDown += _window_KeyDown;            

            ViewModels.Add(new StatisticsViewModel(selectedPatient, selectedExamination));

            CurrentViewModel = ViewModels[0];

            _window.Show();
        }

        private void AddIndex()
        {
            StatisticsViewModel VM = (StatisticsViewModel)GetViewModel<StatisticsViewModel>();
            VM.Init();
        }

        private void _window_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentViewModel.Name == "DataViewModel")
            {
                DataViewModel dataVM = CurrentViewModel as DataViewModel;
                bool isPlaying = dataVM.IsPlaying;

                switch (e.Key)
                {
                    case Key.Right:
                        if (!isPlaying)
                            dataVM.SliderAddValue(50);
                        break;

                    case Key.Left:
                        if (!isPlaying)
                            dataVM.SliderAddValue(-50);
                        break;
                    case Key.Space:
                        if (isPlaying)
                            dataVM.Pause();
                        else
                            dataVM.Play();
                        break;
                }
            }
        }



        private void _window_Closing(object sender, CancelEventArgs e)
        {
            DataViewModel dataVM = (DataViewModel)GetViewModel<DataViewModel>();

            if (dataVM != null)
            {
                while (dataVM.IsTimerEnabled)
                {
                    dataVM.Pause();
                    System.Threading.Thread.Sleep(100);
                }
            }

            Dispose();

            EventHandler handler = OnClosing;
            handler(this, new EventArgs());
        }

        public void Close()
        {
            _window.Close();
        }

        private IViewModel GetViewModel<T>()
        {
            return ViewModels.SingleOrDefault(x => x.GetType() == typeof(T));
        }

        private CommandHandler _viewDataCommand;
        public CommandHandler ViewDataCommand
        {
            get
            {
                if (_viewDataCommand == null)
                {
                    _viewDataCommand = new CommandHandler(
                        param => ChangeViewModel((DataViewModel)GetViewModel<DataViewModel>()),
                        param => CanChangeDataViewModel()
                    );
                }
                return _viewDataCommand;
            }
        }

        private bool CanChangeDataViewModel()
        {
            return _recordLocation == RecordLocation.Local ? true : false;
        }

        private CommandHandler _viewStatisticsCommand;
        public CommandHandler ViewStatisticsCommand
        {
            get
            {
                if (_viewStatisticsCommand == null)
                {
                    _viewStatisticsCommand = new CommandHandler(
                        param => ChangeViewModel((StatisticsViewModel)GetViewModel<StatisticsViewModel>()),
                        null
                    );
                }
                return _viewStatisticsCommand;
            }
        }
        

        private void ChangeViewModel(IViewModel viewModel)
        {
            if (CurrentViewModel.Name == "DataViewModel")
            {
                DataViewModel dataVM = (DataViewModel)GetViewModel<DataViewModel>();

                if (dataVM.IsTimerEnabled)
                    dataVM.Pause();
            }

            if (!ViewModels.Contains(viewModel))
                ViewModels.Add(viewModel);

            CurrentViewModel = ViewModels.FirstOrDefault(vm => vm == viewModel);

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public bool Disposed { get { return disposedValue; } }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in ViewModels)
                    {
                        item.Dispose();
                    }

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
