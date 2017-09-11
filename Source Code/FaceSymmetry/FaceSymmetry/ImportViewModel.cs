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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FaceSymmetry
{
    public class ImportModelView
    {

        private int _patientID;

        public ObservableCollection<string> ImportList = new ObservableCollection<string>();


        public ImportModelView(int patientID)
        {
            _patientID = patientID;
        }

        internal void SelectFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FindExaminations(dialog.SelectedPath.ToString());
                }
            }
        }

        private void FindExaminations(string dirPath)
        {
            var folders = Directory.EnumerateDirectories(dirPath, "*.*", SearchOption.AllDirectories);

            ObservableCollection<string> importDirs = new ObservableCollection<string>();

            if (folders.Count() != 0)
            {
                foreach (var folder in folders)
                {
                    var files = Directory.EnumerateFiles(folder, "*.*").Where(x => x.EndsWith(".bin", StringComparison.CurrentCultureIgnoreCase) ||
                    x.Contains("Record.txt") || x.Contains("Exercises.xml"));

                    if (files.Count() >= 3)
                    {
                        importDirs.Add(folder);
                    }
                }
            }
            else
            {
                var files = Directory.EnumerateFiles(dirPath, "*.*").Where(x => x.EndsWith(".bin", StringComparison.CurrentCultureIgnoreCase) ||
                    x.Contains("Record.txt") || x.Contains("Exercises.xml"));

                if (files.Count() >= 3)
                {
                    importDirs.Add(dirPath);
                }
            }

            if (importDirs.Count != 0)
            {
                ImportList = importDirs;
            }
            else
            {
                new MessageBoxFS("Info", "There are no files to be imported", MessageBoxFSButton.OK, MessageBoxFSImage.Information).ShowDialog();
                ImportList.Clear();
            }

        }

        internal bool ImportExaminations()
        {

            var di = new DriveInfo(Settings.SavePathDir);
            long availableSpace = di.AvailableFreeSpace;

            long importSize = 0;

            foreach (var folder in ImportList)
            {
                importSize += Tools.DirectorySize(folder);
            }

            if (importSize >= availableSpace)
            {
                new MessageBoxFS("Error", "There is not enough space on the disk.", MessageBoxFSButton.OK, MessageBoxFSImage.Error).ShowDialog();
                return false;
            }

            try
            {
                Import import = new Import();
                bool imported = import.ImportFiles(_patientID, ImportList);

                if (imported)
                {
                    new MessageBoxFS("Import", "All files were imported", MessageBoxFSButton.OK, MessageBoxFSImage.Information).ShowDialog();
                    return true;
                }
                else
                {
                    throw new Exception("Import went wrong, should not have been throwed");
                }


            }
            catch (Exception ex)
            {
                new MessageBoxFS("Import", string.Format("Error during importing files, no files were imported \n{0} \n{1}", ex.Message, ex.InnerException), MessageBoxFSButton.OK, MessageBoxFSImage.Warning).ShowDialog();
                return false;
            }


        }
    }
}
