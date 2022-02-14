using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PolicyManage.Diff
{
    class CSVFile : Repository, IReadable
    {

        //Override for the base class 'Read' method
        public override List<string> Read()
        {
            string fileContent = File.ReadAllText(this.FilePath);

            fileContent.Replace('\n', ',');

            string[] fileContentArray = fileContent.Split(',');

            //Removes the last line containing the last ',' character
            List<string> fileContentList = new List<string>(fileContentArray);
            fileContentList.RemoveAt(fileContentList.Count() - 1);

            return fileContentList;
        }

        //Override for the base class 'Write' method
        public override bool Write(string[] fileContent)
        {
            File.WriteAllText(FilePath, string.Join(",", fileContent));
            return true;
        }

        public override bool Create(string directoryPath)
        {
            FilePath = GetAbsolutePath(directoryPath + GetDateTime() + ".csv");


            if (Exists(FilePath))
            {
                File.Create(FilePath);
            }

            return true;
        }
    }
}
