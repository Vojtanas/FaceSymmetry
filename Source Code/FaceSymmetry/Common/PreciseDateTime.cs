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

using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Common
{
    public class PreciseDatetime
    {       
        private static readonly Stopwatch myStopwatch = new Stopwatch();
        private static System.DateTime myStopwatchStartTime;

        static PreciseDatetime()
        {
            Reset();

            try
            {
                SystemEvents.TimeChanged += SystemEvents_TimeChanged;
            }
            catch (Exception)
            {
            }
        }

        static void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            Reset();
        }

        static public void Reset()
        {
            myStopwatchStartTime = System.DateTime.Now;
            myStopwatch.Restart();
        }

        public DateTime Now { get { return myStopwatchStartTime.Add(myStopwatch.Elapsed); } }
    }
}
