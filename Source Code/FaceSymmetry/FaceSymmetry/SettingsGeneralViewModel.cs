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

using Common;
using System;
using System.Diagnostics;
using System.IO;

namespace FaceSymmetry
{
    public class SettingsGeneralModelView
    {

        SettingsGeneral _window;

        public SettingsGeneralModelView()
        {
            _window = new SettingsGeneral(this);
            LoadSettings();
            _window.ShowDialog();
        }

        private void LoadSettings()
        {
            _window.SavePathTxtBx.Text = Settings.SavePathDir;
        }

        public void OpenFolder(string path)
        {
            
            if (path != "")
            {
                try
                {
                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    new MessageBoxFS("Error", string.Format(ex.Message + "\n{0}", path), MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
                }
            }
        }

        internal string SavePath()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                   return dialog.SelectedPath.ToString();
                }
               
            }

            return null;
        }

        public bool SaveSettings(string path)
        {
            bool save = true;

            var di = new DriveInfo(path);
            long availableSpace = di.AvailableFreeSpace;

            long importSize = 0;
            var currentFolder = Directory.EnumerateDirectories(Settings.SavePathDir);

            foreach (var folder in currentFolder)
            {
                importSize += Tools.DirectorySize(folder);
            }

            if (importSize >= availableSpace)
            {
                new MessageBoxFS("Error", "There is not enough space on the disk.", MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
                return false;
            }



            if (!Directory.Exists(path))
            {
               
                bool? create = new MessageBoxFS("Warning", string.Format("Folder {0} does not exists, do you want to create it? ", path), MessageBoxFSButton.YESNO, MessageBoxFSImage.Question).ShowDialog();

                if (create == true)
                {
                    Directory.CreateDirectory(path);                    
                }
                else
                {
                    save = false;
                }
            }           


            if (save)
            {
                if(Settings.SavePathDir != path)
                {                    
                    var dirs = Directory.EnumerateDirectories(Settings.SavePathDir);

                    foreach (var dir in dirs)
                    {
                        string directory = Path.GetFileName(dir);
                        string newDir = Path.Combine(path, directory);
                        Directory.Move(dir, newDir);
                    }

                    if(Settings.SavePathDir != Settings.DefaultSavePathDir && Settings.SavePathDir != Settings.ProgramData)
                        Directory.Delete(Settings.SavePathDir);

                    Settings.SavePathDir = path;
                    Settings.Save();

                }               
            }

            return save;            
        }
    }
}
