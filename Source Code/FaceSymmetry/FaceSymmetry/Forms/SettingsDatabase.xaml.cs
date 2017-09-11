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
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace FaceSymmetry
{

    public partial class SettingsDatabase : Window
    {
        public SettingsDatabase()
        {
            InitializeComponent();
            serverAddressTxtBx.Text = Settings.Server;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            DBAdapter.SetServer(Settings.Server = serverAddressTxtBx.Text);
            Settings.Save();
            Close();
        }

        private void ClearExceptionTxtBx()
        {
            exceptionTxtBx.Text = "";
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(TestConnection);
            thread.Start();
        }

        private void TestConnection()
        {
            Dispatcher.BeginInvoke((Action)(() => ClearExceptionTxtBx()));
            Thread.Sleep(1);
            Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    DBAdapter.SetServer(serverAddressTxtBx.Text);
                    bool connection = DBAdapter.TestConnection();

                    exceptionTxtBx.Text = "Connection established";
                    exceptionTxtBx.Foreground = Brushes.DarkGreen;

                }
                catch (Exception ex)
                {
                    exceptionTxtBx.Text = ex.Message.Trim();
                    exceptionTxtBx.Foreground = Brushes.Red;

                }
                finally
                {
                    DBAdapter.SetServer(Settings.Server);
                }

            }));

        }
    }
}
