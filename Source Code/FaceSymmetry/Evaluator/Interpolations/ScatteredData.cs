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

using Interpolation;
using MathWorks.MATLAB.NET.Arrays;
using Microsoft.Kinect;
using System.Collections.Generic;

namespace Evaluator.Interpolations
{
    public class ScatteredData : Interpolation
    {
        Scattered interp = new Scattered();

        public MWArray Interpolate(IList<CameraSpacePoint> list, MWArray step, string method)
        {
            float[,] ar = new float[3, list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                ar[0, i] = list[i].X;
                ar[1, i] = list[i].Y;
                ar[2, i] = list[i].Z;
            }

            MWArray matrix = new MWNumericArray(ar);

            return interp.GridData(matrix, step, method);
        }


    }

   


}
