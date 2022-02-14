using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PolicyManage.Diff;
using PolicyManage.Parser;

namespace PolicyManage
{
    class Program
    {
        private static string machinePol = Environment.GetEnvironmentVariable("windir") + "\\System32\\GroupPolicy\\Machine\\Registry.pol";
        private static string userPol = Environment.GetEnvironmentVariable("windir") + "\\System32\\GroupPolicy\\User\\Registry.pol";
        static void useage()
        {
            Console.WriteLine("useage: PolicyManage.exe -command [options]");
            Console.WriteLine("\t-export [txt file], 导出所有pol文件记录到文本文档");
            Console.WriteLine("\t-import [txt file], 批量导入记录到组策略，格式可先导出样例查看");
            Console.WriteLine("\t-diff [a.pol] [b.pol] [diff file], 比较两个pol文件，生成差异文件");
            Console.WriteLine("\t-patch [diff file], 应用diff命令生成的差异文件");
            Console.WriteLine("\t-template, 批量导入组策略时模板格式");
        }

        static void setPolicy(int polFileType,string keyPath,string keyType,string value, object data)
        {
            var polFilePath = "";
            PolType polType = PolType.User;

            PolFileManager polFile = new PolFileManager();
            if (((int)PolType.Computer) == polFileType)
            {
                polFilePath = machinePol;
                polType = PolType.Computer;
            }
            else
            {
                polFilePath = userPol;
                polType = PolType.User;
            }
            if (File.Exists(polFilePath))
            {
                polFile.OpenPolFile(polFilePath, polType);
                PolValue val = new PolValue();
                val.m_Key = keyPath;
                Enum.TryParse(keyType, out val.m_KeyType);
                val.m_Value = value;
                if (val.m_KeyType == KeyType.REG_DWORD)
                {
                    ulong v = Convert.ToUInt64(data);
                    val.SetDataAsDWORD(v);
                    if (v == 2L)
                    {
                        //not configurate
                        val.m_bDeleteValue = true;
                    }
                }
                if (val.m_KeyType == KeyType.REG_SZ)
                {
                    val.SetDataAsString(data.ToString());
                }
                if (val.m_KeyType == KeyType.REG_NONE)
                {
                    val.SetDataAsString(data.ToString());
                }
                polFile.Set(val, polType);
                polFile.SavePolFile(polType);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                useage();
            }
            else if (String.Compare(args[0], "-export") == 0)
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("输入参数有误,请检查");
                    useage();
                }
                else
                {
                    string txt_file = args[1];
                    System.IO.File.Delete(@txt_file);
                    var computerFile = new RegistryFile();
                    var userFile = new RegistryFile();
                    computerFile.Open(machinePol);
                    foreach (var setting in computerFile.Settings)
                    {
                        string record = String.Format("0|{0}|{1}|{2}|{3}\n", setting.KeyPath, setting.Type.ToString(), setting.Value, setting.Data);
                        Console.WriteLine(record);
                        System.IO.File.AppendAllText(@txt_file, record, Encoding.UTF8);
                    }
                    userFile.Open(userPol);
                    foreach (var setting in userFile.Settings)
                    {
                        string record = String.Format("1|{0}|{1}|{2}|{3}\n", setting.KeyPath, setting.Type.ToString(), setting.Value, setting.Data);
                        Console.WriteLine(record);
                        System.IO.File.AppendAllText(@txt_file, record, Encoding.UTF8);
                    }
                }
            }
            else if (String.Compare(args[0], "-import") == 0)
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("输入参数有误,请检查");
                    useage();
                }
                else
                {
                    string txt_file = args[1];
                    StreamReader sr = new StreamReader(@txt_file);
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine();
                        Console.WriteLine(str);
                        string[] strs = str.Split('|');
                        Int32 polFileType = Convert.ToInt32(strs[0]);
                        setPolicy(polFileType, strs[1],strs[2],strs[3],strs[4]);
                    }
                    sr.Close();
                }
            }
            else if (String.Compare(args[0], "-diff") == 0)
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("输入参数有误,请检查");
                    useage();
                }
                else
                {
                    string file1Type = Repository.GetFileExtention(@args[1]);
                    string file2Type = Repository.GetFileExtention(@args[2]);
                    int granularity = 0;

                    IReadable file1 = Repository.RENAME(file1Type);
                    IReadable file2 = Repository.RENAME(file2Type);


                    if (!file1.Open(@args[1]))
                    {
                        Console.WriteLine("Parameter 1: '" + file1.Name + "' could not be opened");
                    }

                    if (!file2.Open(@args[2]))
                    {
                        Console.WriteLine("Parameter 2: '" + file2.Name + "' could not be opened");
                    }
                    IWritable outputFile = new TXTFile();
                    System.IO.File.Delete(@args[3]);
                    outputFile.Create(@args[3]);
                    List<string> file1Content = file1.Read();
                    List<string> file2Content = file2.Read();

                    if (file1Content.SequenceEqual(file2Content))
                    {
                        string[] fileContent = outputFile.CreateDiffForFile(granularity);

                        outputFile.Write(fileContent);

                        Console.WriteLine("The two files were identical.");
                    }
                    else
                    {
                        List<Line> lines = file1.FindDifference(file2.Read(), granularity);

                        string[] fileContent = outputFile.CreateDiffForFile(granularity, lines);

                        outputFile.Write(fileContent);
                    }
                }
            }
            else if (String.Compare(args[0], "-patch") == 0)
            {
                if (args.Length != 2)
                {
                    Console.WriteLine("输入参数有误,请检查");
                    useage();
                }
                else
                {
                    string patch_file = args[1];
                    StreamReader sr = new StreamReader(@patch_file);
                    while (!sr.EndOfStream)
                    {
                        string str = sr.ReadLine();
                        if (str.StartsWith("-"))
                        {
                            // 设置为未配置
                            string[] strs = str.Substring(1).Split('|');
                            Int32 polFileType = Convert.ToInt32(strs[0]);
                            setPolicy(polFileType, strs[1], KeyType.REG_DWORD.ToString(), strs[3], 2);
                        }
                        else if (str.StartsWith("="))
                        {
                            // 一致
                            continue;
                        }
                        else if(str.StartsWith("+"))
                        {
                            // 修改配置
                            string[] strs = str.Substring(1).Split('|');
                            Int32 polFileType = Convert.ToInt32(strs[0]);
                            setPolicy(polFileType, strs[1], strs[2], strs[3], strs[4]);
                        }
                        
                    }
                    sr.Close();
                }
            }
            else if (String.Compare(args[0], "-template") == 0)
            {
                Console.WriteLine();
                Console.WriteLine(" ******************************************************************");
                Console.WriteLine(" ***\t\t\t\t\t\t\t\t***");
                Console.WriteLine(" ***	批量设置组策略模板文件，一行记录代表一条组策略设置	***");
                Console.WriteLine(" ***\t\t\t\t\t\t\t\t***");
                Console.WriteLine(" ******************************************************************");
                Console.WriteLine();
                Console.WriteLine(" 策略大类|策略对应注册表|注册表类型|注册表项名称|注册表项值");
                Console.WriteLine();
                Console.WriteLine(" 策略大类：0-代表Machine，计算机配置 1-代表User，用户配置");
                Console.WriteLine(" 策略对应注册表：对应的注册表URL路径");
                Console.WriteLine(" 注册表类型：目前支持REG_DWORD和REG_SZ类型");
                Console.WriteLine(" 注册表项名称：对应注册表项名，禁用策略时名称增加前缀**del.");
                Console.WriteLine(" 注册表项值：对应注册表项值，REG_DWORD时0代表已禁用，1代表已启用,2代表未配置");
                Console.WriteLine();
                Console.WriteLine(" 举例：");
                Console.WriteLine(" 1|Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer|REG_DWORD|ClearRecentDocsOnExit|0");
                Console.WriteLine(" 1|Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer|REG_DWORD|ClearRecentProgForNewUserInStartMenu|0");
                Console.WriteLine(" 1|Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer|REG_DWORD|ClearRecentProgForNewUserInStartMenu|2");
                Console.WriteLine(" 1|Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer|REG_SZ|**del.ClearRecentDocsOnExit| ");
            }
            else
            {
                useage();
            }

            return;
        }
    }
}
