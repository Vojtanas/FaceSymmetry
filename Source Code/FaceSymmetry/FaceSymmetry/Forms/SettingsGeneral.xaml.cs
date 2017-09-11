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

    public partial class SettingsGeneral : Window
    {
        SettingsGeneralModelView _controller;

        public SettingsGeneral(SettingsGeneralModelView settings)
        {
            InitializeComponent();
            if (settings == null) throw new Exception("SettingsGeneralModelView cannot be null");
            _controller = settings;
        }


        private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            _controller.OpenFolder(SavePathTxtBx.Text);
        }

        private void SavePathBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = _controller.SavePath();
            if (path != null && path != "")
            {
                SavePathTxtBx.Text = path;
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            bool close = _controller.SaveSettings(SavePathTxtBx.Text);

            if (close)
            {
                Close();
            }
        }        

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }


}
