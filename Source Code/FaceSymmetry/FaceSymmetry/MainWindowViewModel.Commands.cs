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
using Evaluator;
using Recorder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FaceSymmetry
{
    public partial class MainWindowViewModel
    {
        #region OpenExamination

        public IList<EvaluatorViewModel> EvaluatorList = new List<EvaluatorViewModel>();

        private CommandHandler _openExaminationCommand;
        public CommandHandler OpenExaminationCommand
        {
            get
            {
                if (_openExaminationCommand == null)
                {
                    _openExaminationCommand = new CommandHandler(
                        param => OpenExamination(),
                        param => CanOpenExamination()
                    );
                }
                return _openExaminationCommand;
            }
        }

        private bool CanOpenExamination()
        {
            return SelectedExamination != null ? true : false;
        }

        internal void OpenExamination()
        {
            foreach (var item in EvaluatorList)
            {
                if (SelectedExamination.ID == item.Examination.ID)
                {
                    item.Focus();
                    return;
                }
            }

            try
            {
                EvaluatorViewModel evm = new EvaluatorViewModel(SelectedPatient, SelectedExamination);
                evm.OnClosing += EvaluatorVM_OnClosing;
                EvaluatorList.Add(evm);
            }
            catch (Exception ex)
            {
                StringBuilder message = new StringBuilder();
                if (ex.Message.Contains("Kinect"))
                {
                    message.Append("Check if MS Kinect SDK 2.0 is installed. \n");
                }

                message.Append(ex.Message);

                new MessageBoxFS("Error", message.ToString(), MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }
        }

        private void EvaluatorVM_OnClosing(object sender, EventArgs e)
        {
            EvaluatorViewModel evm = sender as EvaluatorViewModel;
            EvaluatorList.Remove(evm);
        }


        #endregion

        #region DeleteExamination
        private CommandHandler _deleteExaminationCommand;
        public CommandHandler DeleteExaminationCommand
        {
            get
            {
                if (_deleteExaminationCommand == null)
                {
                    _deleteExaminationCommand = new CommandHandler(
                        param => DeleteExamination(),
                        param => CanDeleteExamination()
                    );
                }
                return _deleteExaminationCommand;
            }
        }

        private bool CanDeleteExamination()
        {
            return SelectedExamination != null ? true : false;
        }

        private void DeleteExamination()
        {
            string message = string.Format("Do you wish to delete examination?");
            MessageBoxFS delete = new MessageBoxFS("Warning", message, MessageBoxFSButton.YESNO, MessageBoxFSImage.Question);
            bool? deleteResult = delete.ShowDialog();

            if (deleteResult ?? true)
            {
                string path = Settings.SavePathDir + "\\" + SelectedExamination.Dir;

                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                DeleteExamination(SelectedExamination.ID);
            }

            LoadExaminations(SelectedPatient.ID);
            View.ExaminationChanged();
            FocusNewestExamination();
        }

        #endregion

        #region ImportExamination
        private CommandHandler _importExaminationCommand;
        public CommandHandler ImportExaminationCommand
        {
            get
            {
                if (_importExaminationCommand == null)
                {
                    _importExaminationCommand = new CommandHandler(
                        param => ImportExamination(),
                        param => CanImportExamination()
                    );
                }
                return _importExaminationCommand;
            }
        }

        private bool CanImportExamination()
        {
            return SelectedPatient != null ? true : false;
        }

        private void ImportExamination()
        {
            ImportWindow import = new ImportWindow(SelectedPatient.ID);
            import.ShowDialog();
            import = null;
            LoadExaminations(SelectedPatient.ID);
            View.ExaminationChanged();

        }



        #endregion

        #region OpenExaminationFolder

        private CommandHandler _openExaminationFolderCommand;
        public CommandHandler OpenExaminationFolderCommand
        {
            get
            {
                if (_openExaminationFolderCommand == null)
                {
                    _openExaminationFolderCommand = new CommandHandler(
                        param => OpenExaminationFolder(),
                        param => CanOpenExaminationFolder()
                    );
                }
                return _openExaminationFolderCommand;
            }
        }

        private bool CanOpenExaminationFolder()
        {
            bool can = false;
            if (SelectedExamination != null)
            {
                can = SelectedExamination.RecordLocation == RecordLocation.Local ? true : false;
            }

            return can;
        }

        internal void OpenExaminationFolder()
        {
            string dir = Settings.SavePathDir + "\\" + SelectedExamination.Dir;
            try
            {
                Process.Start(dir);
            }
            catch (Exception ex)
            {
                new MessageBoxFS("Error", string.Format(ex.Message + "\n{0}", dir), MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }
        }

        #endregion

        #region DeletePatient
        private CommandHandler _deletePatientCommand;
        public CommandHandler DeletePatientCommand
        {
            get
            {
                if (_deletePatientCommand == null)
                {
                    _deletePatientCommand = new CommandHandler(
                        param => DeletePatient(),
                        param => CanDeletePatient()
                    );
                }
                return _deletePatientCommand;
            }
        }

        private bool CanDeletePatient()
        {
            return SelectedPatient != null ? true : false;
        }

        private void DeletePatient()
        {
            string message = string.Format("Do you wish to delete patient \"{0}\" {1}", SelectedPatient.Surname, SelectedPatient.PID);
            MessageBoxFS deletePatient = new MessageBoxFS("Warning", message, MessageBoxFSButton.YESNO, MessageBoxFSImage.Question);
            bool? deleteResult = deletePatient.ShowDialog();


            if (deleteResult ?? true)
            {
                if (Examinations.Count > 0)
                {
                    MessageBoxFS deleteFiles = new MessageBoxFS("Warning", "Do you wish to delete all local files?", MessageBoxFSButton.YESNO, MessageBoxFSImage.Question);
                    var filesResult = (bool)deleteFiles.ShowDialog();

                    //TODO delete files
                    if (filesResult == true)
                    {
                        foreach (var item in Examinations)
                        {
                            string dir = Settings.SavePathDir + "\\" + item.Dir;
                            if (Directory.Exists(dir))
                                Directory.Delete(dir, true);
                        }
                    }

                }

                DeletePatient(SelectedPatient.ID);
                LoadPatients();
                ReloadPatientsGrid();
            }
        }

        #endregion

        #region NewPatient
        private CommandHandler _newPatientCommand;
        public CommandHandler NewPatientCommand
        {
            get
            {
                if (_newPatientCommand == null)
                {
                    _newPatientCommand = new CommandHandler(
                        param => NewPatient(), null
                    );
                }
                return _newPatientCommand;
            }
        }

        internal void NewPatient()
        {
            try
            {
                NewPatient patient = new NewPatient(this);
                patient.ShowDialog();
                patient = null;
                FocusNewestPatient();

            }
            catch (Exception ex)
            {
                new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
            }

        }

        #endregion

        #region NewRecord
        private CommandHandler _newRecordCommand;
        public CommandHandler NewRecordCommand
        {
            get
            {
                if (_newRecordCommand == null)
                {
                    _newRecordCommand = new CommandHandler(
                        param => OpenRecorder(),
                        param => CanOpenRecorder()
                    );
                }
                return _newRecordCommand;
            }
        }

        private bool CanOpenRecorder()
        {
            return SelectedPatient != null ? true : false;
        }

        internal void OpenRecorder()
        {
            try
            {
                new RecorderViewModel(SelectedPatient, _emulator);
            }
            catch (Exception ex)
            {
                StringBuilder message = new StringBuilder();
                if (ex.Message.Contains("Kinect"))
                {
                    message.Append("Check if MS Kinect SDK 2.0 is installed. \n");
                }

                message.Append(ex.Message);

                new MessageBoxFS("Error", message.ToString(), MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }
            finally
            {
                LoadExaminations(SelectedPatient.ID);
                FocusNewestExamination();
            }
        }

        #endregion

        #region ExerciseSettings
        private CommandHandler _exerciseSettingsCommand;
        public CommandHandler ExerciseSettingsCommand
        {
            get
            {
                if (_exerciseSettingsCommand == null)
                {
                    _exerciseSettingsCommand = new CommandHandler(
                        param => OpenExerciseSettings(),
                        param => CanExerciseSettingsCommand()
                    );
                }
                return _exerciseSettingsCommand;
            }
        }

        #region EmulatorCommand
        private CommandHandler _emulatorCommand;
        public CommandHandler EmulatorCommand
        {
            get
            {
                if (_emulatorCommand == null)
                {
                    _emulatorCommand = new CommandHandler(
                        param => SetEmulator(),
                        param => CanSetEmulator()
                    );
                }
                return _emulatorCommand;
            }
        }

        private bool _emulator = false;
        private void SetEmulator()
        {
            _emulator = !_emulator;
            if (_emulator)
                View.EmulatorSettings.InputGestureText = "ON";
            else
                View.EmulatorSettings.InputGestureText = "OFF";
        }

        private bool CanSetEmulator()
        {
            return File.Exists(Settings.SavePathDir + "\\Emulator\\Emulator.bin");
        }
        #endregion

        private bool CanExerciseSettingsCommand()
        {
            return true;
        }

        internal void OpenExerciseSettings()
        {
            try
            {
                SettingsExercisesViewModel settingsExercises = new SettingsExercisesViewModel();
                settingsExercises.ShowDialog();

            }
            catch (DataBaseException ex)
            {
                new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
            }

        }

        #endregion

        #region DatabaseSettings
        private CommandHandler _databaseSettingsCommand;
        public CommandHandler DatabaseSettingsCommand
        {
            get
            {
                if (_databaseSettingsCommand == null)
                {
                    _databaseSettingsCommand = new CommandHandler(
                        param => OpenDatabaseSettings(),
                        param => CanDatabaseSettingsCommand()
                    );
                }
                return _databaseSettingsCommand;
            }
        }

        private bool CanDatabaseSettingsCommand()
        {
            return true; // if DB not connected
        }

        private void OpenDatabaseSettings()
        {
            new SettingsDatabase().ShowDialog();
            ReloadPatientsGrid();
        }
        #endregion

        #region GeneralSettingsCommand
        private CommandHandler _generalSettingsCommand;
        public CommandHandler GeneralSettingsCommand
        {
            get
            {
                if (_generalSettingsCommand == null)
                {
                    _generalSettingsCommand = new CommandHandler(
                        param => OpenGeneralSettings(),
                        param => CanGeneralSettingsCommand()
                    );
                }
                return _generalSettingsCommand;
            }
        }

        private bool CanGeneralSettingsCommand()
        {
            return true;
        }

        private void OpenGeneralSettings()
        {
            SettingsGeneralModelView settings = new SettingsGeneralModelView();
        }

        #endregion

        #region ReloadPatientsGridCommand
        private CommandHandler _reloadPatientsGridCommand;
        public CommandHandler ReloadPatientsGridCommand
        {
            get
            {
                if (_reloadPatientsGridCommand == null)
                {
                    _reloadPatientsGridCommand = new CommandHandler(
                        param => ReloadPatientsGrid(),
                        param => CanGeneralSettingsCommand()
                    );
                }
                return _reloadPatientsGridCommand;
            }
        }


        internal void ReloadPatientsGrid()
        {
            SelectedExamination = null;
            SelectedPatient = null;

            try
            {
                LoadPatientsFromDB();
                View.UpdatePatientGrid();
            }
            catch (Exception ex)
            {
                new MessageBoxFS("Error", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
            }

        }

        private bool CanReloadPatientsGridCommand()
        {
            return true; // if DB not connected
        }
        #endregion

        #region UpdatePatientToDB
        private CommandHandler _updatePatientToDBCommand;
        public CommandHandler UpdatePatientToDBCommand
        {
            get
            {
                if (_updatePatientToDBCommand == null)
                {
                    _updatePatientToDBCommand = new CommandHandler(
                        param => UpdatePatientToDB(),
                        param => CanUpdatePatientToDB()
                    );
                }
                return _updatePatientToDBCommand;
            }
        }


        private bool CanUpdatePatientToDB()
        {
            return SelectedPatient != null ? true : false;
        }


        #endregion
    }
}
