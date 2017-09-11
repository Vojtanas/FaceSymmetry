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
using System.Data;
using System.Linq;

namespace FaceSymmetry
{
    public class SettingsExercisesViewModel
    {
        private SettingsExercises _view;
        public ExerciseCollection Exercises;

        public SettingsExercisesViewModel()
        {
            Exercises = DBAdapter.GetExercisesFromDB();
            Exercises.ExerciseChanged += Exercises_ExerciseChanged;
            _view = new SettingsExercises(this);
        }        

        private void Exercises_ExerciseChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Exercises.Single(x => x.Name == e.PropertyName).Changed = true;
        }

        internal void AddExercise(string exerciseName)
        {
            ObservableExercise exercise = new ObservableExercise("", exerciseName, "", true);
            Exercises.Add(exercise);
        }

        internal void RemoveExercise(string itemName)
        {
            Exercises.Remove(itemName);
        }


        internal void Save()
        {
            var deleteList = Exercises.Where(x => x.Deleted == true && x.ID != "");
            var updateList = Exercises.Where(x => x.Changed == true && x.Deleted == false && x.ID != "");
            var insertList = Exercises.Where(x => x.ID == "" && x.Deleted == false);

            string table = "exercise";
            string[] column = { "name", "description", "active" };
            
            DBAdapter.Transaction((transaction) =>
            {
                foreach (var item in deleteList)
                {
                    try
                    {
                        string where = string.Format("excerciseID = {0}", item.ID);
                        DBAdapter.Delete(table, where, transaction);
                    }
                    catch (Exception ex)
                    {

                        new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
                    }
                }

                foreach (var item in insertList)
                {
                    try
                    {
                        string[] values = { item.Name, item.Description, Convert.ToInt32(item.Active).ToString() };
                        DBAdapter.Insert(table, column, values, transaction);

                    }
                    catch (Exception ex)
                    {

                        new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
                    }
                }

                foreach (var item in updateList)
                {
                    try
                    {
                        string[] values = { item.Name, item.Description, Convert.ToInt32(item.Active).ToString() };
                        string where = string.Format("excerciseID = {0}", item.ID);
                        DBAdapter.Update(table, column, values, where, transaction);

                    }
                    catch (Exception ex)
                    {
                        new MessageBoxFS("Warning", ex.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
                    }
                }
            });

        }

        internal void ShowDialog()
        {
            _view.ShowDialog();
        }
    }
}
