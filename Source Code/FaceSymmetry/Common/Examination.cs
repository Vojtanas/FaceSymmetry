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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class Examination
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Guid { get; set; }
        public string Dir { get; set; }
        public RecordLocation RecordLocation { get; set; }
        public string Notes { get; set; }
        public List<string> Exercises { get; set; }

        public Examination(string id, string date, string guid, string dir, RecordLocation record, string notes, string exercises)
        {
            ID = int.Parse(id);
            Date = DateTime.Parse(date);
            Guid = guid;
            Dir = dir;
            RecordLocation = record;
            Notes = notes;
            Exercises = ParseExercises(exercises);

        }

        private List<string> ParseExercises(string exercises)
        {
            return exercises.Split(';').ToList();
        }
    }
}
