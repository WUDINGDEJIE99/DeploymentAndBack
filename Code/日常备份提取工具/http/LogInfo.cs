using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace 数据制作
{
    public class LogInfoFile
    {
        private static ReaderWriterLockSlim lockRW = new ReaderWriterLockSlim();
        private static string path = System.AppDomain.CurrentDomain.BaseDirectory + "MLog";

        /// <summary>
        /// 获取格式化的json，而不是一行文本
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetJsonGeShiHuaString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
            //待研究
            //StringWriter textWriter = new StringWriter();
            //JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            //{
            //    Formatting = Formatting.Indented,//格式化缩进
            //    Indentation = 4,  //缩进四个字符
            //    IndentChar = ' '  //缩进的字符是空格
            //};
            //serializer.Serialize(jsonWriter, obj);
            //return textWriter.ToString();
            ////JsonSerializerSettings jsetting = new JsonSerializerSettings();
            ////jsetting.DefaultValueHandling = DefaultValueHandling.Ignore;

            ////josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented, jsetting);
            //////josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented);
            ////System.IO.File.WriteAllText(filename, josnString, Encoding.UTF8);
        }

        /// <summary>
        /// 返回是否带有行号的文本
        /// </summary>
        /// <param name="listwhere"></param>
        /// <param name="hanghao"></param>
        /// <returns></returns>
        public static string GetNR(List<string> listwhere, bool hanghao = false)
        {
            StringBuilder sb = new StringBuilder();
            if (hanghao)
            {
                int i = 0;
                foreach (var item in listwhere)
                {
                    i++;
                    sb.AppendLine($"{i} {item}");
                }
            }
            else
            {
                sb.AppendLine($"{string.Join(Environment.NewLine, listwhere.ToArray())}");//用空格串联字符串   ;
            }

            return sb.ToString();
        }

        public static string ReadFileALL(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            //Stopwatch sw = new Stopwatch();
            //sw.Restart();
            if (File.Exists(filePath))
            {
                //FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))//Encoding.GetEncoding("GB2312")
                    {
                        var ls = "";
                        while ((ls = sr.ReadLine()) != null)
                        {
                            sb.Append(ls);
                        }
                    }
                }
            }
            else
            {
                sb.Append($"文件不存在[{filePath}],请清除文本框后选择或者手动输入！");
            }
            //sw.Stop();
            //LogInfoFile.WriteLog("ReadFileLine", $"执行毫秒数!  {sw.ElapsedMilliseconds}", "FileRWTest");
            return sb.ToString();
        }

        /// <summary>
        /// 读取全部文件按行返回
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<string> ReadFileLine(string filePath)
        {
            List<string> fl = new List<string>();
            //Stopwatch sw = new Stopwatch();
            //sw.Restart();
            if (File.Exists(filePath))
            {
                //FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))//Encoding.GetEncoding("GB2312")
                    {
                        var ls = "";
                        while ((ls = sr.ReadLine()) != null)
                        {
                            fl.Add(ls);
                        }
                    }
                }
            }
            else
            {
                fl.Add($"文件不存在[{filePath}],请清除文本框后选择或者手动输入！");
            }
            //sw.Stop();
            //LogInfoFile.WriteLog("ReadFileLine", $"执行毫秒数!  {sw.ElapsedMilliseconds}", "FileRWTest");
            return fl;
        }

        public static void WriteFile(string action, string logfilePath = "", bool FULL = false)
        {
            try
            {
                lockRW.EnterWriteLock();//锁定
                string RZ = "ini.txt";
                if (!string.IsNullOrEmpty(logfilePath))
                {
                    RZ = logfilePath;
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var logfilepath = path + "\\" + RZ;

                if (FULL && !string.IsNullOrEmpty(logfilePath))
                {
                    logfilepath = logfilePath;
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(logfilepath)))
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logfilepath));
                    }
                }
                //File.WriteAllText(logfilepath
                //    , action
                //    , Encoding.UTF8);// Encoding.GetEncoding("UTF-8")

                File.WriteAllText(logfilepath, action, new System.Text.UTF8Encoding(false));// Encoding.GetEncoding("UTF-8")
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                lockRW.ExitWriteLock();//解锁
            }
        }

        //string str = "获取文件的全路径：" + Path.GetFullPath(filePath);   //-->C:\JiYF\BenXH\BenXHCMS.xml
        //Console.WriteLine(str);
        //str = "获取文件所在的目录：" + Path.GetDirectoryName(filePath); //-->C:\JiYF\BenXH
        //Console.WriteLine(str);
        //str = "获取文件的名称含有后缀：" + Path.GetFileName(filePath);  //-->BenXHCMS.xml
        //Console.WriteLine(str);
        //str = "获取文件的名称没有后缀：" + Path.GetFileNameWithoutExtension(filePath); //-->BenXHCMS
        //Console.WriteLine(str);
        //str = "获取路径的后缀扩展名称：" + Path.GetExtension(filePath); //-->.xml
        //Console.WriteLine(str);
        //str = "获取路径的根目录：" + Path.GetPathRoot(filePath); //-->C:\
        //Console.WriteLine(str);
        public static void WriteFileALL(string neirong, string logfilePath)
        {
            try
            {
                //lockRW.EnterWriteLock();//锁定
                if (!Directory.Exists(Path.GetDirectoryName(logfilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logfilePath));
                }
                //File.WriteAllText(logfilePath, neirong, Encoding.UTF8);// Encoding.GetEncoding("UTF-8")
                //下面的保存为不带BOM 的 utf-8
                File.WriteAllText(logfilePath, neirong, new System.Text.UTF8Encoding(false));// Encoding.GetEncoding("UTF-8")
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //lockRW.ExitWriteLock();//解锁
            }
        }

        //Environment.NewLine
        /// <summary>
        /// 将日志写入指定的文件
        /// </summary>
        /// <param name="action">事件类型</param>
        /// <param name="description">事件描述</param>
        /// <param name="logfilePath">指定的路径，不指定写入工作目录下的MLog文件夹下的RZ开头的文件中</param>
        public static void WriteLog(string action, string description, string logfilePath = "")
        {
            try
            {
                lockRW.EnterWriteLock();//锁定
                string RZ = "RZ";
                if (!string.IsNullOrEmpty(logfilePath))
                {
                    RZ += logfilePath;
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var logfilepath = path + "\\" + RZ + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                File.AppendAllText(logfilepath
                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + " | " + action + " | " + description + "\r\n"
                    , Encoding.UTF8);// Encoding.GetEncoding("UTF-8")
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                lockRW.ExitWriteLock();//解锁
            }
        }

        //string fileDir = Environment.CurrentDirectory;
        //Console.WriteLine("当前程序目录："+fileDir);

        ////一个文件目录
        //string filePath = "C:\\JiYF\\BenXH\\BenXHCMS.xml";
        //Console.WriteLine("该文件的目录："+filePath);
    }

    public class TimerExampleState
    {
        public int counter = 0;
        public string tmr;//可以保留当前计时器
    }

    public class XXTZDL
    {
        private static object _lock = new object();
        private static List<string> FHXX = new List<string>();
        private static string iffasongzhong = "0";
        private static Dictionary<string, string> ZIDIANDXDH = new Dictionary<string, string>();

        public static void ADD(string cmd)
        {
            lock (_lock)
            {
                FHXX.Add(cmd);
            }
        }

        public static void DEl(string cmd)
        {
            lock (_lock)
            {
                FHXX.Remove(cmd);
            }
        }

        public static void FSLOCK(string cmd)
        {
            lock (_lock)
            {
                iffasongzhong = cmd;
            }
        }

        public static void FSUNLOCK(string cmd)
        {
            lock (_lock)
            {
                iffasongzhong = cmd;
            }
        }

        public static string GET()
        {
            lock (_lock)
            {
                string str = string.Join(Environment.NewLine, FHXX.ToArray());
                FHXX.Clear();
                return str;
            }
        }

        public static string GETIFFASONGZHONG()
        {
            lock (_lock)
            {
                return iffasongzhong;
            }
        }

        public static void ZIDIANADD(string cmd, string id)
        {
            lock (_lock)
            {
                ZIDIANDXDH.Add(cmd, id);
            }
        }

        public static void ZIDIANDEl(string cmd)
        {
            lock (_lock)
            {
                if (ZIDIANDXDH.ContainsKey(cmd))
                {
                    ZIDIANDXDH.Remove(cmd);
                }
            }
        }

        public static string ZIDIANGET(string cmd)
        {
            lock (_lock)
            {
                if (ZIDIANDXDH.ContainsKey(cmd))
                {
                    return ZIDIANDXDH[cmd]; ;
                }
                else
                {
                    return "字典没有找到";
                }
            }
        }
    }

    public class YUNXINGSHI
    {
        public YUNXINGSHI(string mc)
        {
            MINGCHEGN = mc;
            KAISHI_SJ = DateTime.Now;
        }

        /// <summary>
        /// 记录上次异常的时间，第二次发送的时候的，要更新这个值，用户发送的
        /// </summary>
        public DateTime ERR_SJ { get; set; }

        public int JISHU { get; set; }
        public DateTime KAISHI_SJ { get; set; }
        public string MINGCHEGN { get; set; }//可以保留当前计时器
        public DateTime SHANGCI_SJ { get; set; }

        public void SETCG()
        {
            JISHU = 0;
            SHANGCI_SJ = DateTime.Now;
        }

        public void SETERR()
        {
            JISHU++;
            SHANGCI_SJ = DateTime.Now;
        }
    }
}