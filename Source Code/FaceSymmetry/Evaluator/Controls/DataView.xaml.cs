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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Evaluator
{

    public partial class DataView : UserControl
    {

        public DataView()
        {
            InitializeComponent();
            tabControl.SelectedIndex = 0;

            border3D.MouseMove += Border3D_MouseMove;
            border3D.MouseDown += Border3D_MouseDown;
            border3D.MouseLeave += Border3D_MouseLeave;
            border3D.MouseEnter += Border3D_MouseEnter;
            border3D.MouseUp += Border3D_MouseUp;
            border3D.MouseWheel += Border3D_MouseWheel;


            Transform3DGroup transforms = new Transform3DGroup();
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0)));
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0)));
            transforms.Children.Add(new TranslateTransform3D());
            modelVisual3D.Transform = transforms;

            border3DInterpol.MouseMove += Border3DInterpol_MouseMove;
            border3DInterpol.MouseDown += Border3DInterpol_MouseDown;
            border3DInterpol.MouseLeave += Border3DInterpol_MouseLeave;
            border3DInterpol.MouseEnter += Border3DInterpol_MouseEnter;
            border3DInterpol.MouseUp += Border3DInterpol_MouseUp;
            border3DInterpol.MouseWheel += Border3DInterpol_MouseWheel;


            Transform3DGroup transforms2 = new Transform3DGroup();
            transforms2.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0)));
            transforms2.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
            transforms2.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0)));
            transforms2.Children.Add(new TranslateTransform3D());
            modelVisual3DInterpol.Transform = transforms2;

        }

        private void Border3DInterpol_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var wheel = e.Delta / 100;
            var position = cameraInterp.Position;
            cameraInterp.Position = new Point3D(position.X, position.Y, position.Z + wheel);
        }

        private void Border3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var wheel = e.Delta / 100;
            var position = camera.Position ;
            camera.Position = new Point3D(position.X, position.Y, position.Z + wheel);
        }

        private void Border3DInterpol_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
        }

        private void Border3DInterpol_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Border3DInterpol_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
            Mouse.OverrideCursor = null;
        }

        private void Border3DInterpol_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mousePosition = e.GetPosition(border3DInterpol);
            _mouseDown = true;
        }

        private void Border3DInterpol_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                var currentMousePosition = e.GetPosition(border3DInterpol);
                var difference = currentMousePosition - _mousePosition;
                SetRotationInterpol(difference.Y, difference.X, 0);
                _mousePosition = e.GetPosition(border3DInterpol);
            }
        }

        private void Border3D_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Border3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
        }

        private void Border3D_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
            Mouse.OverrideCursor = null;
        }

        private void Border3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mousePosition = e.GetPosition(border3D);
            _mouseDown = true;
        }

        private bool _mouseDown = false;
        private Point _mousePosition;

        private void Border3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                var currentMousePosition = e.GetPosition(border3D);
                var difference = currentMousePosition - _mousePosition;
                SetRotation(difference.Y, difference.X, 0);
                _mousePosition = e.GetPosition(border3D);
            }
        }

        public override void EndInit()
        {
            base.EndInit();
            slider.DataContext = this.DataContext;
        }

        #region Rotation

        double RotationX = 0;
        double RotationY = 0;
        double RotationZ = 0;

        public void SetRotation(double amountX, double amountY, double amountZ)
        {
            RotationX += amountX;
            RotationY += amountY;
            RotationZ += amountZ;
            
            Transform3DGroup transforms = GetTransforms();
            TranslateTransform3D translation = transforms.Children[3] as TranslateTransform3D;            
            Point3D translatedCenter = translation.Transform(GetCenter());

            RotateTransform3D rotX = (transforms.Children[0] as RotateTransform3D);
            RotateTransform3D rotY = (transforms.Children[1] as RotateTransform3D);
            RotateTransform3D rotZ = (transforms.Children[2] as RotateTransform3D);

            rotX.CenterX = rotY.CenterX = rotZ.CenterX = translatedCenter.X;
            rotX.CenterY = rotY.CenterY = rotZ.CenterY = translatedCenter.Y;
            rotX.CenterZ = rotY.CenterZ = rotZ.CenterZ = translatedCenter.Z;

            (rotX.Rotation as AxisAngleRotation3D).Angle = RotationX;
            (rotY.Rotation as AxisAngleRotation3D).Angle = RotationY;
            (rotZ.Rotation as AxisAngleRotation3D).Angle = RotationZ;
        }

        double RotationInterpolX = 0;
        double RotationInterpolY = 0;
        double RotationInterpolZ = 0;

        public void SetRotationInterpol(double amountX, double amountY, double amountZ)
        {
            RotationInterpolX += amountX;
            RotationInterpolY += amountY;
            RotationInterpolZ += amountZ;

           
            Transform3DGroup transforms = GetTransformsInterpol();
            TranslateTransform3D translation = transforms.Children[3] as TranslateTransform3D;          
            Point3D translatedCenter = translation.Transform(GetCenter());

            RotateTransform3D rotX = (transforms.Children[0] as RotateTransform3D);
            RotateTransform3D rotY = (transforms.Children[1] as RotateTransform3D);
            RotateTransform3D rotZ = (transforms.Children[2] as RotateTransform3D);

            rotX.CenterX = rotY.CenterX = rotZ.CenterX = translatedCenter.X;
            rotX.CenterY = rotY.CenterY = rotZ.CenterY = translatedCenter.Y;
            rotX.CenterZ = rotY.CenterZ = rotZ.CenterZ = translatedCenter.Z;

            (rotX.Rotation as AxisAngleRotation3D).Angle = RotationInterpolX;
            (rotY.Rotation as AxisAngleRotation3D).Angle = RotationInterpolY;
            (rotZ.Rotation as AxisAngleRotation3D).Angle = RotationInterpolZ;
        }

        private Point3D GetCenter()
        {
            var rect3D = Rect3D.Empty;
            UnionRect(modelVisual3D, ref rect3D);
            var _center = new Point3D((rect3D.X + rect3D.SizeX / 2), (rect3D.Y + rect3D.SizeY / 2), (rect3D.Z + rect3D.SizeZ / 2));
            return _center;
        }

        private Point3D GetCenterInterpol()
        {
            var rect3D = Rect3D.Empty;
            UnionRect(modelVisual3DInterpol, ref rect3D);
            var _center = new Point3D((rect3D.X + rect3D.SizeX / 2), (rect3D.Y + rect3D.SizeY / 2), (rect3D.Z + rect3D.SizeZ / 2));
            return _center;
        }

        private void UnionRect(ModelVisual3D model, ref Rect3D rect3D)
        {
            for (int i = 0; i < model.Children.Count; i++)
            {
                var child = model.Children[i] as ModelVisual3D;
                UnionRect(child, ref rect3D);
            }
            if (model.Content != null)
                rect3D.Union(model.Content.Bounds);
        }

        private Transform3DGroup GetTransforms()
        {
            return (Transform3DGroup)modelVisual3D.Transform;
        }
        private Transform3DGroup GetTransformsInterpol()
        {
            return (Transform3DGroup)modelVisual3DInterpol.Transform;
        }

        #endregion


    }
}


