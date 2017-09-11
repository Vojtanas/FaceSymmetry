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
using Microsoft.Kinect.Face;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Recorder
{
    public partial class RecorderViewModel
    {
        private KinectSensor sensor = null;
        private BodyFrameSource bodySource = null;
        private BodyFrameReader bodyReader = null;
        private HighDefinitionFaceFrameSource highDefinitionFaceFrameSource = null;
        private HighDefinitionFaceFrameReader highDefinitionFaceFrameReader = null;
        internal FaceAlignment currentFaceAlignment = null;
        internal FaceModel currentFaceModel = null;
        private FaceModelBuilder faceModelBuilder = null;
        private Body currentTrackedBody = null;
        private ulong currentTrackingId = 0;
        private string currentBuilderStatus = string.Empty;


        private ulong CurrentTrackingId
        {
            get
            {
                return currentTrackingId;
            }

            set
            {
                currentTrackingId = value;
            }
        }



        private static double VectorLength(CameraSpacePoint point)
        {
            var result = Math.Pow(point.X, 2) + Math.Pow(point.Y, 2) + Math.Pow(point.Z, 2);

            result = Math.Sqrt(result);

            return result;
        }


        private static Body FindClosestBody(BodyFrame bodyFrame)
        {
            Body result = null;
            double closestBodyDistance = double.MaxValue;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    var currentLocation = body.Joints[JointType.SpineBase].Position;

                    var currentDistance = VectorLength(currentLocation);

                    if (result == null || currentDistance < closestBodyDistance)
                    {
                        result = body;
                        closestBodyDistance = currentDistance;
                    }
                }
            }

            return result;
        }


        private static Body FindBodyWithTrackingId(BodyFrame bodyFrame, ulong trackingId)
        {
            Body result = null;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    if (body.TrackingId == trackingId)
                    {
                        result = body;
                        break;
                    }
                }
            }

            return result;
        }

        internal void InitializeHDFace()
        {
            sensor = KinectSensor.GetDefault();
            bodySource = sensor.BodyFrameSource;
            bodyReader = bodySource.OpenReader();
            bodyReader.FrameArrived += BodyReader_FrameArrived;

            highDefinitionFaceFrameSource = new HighDefinitionFaceFrameSource(sensor);
            highDefinitionFaceFrameSource.TrackingQuality = FaceAlignmentQuality.High;
            highDefinitionFaceFrameSource.TrackingIdLost += HdFaceSource_TrackingIdLost;

            highDefinitionFaceFrameReader = highDefinitionFaceFrameSource.OpenReader();
            highDefinitionFaceFrameReader.FrameArrived += HdFaceReader_FrameArrived;

            currentFaceModel = new FaceModel();
            currentFaceAlignment = new FaceAlignment();

            InitializeMesh();
            _view.UpdateMesh();

            sensor.Open();
        }


        internal void InitializeMesh()
        {
            var vertices = currentFaceModel.CalculateVerticesForAlignment(currentFaceAlignment);

            var triangleIndices = currentFaceModel.TriangleIndices;

            var indices = new Int32Collection(triangleIndices.Count);

            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                uint index01 = triangleIndices[i];
                uint index02 = triangleIndices[i + 1];
                uint index03 = triangleIndices[i + 2];

                indices.Add((int)index03);
                indices.Add((int)index02);
                indices.Add((int)index01);
            }

            _view.theGeometry.TriangleIndices = indices;
            _view.theGeometry.Normals = null;
            _view.theGeometry.Positions = new Point3DCollection();
            _view.theGeometry.TextureCoordinates = new PointCollection();

            foreach (var vert in vertices)
            {
                _view.theGeometry.Positions.Add(new Point3D(vert.X, vert.Y, -vert.Z));
                _view.theGeometry.TextureCoordinates.Add(new Point());
            }
        }


        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            var frameReference = e.FrameReference;
            using (var frame = frameReference.AcquireFrame())
            {
                if (frame == null)
                {
                    return;
                }

                if (currentTrackedBody != null)
                {
                    currentTrackedBody = FindBodyWithTrackingId(frame, CurrentTrackingId);

                    if (currentTrackedBody != null)
                    {
                        return;
                    }
                }

                Body selectedBody = FindClosestBody(frame);

                if (selectedBody == null)
                {
                    return;
                }

                currentTrackedBody = selectedBody;
                CurrentTrackingId = selectedBody.TrackingId;

                highDefinitionFaceFrameSource.TrackingId = CurrentTrackingId;
            }
        }


        private void HdFaceSource_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            var lostTrackingID = e.TrackingId;

            if (CurrentTrackingId == lostTrackingID)
            {
                CurrentTrackingId = 0;
                currentTrackedBody = null;
                if (faceModelBuilder != null)
                {
                    faceModelBuilder.Dispose();
                    faceModelBuilder = null;
                }

                highDefinitionFaceFrameSource.TrackingId = 0;
            }
        }



        private void HdFaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null || !frame.IsFaceTracked)
                {
                    return;
                }

                frame.GetAndRefreshFaceAlignmentResult(currentFaceAlignment);
                _view.UpdateMesh();
            }
        }


        internal void StartCapturesd()
        {
            StopFaceCapture();

            faceModelBuilder = null;
            faceModelBuilder = highDefinitionFaceFrameSource.OpenModelBuilder(FaceModelBuilderAttributes.None);
            faceModelBuilder.BeginFaceDataCollection();
        }


        private void StopFaceCapture()
        {
            if (faceModelBuilder != null)
            {
                faceModelBuilder.Dispose();
                faceModelBuilder = null;
            }
        }


    }
}
