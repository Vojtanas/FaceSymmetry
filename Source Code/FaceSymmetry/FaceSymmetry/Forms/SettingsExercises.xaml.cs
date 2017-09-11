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
using System.Windows;

namespace FaceSymmetry
{

    public partial class SettingsExercises : Window
    {
        
        private SettingsExercisesViewModel _modelView;

        public SettingsExercises(SettingsExercisesViewModel modelView)
        {
            if (modelView == null)
                throw new ArgumentException("Controller can not be null");

            InitializeComponent();
            _modelView = modelView;

            exercisesControl.AddExercise += ExercisesControl_AddExercise;
            exercisesControl.RemoveExercise += ExercisesControl_RemoveExercise;
           
        }

        private void ExercisesControl_RemoveExercise(string itemID)
        {
            _modelView.RemoveExercise(itemID);
        }

        private void ExercisesControl_AddExercise(string exerciseName)
        {
            _modelView.AddExercise(exerciseName);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {          
            _modelView.Save();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            exercisesControl.ExercisesCollection = _modelView.Exercises;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}
