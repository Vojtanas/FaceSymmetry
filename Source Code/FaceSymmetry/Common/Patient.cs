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

namespace Common
{

    public class Patient : ObservableObject
    {
        private int _ID;
        private string _surname;   

        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                OnPropertyChanged("ID");
            }
        }

        public string FirstName { get; set; }
        public string Surname
        {
            get { return _surname; }
            set
            {
                _surname = value;
                OnPropertyChanged("Surname");
            }
        }
        public string SecondName { get; set; }
        public string PID { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string Notes { get; set; }

        public Patient()
        {
            ID =  -1;
            Surname = string.Empty;
            FirstName = string.Empty;
            SecondName = string.Empty;
            PID = string.Empty;
            Birthday = string.Empty;
            Gender = string.Empty;
            Notes = string.Empty;
        }

        public Patient(string id, string surname, string firstName, string secondName, string pid, string birthday, string gender, string notes)
        {
            ID = int.Parse(id);
            Surname = surname;
            FirstName = firstName;
            SecondName = secondName;
            PID = pid;
            Birthday = birthday;
            Gender = gender;
            Notes = notes;
        }

        internal string[] ToArray()
        {
            string[] array = new string[] { FirstName, SecondName, Surname, Birthday, PID, Gender, Notes };
            return array;
        }
    }
}
