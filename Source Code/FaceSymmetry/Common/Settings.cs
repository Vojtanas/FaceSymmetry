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
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Common
{
    public static class Settings
    {
        public static string ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\FaceSymmetry";
        private static string _filePath = ProgramData + "\\Settings.xml";

        public static string DefaultSavePathDir = ProgramData + "\\Records";
        public static string Server { get; set; }
        public static string SavePathDir { get; set; }


        static Settings()
        {          
            Deserialize();
        }

        public static void Save()
        {
            Serialize();
        }

        private static void Serialize()
        {
            XDocument document = new XDocument(
            new XComment("Face Symmetry Settings"),
            new XElement("Settings",
            new XElement("Server", Server),
            new XElement("SavePath", SavePathDir)));

            document.Save(_filePath);
        }

        private static void Deserialize()
        {
            XDocument document = new XDocument();

            try
            {
                document = XDocument.Load(_filePath);
                Server = document.Root.Descendants("Server").Single().Value;
                string savePath = document.Root.Descendants("SavePath").Single().Value;

                if (savePath == "")
                {
                    SavePathDir = DefaultSavePathDir;

                    if (!Directory.Exists(DefaultSavePathDir))
                    {
                        Directory.CreateDirectory(DefaultSavePathDir);
                    }

                    Serialize();
                }
                else
                {
                    SavePathDir = savePath;
                }
            }
            catch (Exception)
            {
                CreateDefaultSettings();
            }
        }

        private static void CreateDefaultSettings()
        {
            if (!Directory.Exists(DefaultSavePathDir))
                Directory.CreateDirectory(DefaultSavePathDir);

            Server = "";
            SavePathDir = DefaultSavePathDir;
            Serialize();
        }

    }
}
