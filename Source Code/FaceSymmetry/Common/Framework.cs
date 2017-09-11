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
using System.ComponentModel;
using System.Windows.Input;

namespace Common
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CommandHandler : ICommand
    {
        private Action<object> _action;
        private Predicate<object> _canExecute;

        public CommandHandler(Action<object> action, Predicate<object> canExecute)
        {
            if (action == null)
                throw new ArgumentNullException("execute");

            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            if (_canExecute != null)
                OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }

}
