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

using MathWorks.MATLAB.NET.Arrays;
using Microsoft.Kinect;
using System.Collections.Generic;

namespace Evaluator.Interpolations
{
    public enum InterpolationMethod
    {
        None = 0,
        ScatteredData = 1,             
    }

    public enum ScatteredInterpolationMethod
    {
        None = 0,
        cubic = 1,
        natural,
        linear,
        v4,
        nearest,
               
    }     


    interface Interpolation
    {
        MWArray Interpolate(IList<CameraSpacePoint> list, MWArray step, string method);
    }
}
