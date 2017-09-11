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

namespace FaceSymmetry
{

    public partial class App : Application
    {
        static Mutex mutex = new Mutex(true, "{FaceSymmetryApplicationMutex}");
        MainWindowViewModel Application;


        [STAThread]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;


            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application = new MainWindowViewModel();
                mutex.ReleaseMutex();
            }
            else
            {
                Shutdown();
            }
        }
      

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            new MessageBoxFS("Unhandled Error", "Unhandled exception occured, for solution please contact author.\n" + e.Exception.Message, MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
        }
    }


}