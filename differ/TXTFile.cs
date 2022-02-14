using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PolicyManage.Diff
{

    class TXTFile : Repository, IReadable
    {
        //Override for the base class 'Read' method
        public override List<string> Read()
        {

            
            string fileContent = File.ReadAllText(FilePath);
       
            string[] fileContentArray = fileContent.Split('\n');

            /*for (int i = 0; i < fileContentArray.Length - 1; i++)
            {
                fileContentArray[i] = fileContentArray[i].Remove(fileContentArray[i].Length - 1);
            }*/

            //Removes the last line containing the \n character
            List<string> fileContentList = new List<string>(fileContentArray);
            fileContentList.RemoveAt(fileContentList.Count() - 1);

            return fileContentList;
        }

        public override bool Create(string fileName)
        {


            FilePath = GetAbsolutePath(fileName);


            if (Exists(FilePath))
            {
                File.Create(FilePath);
            }

            return true;
        }

       

        //Override for the base class 'Write' method
        public override bool Write(string[] fileContent)
        {
            File.WriteAllLines(FilePath, fileContent);
            return true;
        }
    }
}
