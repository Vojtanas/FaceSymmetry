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
using System.IO;
using System.Windows.Media.Media3D;

namespace Common
{
    public enum StorageType
    {
        RawData = 0,
        Transformed,
        Interpolated,
    }

    public class Storage : IDisposable
    {
        private FileStream _stream;
        private object _lock = new object();
        private string _filePath;
        private readonly int _sizeOfFrameRaw = 16172; // RawData,Transformed //(8 + (1347 * (sizeof(float) * 3))) 

        public int MaxFrame = 0;
        public StorageType StorageType;
        public long[] FramePositions;
        public double Step;

        public Storage(string filePath, StorageType storageType, int frameCount = -1)
        {
            _filePath = filePath;
            StorageType = storageType;

            if (File.Exists(filePath))
            {
                _stream = new FileStream(filePath, FileMode.Open);

                switch (storageType)
                {
                    case StorageType.RawData:
                    case StorageType.Transformed:
                        MaxFrame = (int)GetStorageLength();
                        break;
                    case StorageType.Interpolated:
                        FramePositions = ReadHeader();
                        Step = GetStep();
                        break;
                    default:
                        throw new Exception("Unknow type of StorageType");
                }
            }
            else
            {
                _stream = new FileStream(filePath, FileMode.Create);
            }
        }

        private double GetStep()
        {
            FileInfo info = new FileInfo(_filePath);

            string[] stepA = info.Name.Split('_');
            string stepS = stepA[stepA.Length - 1].Replace(".bin", "").Replace("-", ",");
            double step = double.Parse(stepS);
            return step;

        }

        private long GetStorageLength()
        {
            var end = _stream.Seek(0, SeekOrigin.End);
            end = end / _sizeOfFrameRaw;
            return end;
        }

        public void WriteData(DateTime date, IReadOnlyList<Microsoft.Kinect.CameraSpacePoint> list)
        {
            byte[] byteDate = BitConverter.GetBytes(date.ToBinary());
            byte[] dataWrite = new byte[byteDate.Length + list.Count * 3 * sizeof(float)];


            Array.Copy(byteDate, 0, dataWrite, 0, byteDate.Length);
            int index = byteDate.Length;
            foreach (var point3 in list)
            {
                byte[] x = BitConverter.GetBytes(point3.X);
                Array.Copy(x, 0, dataWrite, index, x.Length);
                index += x.Length;

                byte[] y = BitConverter.GetBytes(point3.Y);
                Array.Copy(y, 0, dataWrite, index, y.Length);
                index += y.Length;

                byte[] z = BitConverter.GetBytes(point3.Z);
                Array.Copy(z, 0, dataWrite, index, z.Length);
                index += z.Length;
            }

            lock (_lock)
            {
                _stream.Write(dataWrite, 0, dataWrite.Length);
                _stream.Flush(true);
            }

        }

        public void ReadData(ref ObservableCollection<RecordData> list, int beginFrame, int frames)
        {
            list = new ObservableCollection<RecordData>();

            lock (_lock)
            {
                _stream.Position = beginFrame * _sizeOfFrameRaw;

                byte[] dateTime = new byte[8];
                int size = sizeof(float);
                byte[] readPoints = new byte[size * 3];

                for (int i = 0; i < frames; i++)
                {

                    _stream.Read(dateTime, 0, dateTime.Length);
                    long dat = BitConverter.ToInt64(dateTime, 0);
                    DateTime date = DateTime.FromBinary(dat);

                    list.Add(new RecordData());
                    list[i].Date = date;

                    for (int j = 0; j < 1347; j++)
                    {
                        Microsoft.Kinect.CameraSpacePoint point = new Microsoft.Kinect.CameraSpacePoint();

                        _stream.Read(readPoints, 0, size * 3);

                        point.X = BitConverter.ToSingle(readPoints, 0);
                        point.Y = BitConverter.ToSingle(readPoints, size);
                        point.Z = BitConverter.ToSingle(readPoints, size * 2);

                        list[i].PointsFloat.Add(point);

                    }
                }
            }
        }

        public void WriteData(ObservableCollection<RecordData> list)
        {
            _stream.Close();
            _stream = new FileStream(_filePath, FileMode.Truncate, FileAccess.ReadWrite);
            _stream.Position = 0;

            for (int i = 0; i < list.Count; i++)
            {
                byte[] byteDate = BitConverter.GetBytes(list[i].Date.ToBinary()); // Encoding.ASCII.GetBytes(date.ToString("s")); //old
                byte[] data = new byte[byteDate.Length + list[i].PointsFloat.Count * 3 * sizeof(float)];

                Array.Copy(byteDate, 0, data, 0, byteDate.Length);
                int index = byteDate.Length;
                foreach (var point3 in list[i].PointsFloat)
                {
                    byte[] x = BitConverter.GetBytes(point3.X);
                    Array.Copy(x, 0, data, index, x.Length);
                    index += x.Length;

                    byte[] y = BitConverter.GetBytes(point3.Y);
                    Array.Copy(y, 0, data, index, y.Length);
                    index += y.Length;

                    byte[] z = BitConverter.GetBytes(point3.Z);
                    Array.Copy(z, 0, data, index, z.Length);
                    index += z.Length;
                }

                _stream.Write(data, 0, data.Length);
                _stream.Flush(true);
            }

        }

        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
        }


        #region Interpolation


        internal void WriteHeader()
        {
            _stream.Position = 0;

            int size = FramePositions.Length;
            byte[] sizeB = BitConverter.GetBytes(size);
            _stream.Write(sizeB, 0, sizeB.Length);

            for (int i = 0; i < FramePositions.Length; i++)
            {
                byte[] frame = BitConverter.GetBytes(FramePositions[i]);
                _stream.Write(frame, 0, frame.Length);
            }
        }

        private long[] ReadHeader()
        {
            lock (_lock)
            {
                _stream.Position = 0;

                byte[] length = new byte[sizeof(int)];

                _stream.Read(length, 0, length.Length);
                int dat = BitConverter.ToInt32(length, 0);

                long[] frameLength = new long[dat];

                for (int i = 0; i < dat; i++)
                {
                    byte[] size = new byte[sizeof(ulong)];
                    _stream.Read(size, 0, size.Length);
                    long data = (long)BitConverter.ToInt64(size, 0);
                    frameLength[i] = data;
                }

                return frameLength;
            }
        }

        public void ReadInterpolatedData(ref ObservableCollection<RecordData> list, int beginFrame, int frames)
        {
            lock (_lock)
            {
                list = new ObservableCollection<RecordData>();
                _stream.Position = FramePositions[beginFrame];

                byte[] dateTime = new byte[8];
                byte[] length = new byte[4];
                int size = sizeof(double);
                byte[] readPoints = new byte[size * 3];

                for (int i = 0; i < frames; i++)
                {
                    _stream.Read(dateTime, 0, dateTime.Length);
                    long dat = BitConverter.ToInt64(dateTime, 0);
                    DateTime date = DateTime.FromBinary(dat);

                    _stream.Read(length, 0, length.Length);
                    int LengthX = BitConverter.ToInt32(length, 0);
                    _stream.Read(length, 0, length.Length);
                    int LengthY = BitConverter.ToInt32(length, 0);

                    list.Add(new RecordData());
                    list[i].Date = date;
                    list[i].LengthX = LengthX;
                    list[i].LengthY = LengthY;

                    for (int j = 0; j < LengthX * LengthY; j++)
                    {
                        _stream.Read(readPoints, 0, size * 3);

                        double x = BitConverter.ToDouble(readPoints, 0);
                        double y = BitConverter.ToDouble(readPoints, size);
                        double z = BitConverter.ToDouble(readPoints, size * 2);
                        Point3D point = new Point3D(x, y, z);

                        list[i].PointsDouble.Add(point);
                    }
                }
            }
        }


        internal void WriteInterpolatedData(DateTime date, IList<Point3D> points, int lengthX, int lengthY, int frame)
        {
            byte[] byteDate = BitConverter.GetBytes(date.ToBinary());
            byte[] LengthX = BitConverter.GetBytes(lengthX);
            byte[] LengthY = BitConverter.GetBytes(lengthY);

            byte[] data = new byte[byteDate.Length + LengthX.Length + LengthY.Length + points.Count * 3 * sizeof(double)];

            Array.Copy(byteDate, 0, data, 0, byteDate.Length);
            int index = byteDate.Length;

            Array.Copy(LengthX, 0, data, index, LengthX.Length);
            index += LengthX.Length;

            Array.Copy(LengthY, 0, data, index, LengthY.Length);
            index += LengthY.Length;

            foreach (var point3 in points)
            {
                byte[] x = BitConverter.GetBytes(point3.X);
                Array.Copy(x, 0, data, index, x.Length);
                index += x.Length;

                byte[] y = BitConverter.GetBytes(point3.Y);
                Array.Copy(y, 0, data, index, y.Length);
                index += y.Length;

                byte[] z = BitConverter.GetBytes(point3.Z);
                Array.Copy(z, 0, data, index, z.Length);
                index += z.Length;

            }

            lock (_lock)
            {
                _stream.Position = (long)FramePositions[frame];
                _stream.Write(data, 0, data.Length);
                _stream.Flush(true);
            }

        }

        internal void Delete()
        {
            File.Delete(_filePath);
        }

        #endregion
    }
}
