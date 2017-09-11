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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace FaceSymmetry
{
    public partial class PatientControl : UserControl
    {
        public PatientControl()
        {
            InitializeComponent();

            genderComboBox.Items.Add("");
            genderComboBox.Items.Add("Male");
            genderComboBox.Items.Add("Female");

        }

        public void ClearAllBoxes()
        {
            IEnumerable<TextBox> textBoxes = patientGrid.Children.OfType<TextBox>();

            foreach (TextBox item in textBoxes)
            {
                item.Text = string.Empty;
            }

            IEnumerable<ComboBox> comboBoxes = patientGrid.Children.OfType<ComboBox>();

            foreach (ComboBox item in comboBoxes)
            {
                item.SelectedIndex = 0;
            }
        }

        internal bool IsAllBoxesCleared()
        {
            IEnumerable<TextBox> collection = patientGrid.Children.OfType<TextBox>();

            foreach (var item in collection)
            {
                if (item.Text != "")
                    return false;
            }

            return true;
        }      
    }
}
