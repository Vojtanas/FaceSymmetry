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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Timers;
using System.Collections.ObjectModel;

namespace Recorder
{
    public class Emulator : IDisposable
    {
        private RecorderViewModel _controller;
        private Storage _storage;

        private Timer _timer;
        private int _currentPosition = 0;
        private int _maxPosition;

        private object _lock = new object();

        public ObservableCollection<RecordData> Data = new ObservableCollection<RecordData>();


        public Emulator(RecorderViewModel controller, string filePath)
        {
            _controller = controller;
            _storage = new Storage(filePath,StorageType.RawData);
            _maxPosition = _storage.MaxFrame;
        }


        public void Start()
        {
            _timer = new Timer(100);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                _storage.ReadData(ref Data, _currentPosition, 1);
                _controller.UpdateFacePoints(Data[0].PointsFloat);
            }

            _currentPosition++;

            if (_currentPosition > _maxPosition)
                _currentPosition = 0;
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Elapsed -= _timer_Elapsed;

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                    _storage?.Dispose();
                }

                _timer = null;
                _storage = null;

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion



    }
}
