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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace FaceSymmetry
{
    public partial class MainWindow : Window
    {
        private SolidColorBrush _menuColor;
        private SolidColorBrush _borderHighlight;
        MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel controller)
        {
            DataContext = _viewModel = controller;
            _menuColor = (SolidColorBrush)FindResource("MenuColor");
            _borderHighlight = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF86BCFF"));

            InitializeComponent();
        }


        public void PatientChanged()
        {
            CollectionViewSource.GetDefaultView(examinationGrid.ItemsSource = _viewModel.Examinations);
        }

        public void ExaminationChanged()
        {
            CollectionViewSource.GetDefaultView(examinationGrid.ItemsSource = _viewModel.Examinations);
        }

        private void searchBorder_GotFocus(object sender, RoutedEventArgs e)
        {
            searchBorder.BorderBrush = _borderHighlight;
            if (searchBox.Text == "Search...")
            {
                searchBox.Text = "";
                searchBox.Foreground = Brushes.Black;
            }

            searchBox.TextChanged += searchBox_TextChanged;
        }

        private void searchBorder_LostFocus(object sender, RoutedEventArgs e)
        {
            searchBox.TextChanged -= searchBox_TextChanged;

            searchBorder.BorderBrush = _menuColor;
            if (searchBox.Text == "")
            {
                searchBox.Text = "Search...";
                searchBox.Foreground = Brushes.LightGray;
            }
        }

        public bool PatientFilter(object item)
        {
            if (String.IsNullOrEmpty(searchBox.Text) || searchBox.Text == "Search...")
                return true;

            var patient = (Patient)item;

            return (patient.FirstName.StartsWith(searchBox.Text, StringComparison.OrdinalIgnoreCase)
                    || patient.Surname.StartsWith(searchBox.Text, StringComparison.OrdinalIgnoreCase)
                    || patient.ID.ToString().StartsWith(searchBox.Text, StringComparison.OrdinalIgnoreCase)
                    || patient.PID.StartsWith(searchBox.Text, StringComparison.OrdinalIgnoreCase)
                    );
        }


        public void RefreshPatientGrid()
        {
            CollectionViewSource.GetDefaultView(patientGrid.ItemsSource).Refresh();
        }

        public void UpdatePatientGrid()
        {
            if (_viewModel.Patients != null)
                CollectionViewSource.GetDefaultView(patientGrid.ItemsSource = _viewModel.Patients).Filter = PatientFilter;
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBoxName = (TextBox)sender;
            string filterText = textBoxName.Text;

            if (patientGrid.ItemsSource == null)
                return;

            RefreshPatientGrid();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                searchBox.Text = "";
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            _viewModel.OpenExamination();
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                _viewModel.ReloadPatientsGrid();
            }
            catch (DataBaseException ex)
            {
                new MessageBoxFS("Warning", string.Format("{0} \n\nException\n {1} ", ex.Message, ex.InnerException), MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
            }

            WindowState = WindowState.Maximized;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    _viewModel.ReloadPatientsGrid();
                    break;
                case Key.F1:
                    _viewModel.NewPatient();
                    break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.Close();
        }

      

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();           
        }
    }
}
