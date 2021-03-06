﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class DBFiles
    {
        public List<DBFile> FileList = new List<DBFile>();
        public List<DBFile> ValidFileList = new List<DBFile>();
        public DBFile SelectedFile = new DBFile();

        public void SeekAll(string rootPath)
        {
            Console.WriteLine("start seeking...");
            FileList.Clear();
            WalkThruFileSystem(rootPath, FileList);
            Console.WriteLine("end seekin. found: " + FileList.Count);
            Console.WriteLine("start sorting...");
            FileList.Sort(new DBFileComparer());
            Console.WriteLine("end sorting");
        }

        public void ValidateAll()
        {
            Console.WriteLine("start validating...");
            foreach (DBFile file in FileList)
            {
                if (file.IsValidChatDB(file.name))
                {
                    ValidFileList.Add(file);
                }
            }
            Console.WriteLine("end validating found: " + ValidFileList.Count);
        }


        /// <summary>
        /// http://www.mycsharp.de/wbb2/thread.php?threadid=58665
        /// </summary>
        /// <param name="strDir">root folder for recursive search</param>
        public static void WalkThruFileSystem(string strDir, List<DBFile> list)
        {
            // 0. Einstieg in die Rekursion auf oberster Ebene
            WalkThruFileSystem(new DirectoryInfo(strDir), list);
        }

        private static void WalkThruFileSystem(DirectoryInfo di, List<DBFile> list)
        {
            try
            {
                // 1. Für alle Dateien im aktuellen Verzeichnis
                foreach (FileInfo fi in di.GetFiles())
                {
                    // 1a. Statt Console.WriteLine hier die gewünschte Aktion
                    //Console.WriteLine("file: " + fi.FullName);
                    DBFile newfile = new DBFile();
                    newfile.name = fi.FullName;
                    newfile.timestamp = fi.LastWriteTime;
                    newfile.size = fi.Length;
                    list.Add(newfile);
                }

                // 2. Für alle Unterverzeichnisse im aktuellen Verzeichnis
                foreach (DirectoryInfo diSub in di.GetDirectories())
                {
                    // 2a. Statt Console.WriteLine hier die gewünschte Aktion
                    //Console.WriteLine("dir :" + diSub.FullName);

                    // 2b. Rekursiver Abstieg
                    WalkThruFileSystem(diSub, list);
                }
            }
            catch (Exception)
            {
                // 3. Statt Console.WriteLine hier die gewünschte Aktion
            }
        }

        public class DBFileComparer : IComparer<DBFile>
        {
            public int Compare(DBFile file2, DBFile file1)
            {
                var property1 = file1.timestamp;
                var Property2 = file2.timestamp;
                if (property1 > Property2)
                { return 1; }
                else if (property1 < Property2)
                { return -1; }
                else { return 0; }
            }
        }

        public int GetValidFileByName(string name)
        {
            int retvalue = -1;
            int index = ValidFileList.FindIndex(x => x.name == name);
            if (index >= 0)
            {
                SelectedFile = ValidFileList[index];
                retvalue = 0;
            }
            return retvalue;
        }

    }
}
