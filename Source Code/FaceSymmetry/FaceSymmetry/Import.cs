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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FaceSymmetry
{
    public class Import
    {
        public bool ImportFiles(int patientID, ObservableCollection<string> importList)
        {
            bool success = false;

            string saveDir = Common.Settings.SavePathDir;
            string table = "examination";
            string[] column = { "dir", "patientID", "guid", "created", "notes", "exercises" };

            List<string> folders = new List<string>();

            object addLock = new object();

            try
            {
                DBAdapter.Transaction((transaction) =>
                {

                    Parallel.ForEach(importList, (dir) =>
                    {
                        string folderName = new DirectoryInfo(dir).Name;

                        string newFolderName = folderName;

                        if (folderName.Length != 19)
                        {
                            string[] fName = folderName.Split('_');

                            for (int j = 1; j < fName.Length; j++)
                            {
                                if (fName[j].Length != 2)
                                {
                                    fName[j] = "0" + fName[j];
                                }
                            }

                            newFolderName = string.Join("_", fName);
                        }

                        lock (addLock)
                        {
                            folders.Add(newFolderName);
                        }

                        string newDir = Path.Combine(saveDir, newFolderName);

                        if (!Directory.Exists(newDir))
                            Directory.CreateDirectory(newDir);

                        var files = Directory.EnumerateFiles(dir);

                        foreach (var filePath in files)
                        {
                            string newPath = Path.Combine(newDir, Path.GetFileName(filePath));
                            if (filePath.Contains(folderName + ".bin"))
                            {
                                newPath = Path.Combine(newDir, newFolderName + ".bin");
                            }
                            File.Copy(filePath, newPath, true);
                        }

                    });

                    int i = 0;
                    foreach (var item in folders)
                    {
                        string[] dateS = item.Split('_');

                        for (int j = 1; j < dateS.Length; j++)
                        {
                            if (dateS[j].Length != 2)
                            {
                                dateS[j] = "0" + dateS[j];
                            }
                        }

                        string datetime = string.Join("_", dateS);

                        DateTime date;
                        bool valid = DateTime.TryParseExact(datetime, "yyyy'_'MM'_'dd'_'HH'_'mm'_'ss", null, System.Globalization.DateTimeStyles.None, out date);
                        if (!valid)
                        {
                            throw new NotSupportedException("Provided DateTime format is not supported");
                        }

                        string dateTime = date.ToString("yyyy-MM-dd HH:mm:ss");

                        var files = Directory.EnumerateFiles(Path.Combine(saveDir, item), "*", SearchOption.TopDirectoryOnly);

                        var exercise = files.Single(x => x.Contains("Exercises.xml"));
                        var notes = files.Single(x => x.Contains("Record.txt"));

                        XDocument xdoc = XDocument.Load(exercise);
                        var erx = xdoc.Root.Descendants("Name");

                        StringBuilder sb = new StringBuilder();
                        foreach (var exe in erx)
                        {
                            sb.Append(exe.Value + ";");
                        }

                        string notesS = string.Empty;

                        using (StreamReader sr = new StreamReader(notes))
                        {
                            notesS = sr.ReadToEnd();
                        }

                        string[] values = { item, patientID.ToString(), Guid.NewGuid().ToString(), dateTime, notesS, sb.ToString() };
                        DBAdapter.Insert(table, column, values, transaction);

                        i++;
                    }

                });


                success = true;

            }

            catch (Exception)
            {

                Parallel.ForEach(importList, (dir) =>
                {
                    string folderName = new DirectoryInfo(dir).Name;
                    string newDir = Path.Combine(saveDir, folderName);

                    if (Directory.Exists(newDir))
                        Directory.Delete(newDir, true);

                });

                throw;
            }

            return success;

        }

    }
}
