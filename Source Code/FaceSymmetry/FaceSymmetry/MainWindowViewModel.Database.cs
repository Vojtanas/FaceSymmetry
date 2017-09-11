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
using System.Data;
using System.IO;
using System.Linq;

namespace FaceSymmetry
{
    public partial class MainWindowViewModel
    {
        private void LoadPatients()
        {
            string[] what = { "patientID", "surname", "first_name", "second_name", "personal_id", "date_of_birth", "gender", "notes" };
            var ds = DBAdapter.Select("patient", what);

            if (ds != null)
                Patients = PatientFromDBtoList(ds);

        }

        private void LoadExaminations(int patientID)
        {
            string[] what = { "examination.examinationID", "created", "guid", "dir", "notes", "exercises" };
            string where = string.Format("patientID = {0}", patientID);

            var ds = DBAdapter.Select("examination", what, where);

            if (ds != null)
                Examinations = ExaminationFromDBtoList(ds);
        }

        private List<Examination> ExaminationFromDBtoList(DataSet examinationDB)
        {
            List<Examination> examinations = new List<Examination>();

            var patientTable = examinationDB.Tables[0];

            foreach (DataRow examination in patientTable.Rows)
            {
                if (string.IsNullOrEmpty(examination.ItemArray[0].ToString()))
                    continue;

                string recordDir = examination.ItemArray[3].ToString();
                RecordLocation recordLocation = GetRecordLocation(recordDir);

                examinations.Add(new Examination(
                    examination.ItemArray[0].ToString(),
                    examination.ItemArray[1].ToString(),
                    examination.ItemArray[2].ToString(),
                    examination.ItemArray[3].ToString(),
                     recordLocation,
                     examination.ItemArray[4].ToString(),
                     examination.ItemArray[5].ToString()
                    ));
            }

            return examinations;
        }

        private RecordLocation GetRecordLocation(string recordDir)
        {
            if (string.IsNullOrWhiteSpace(recordDir))
                return RecordLocation.Distinct;


            recordDir = Settings.SavePathDir + "\\" + recordDir;

            if (!Directory.Exists(recordDir))
                return RecordLocation.Distinct;

            var binaryFile = Directory.EnumerateFiles(recordDir, "*.bin", SearchOption.AllDirectories).FirstOrDefault();

            if (binaryFile != null)
            {
                return RecordLocation.Local;
            }
            else
            {
                return RecordLocation.Distinct;
            }
        }

        private List<Patient> PatientFromDBtoList(DataSet patientsDB)
        {
            List<Patient> patients = new List<Patient>();

            var patientTable = patientsDB.Tables[0];

            foreach (DataRow patient in patientTable.Rows)
            {
                string dateOfBirth = patient.ItemArray[5].ToString().Split(' ')[0];

                patients.Add(new Patient(
                    patient.ItemArray[0].ToString(),
                    patient.ItemArray[1].ToString(),
                    patient.ItemArray[2].ToString(),
                    patient.ItemArray[3].ToString(),
                    patient.ItemArray[4].ToString(),
                    dateOfBirth,
                    patient.ItemArray[6].ToString(),
                    patient.ItemArray[7].ToString()
                    ));
            }

            //public Patient(string id, string surname, string firstName, string pid,string birthday,string gender,string notes)

            return patients;
        }

        internal void UpdatePatientToDB()
        {
            int patientIndex = View.patientGrid.Items.IndexOf(SelectedPatient);

            string[] values = SelectedPatient.ToArray();

            if (SelectedPatient != null)
            {
                string[] columns = {
                "first_name",
                "second_name",
                "surname",
                "date_of_birth",
                "personal_id",
                "gender",
                "notes"};
                string where = string.Format("patientID = {0}", SelectedPatient.ID);

                DateTime date;
                if (!DateTime.TryParse(values[3], out date))
                {
                    values = values.RemoveAt(3);
                    columns = columns.RemoveAt(3);
                }
                else
                {
                    values[3] = date.ToString("yyyy-MM-dd HH:mm");
                }

                DBAdapter.Update("patient", columns, values, where);

                LoadPatients();
                View.UpdatePatientGrid();
                View.patientGrid.SelectedIndex = patientIndex;
                View.patientGrid.Focus();
            }
        }

        internal void DeletePatient(int id)
        {
            string where = string.Format("patientID = {0}", id.ToString());
            DBAdapter.Delete("patient", where);
        }

        internal void DeleteExamination(int id)
        {
            string where = string.Format("examinationID = '{0}'", id.ToString());
            DBAdapter.Delete("examination", where);
        }

        internal bool InsertPatient(string[] patientValues)
        {
            string[] column = {
            "first_name",
                "second_name",
                "surname",
                "date_of_birth",
                "personal_id",
                "gender",
                "notes"};

            if (DBAdapter.Insert("patient", column, patientValues))
            {
                LoadPatients();
                View.UpdatePatientGrid();
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
