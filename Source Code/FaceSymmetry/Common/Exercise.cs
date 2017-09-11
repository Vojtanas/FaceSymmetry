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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Common
{
    public class ObservableExercise : ObservableObject
    {
        private string _id;
        private string _name;
        private string _description;
        private bool _active;

        public string ID
        {
            get { return _id; }
            set
            {
                if (value != null)
                {
                    _id = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != null)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (value != null)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged("Active");
            }
        }

        public bool Deleted;
        public bool Changed;

        public ObservableExercise(string id, string name, string description, string active)
        {
            _id = id;
            _name = name;
            _description = description;
            _active = bool.Parse(active);
            Deleted = false;
            Changed = false;
        }

        public ObservableExercise(string name, string description)
        {
            _id = "-1";
            _name = name;
            _description = description;
            _active = true;
            Deleted = false;
            Changed = false;
        }

        public ObservableExercise(string id, string name, string description, bool active)
        {
            _id = id;
            _name = name;
            _description = description;
            _active = active;
            Deleted = false;
            Changed = false;
        }


    }

    public class ExerciseCollection : ObservableCollection<ObservableExercise>
    {
        public event PropertyChangedEventHandler ExerciseChanged;

        private void NotifyExerciseChanged(string exerciseName)
        {
            ExerciseChanged?.Invoke(this, new PropertyChangedEventArgs(exerciseName));
        }


        public new void Add(ObservableExercise exercise)
        {
            int count = this.Where(x => x.Name == exercise.Name).Count();
            if (count == 0)
            {

                base.Add(exercise);
                exercise.PropertyChanged += Exercise_PropertyChanged;

            }
            else
            {
                throw new ArgumentException("There already exists exercise with same name");
            }
        }

        private void Exercise_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ObservableExercise exercise = sender as ObservableExercise;
            NotifyExerciseChanged(exercise.Name);
        }

        public ExerciseCollection Clone()
        {
            ExerciseCollection result = new ExerciseCollection();
            result = new ExerciseCollection();
            foreach (var item in this)
            {
                result.Add(new ObservableExercise(item.ID, item.Name, item.Description, item.Active));
            }

            return result;
        }

        public void Remove(string name)
        {
            this.Single(x => x.Name == name).Deleted = true;
        }
    }



    public class ExerciseDictionary : Dictionary<ObservableExercise, List<TimeSpan[]>>
    {
        public event PropertyChangedEventHandler KeyChanged;

        public void OnKeyChanged(string propertyName)
        {
            KeyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
