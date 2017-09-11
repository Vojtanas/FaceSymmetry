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
using System.Windows.Data;

namespace FaceSymmetry
{
    public partial class ImportWindow : Window
    {
        private ImportModelView _modelView;

        public ImportWindow(int patientID)
        {
            InitializeComponent();
            _modelView = new ImportModelView(patientID);
            DataContext = _modelView;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            _modelView.SelectFolder();
            UpdateGrid();            
        }

        public void UpdateGrid()
        {
            CollectionViewSource.GetDefaultView(dataGrid.ItemsSource = _modelView.ImportList);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void patientSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_modelView.ImportExaminations())
                this.Close();
        }
    }
}
