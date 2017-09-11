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

using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Common
{
    public enum RecordLocation
    {
        Local = 0,
        Distinct,
    }

    public class RecordData
    {
        public DateTime Date;
        public int LengthX;
        public int LengthY;
        public List<CameraSpacePoint> PointsFloat = new List<CameraSpacePoint>();
        public Point3DCollection PointsDouble = new Point3DCollection();
    }

    public class DataBaseException : Exception
    {
        public DataBaseException(string message) : base(message)
        {
        }

        public DataBaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }    

    public class AnalysisExercise
    {
        public string Exercise { get; set; }
        public string ExerciseAndCount { get; set; }
        public string Description { get; set; }
        public string Area { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Variance { get; set; }
        public double StdDev { get; set; }

        public AnalysisExercise(string exercise, string area, string mean, string max, string min, string median, string variance, string stddev, string count, string description)
        {
            Exercise = exercise;
            ExerciseAndCount = string.Format("{0} ({1})", exercise, count);
            Description = description;
            Area = area;
            Min = double.Parse(min);
            Max = double.Parse(max);
            Mean = double.Parse(mean);
            Median = double.Parse(median);
            Variance = double.Parse(variance);
            StdDev = double.Parse(stddev);
        }

    }



}
