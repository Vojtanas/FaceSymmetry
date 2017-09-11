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
using System.Windows.Media;

namespace FaceSymmetry
{
    public class NewPatientViewModel : ObservableObject
    {
        private NewPatient _view;
        private MainWindowViewModel _mainVM;
        private bool _isReset = true;

        private Patient _selectedPatient;
        public Patient SelectedPatient
        {
            get { return _selectedPatient; }
            set
            {
                if (value != _selectedPatient)
                {
                    _selectedPatient = value;
                    OnPropertyChanged("SelectedPatient");
                }
            }
        }


        public NewPatientViewModel(NewPatient view, MainWindowViewModel mainVM)
        {
            _view = view;
            _mainVM = mainVM;
            SelectedPatient = new Patient();
            SelectedPatient.Surname = "Surname";

            _view.patientControl.surnameTxtBox.Foreground = Brushes.Gray;
            _view.patientControl.surnameTxtBox.GotFocus += SurnameTxtBox_GotFocus;
            _view.patientControl.surnameTxtBox.TextChanged += SurnameTxtBox_TextChanged;
        }

        private void SurnameTxtBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }


        private void SurnameTxtBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            _view.patientControl.surnameTxtBox.Text = "";
            _view.patientControl.surnameTxtBox.Foreground = Brushes.Black;
            _view.patientControl.surnameTxtBox.GotFocus -= SurnameTxtBox_GotFocus;
            _isReset = false;
        }


        #region CancelCommand
        private CommandHandler _cancelCommand;
        public CommandHandler CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new CommandHandler(
                        param => Cancel(), null
                    );
                }
                return _cancelCommand;
            }
        }

        private void Cancel()
        {
            _isReset = true;
            SelectedPatient = new Patient();
            SelectedPatient.Surname = "Surname";
            _view.patientControl.surnameTxtBox.Text = "Surname";
            _view.patientControl.surnameTxtBox.Foreground = Brushes.Gray;
            _view.patientControl.surnameTxtBox.GotFocus += SurnameTxtBox_GotFocus;

        }
        #endregion

        #region SaveCommand
        private CommandHandler _saveCommand;
        public CommandHandler SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new CommandHandler(
                        param => Save(),
                        param => CanSave()
                    );
                }
                return _saveCommand;
            }
        }

        private void Save()
        {
            string[] patientValues = SelectedPatient.ToArray();

            if (_mainVM.InsertPatient(patientValues))
            {
                new MessageBoxFS("Success", "New patient was added", MessageBoxFSButton.OK, MessageBoxFSImage.Information).ShowDialog();
                Cancel();
            }
        }

        private bool CanSave()
        {
            bool canSave = false;
            if (!_isReset && !string.IsNullOrEmpty(SelectedPatient?.Surname) && !string.IsNullOrEmpty(_view.patientControl.surnameTxtBox.Text))
                canSave = true;

            return canSave;

        }
        #endregion
    }
}
