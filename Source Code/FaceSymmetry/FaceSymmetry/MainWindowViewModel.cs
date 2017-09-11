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
using System.Data;
using System.Linq;

namespace FaceSymmetry
{
    public partial class MainWindowViewModel : ObservableObject, INotifyPropertyChanged
    {
        public MainWindow View;

        public ICollectionView PatientData;

        public List<Patient> Patients { get; set; }
        public List<Examination> Examinations { get; set; }

        private Patient _selectedPatient;
        public Patient SelectedPatient
        {
            get { return _selectedPatient; }
            set
            {
                _selectedPatient = value;
                OnPropertyChanged("SelectedPatient");
            }
        }

        private Examination _selectedExamination;

        public Examination SelectedExamination
        {
            get { return _selectedExamination; }
            set
            {
                _selectedExamination = value;
                OnPropertyChanged("SelectedExamination");
            }
        }

        public MainWindowViewModel()
        {
            this.PropertyChanged += MainWindowModelView_PropertyChanged;

            Examinations = new List<Examination>();
            View = new MainWindow(this);
            View.Show();

        }

        private void MainWindowModelView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedPatient":
                    if (SelectedPatient != null)
                    {
                        LoadExaminations(SelectedPatient.ID);
                    }
                    else
                    {
                        SelectedExamination = null;
                        Examinations = null;
                    }

                    View.PatientChanged();

                    View.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ImportExaminationCommand.RaiseCanExecuteChanged();
                        NewRecordCommand.RaiseCanExecuteChanged();
                        DeletePatientCommand.RaiseCanExecuteChanged();
                        UpdatePatientToDBCommand.RaiseCanExecuteChanged();

                    }));
                    break;

                case "SelectedExamination":
                    View.ExaminationChanged();
                    View.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        OpenExaminationCommand.RaiseCanExecuteChanged();
                        DeleteExaminationCommand.RaiseCanExecuteChanged();
                        OpenExaminationFolderCommand.RaiseCanExecuteChanged();

                    }));
                    break;
                default:
                    break;
            }
        }

        public void LoadPatientsFromDB()
        {
            SelectedPatient = null;
            LoadPatients();
        }

        private void FocusNewestExamination()
        {
            if (Examinations.Count > 0)
            {
                SelectedExamination = Examinations.OrderByDescending(x => x.Date).First();
                View.examinationGrid.SelectedIndex = View.examinationGrid.Items.IndexOf(SelectedExamination);
                View.examinationGrid.Focus();
            }
        }

        private void FocusNewestPatient()
        {
            if (Patients.Count > 0)
            {
                SelectedPatient = Patients.OrderByDescending(x => x.ID).First();
                View.patientGrid.SelectedIndex = View.patientGrid.Items.IndexOf(SelectedPatient);
                View.patientGrid.Focus();
            }
        }

        public void Close()
        {
            try
            {
                foreach (var item in EvaluatorList)
                {
                    if (item != null && !item.Disposed)
                    {
                        item.Close();
                    }
                }
            }
            catch (Exception) { }
        }


    }
}

