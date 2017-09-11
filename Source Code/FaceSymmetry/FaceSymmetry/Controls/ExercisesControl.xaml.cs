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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FaceSymmetry
{
    public partial class ExercisesControl : UserControl
    {

        private bool _suppressSelection = false;
        private string _selectedItem { get; set; }
        private ExerciseCollection _exercisesCollection;

        public delegate void AddExerciseDelegate(string exerciseName);
        public delegate void RemoveExerciseDelegate(string itemID);

        public event AddExerciseDelegate AddExercise;
        public event RemoveExerciseDelegate RemoveExercise;

        public ExerciseCollection ExercisesCollection
        {
            get { return _exercisesCollection; }
            set
            {
                if (value != null)
                {
                    _exercisesCollection = value;
                    LoadSettings();
                }
            }
        }

        public List<string> InactiveList
        {
            get { return ExercisesCollection.Where(x => x.Active == false && x.Deleted == false).Select(x => x.Name).ToList(); }
        }

        public List<string> ActiveList
        {
            get { return ExercisesCollection.Where(x => x.Active == true && x.Deleted == false).Select(x => x.Name).ToList(); }
        }


        public ExercisesControl()
        {
            _exercisesCollection = new ExerciseCollection();
            InitializeComponent();

            SetButtons();
        }


        private void LoadSettings()
        {
            ActiveListBox.ItemsSource = ActiveList;
            InactiveListBox.ItemsSource = InactiveList;
            ActiveListBox.SelectionChanged += ActiveListBox_SelectionChanged;
            InactiveListBox.SelectionChanged += InactiveListBox_SelectionChanged;

        }

        private void InactiveListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_suppressSelection)
            {
                _suppressSelection = true;
                ActiveListBox.SelectedIndex = -1;
                SelectionChanged(sender, e);
                _suppressSelection = false;
            }
        }

        private void ActiveListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_suppressSelection)
            {
                _suppressSelection = true;
                InactiveListBox.SelectedIndex = -1;
                SelectionChanged(sender, e);
                _suppressSelection = false;
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;

            if (listBox.SelectedItem == null)
            {
                _selectedItem = "";
                descriptionTxtBx.Text = "";
            }
            else
            {
                var selectedItem = _selectedItem = listBox.SelectedItem.ToString();

                if (selectedItem != "null" && selectedItem != "")
                {
                    descriptionTxtBx.Text = ExercisesCollection.Where(x => x.Name == selectedItem).Single().Description;
                }
            }
        }

        private void SetButtons()
        {
            moveActiveBtn.Content = "<<";
            moveActiveBtn.FontSize = 12;
            removeActiveBtn.Content = ">>";
            removeActiveBtn.FontSize = 12;
        }

        private void moveActiveBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = InactiveListBox?.SelectedItem?.ToString();
            if (selectedItem != null)
            {
                ExercisesCollection.Where(x => x.Name == selectedItem).Single().Active = true;
                ListBox_SourceUpdated(null, null);
            }
        }

        private void removeActiveBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ActiveListBox?.SelectedItem?.ToString();
            if (selectedItem != null)
            {
                ExercisesCollection.Where(x => x.Name == selectedItem).Single().Active = false;
                ListBox_SourceUpdated(null, null);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ActiveListBox.SelectionChanged -= ActiveListBox_SelectionChanged;
            InactiveListBox.SelectionChanged -= InactiveListBox_SelectionChanged;
        }

        private void descriptionTxtBx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedItem != "")
            {
                ExercisesCollection.Where(x => x.Name == _selectedItem).Single().Description = descriptionTxtBx.Text;
            }
        }

        private void addNewExBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddExercise(newExTxtBx.Text);
                ListBox_SourceUpdated(null, null);
                ActiveListBox.SelectedItem = newExTxtBx.Text;
                newExTxtBx.Text = "new exercise";
                ActiveListBox.Focus();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem != null && _selectedItem != "")
            {
                RemoveExercise(_selectedItem);
                ListBox_SourceUpdated(null, null);
            }

        }

        private void ListBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            ActiveListBox.ItemsSource = ActiveList;
            InactiveListBox.ItemsSource = InactiveList;
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                    tb.SelectAll();
                }
            }
        }

        private void newExTxtBx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains(";"))
            {
                e.Handled = true;
            }

        }
    }
}
