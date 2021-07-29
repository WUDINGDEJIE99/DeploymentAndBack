using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace 数据制作
{
    /// <summary>
    /// 文件读写辅助类
    /// </summary>
    public class FileHelper
    {
        private const double GbCount = MbCount * 1024;
        private const double KbCount = 1024;
        private const double MbCount = KbCount * 1024;
        private const double TbCount = GbCount * 1024;

        /// <summary>
        ///大文件读写
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string BigFileWrite(string filePath)
        {
            //int bufferSize = 1024;
            //byte[] buffer = new byte[bufferSize];
            //int uploadFileLength = 0;
            //int CountLength = 0;
            //string saveFullPath = System.AppDomain.CurrentDomain.BaseDirectory + ("\\UploadFile\\") + "\\" + filePath;
            //using (FileStream fs = new FileStream(saveFullPath, FileMode.Create))
            //{
            //    while (uploadFileLength < FileUpload1.PostedFile.ContentLength)
            //    {
            //        //从输入流放进缓冲区
            //        int bytes = FileUpload1.PostedFile.InputStream.Read(buffer, 0, bufferSize);
            //        fs.Write(buffer, 0, bytes);
            //        fs.Flush(); // 字节写入文件流
            //        uploadFileLength += bytes;// 更新大小
            //    }
            //    fs.Close();
            //}
            //Label1.Text = "文件" + FileUpload1.FileName + "已经成功上传";
            //uploadFileLength = 0;
            //CountLength = 0;

            if (File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                //StreamReader sr = new StreamReader(open.FileName, Encoding.GetEncoding("GB2312"));

                //using (var sr = new StreamReader(filePath, Encoding.UTF8))//Encoding.GetEncoding("GB2312")
                using (var sr = new StreamReader(fs, Encoding.UTF8))//Encoding.GetEncoding("GB2312")
                {
                    return sr.ReadToEnd();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 取得文件编码方式
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Encoding GetEncode(byte[] buffer)
        {
            if (buffer.Length <= 0 || buffer[0] < 239)
                return Encoding.Default;
            if (buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
                return Encoding.UTF8;
            if (buffer[0] == 254 && buffer[1] == byte.MaxValue)
                return Encoding.BigEndianUnicode;
            if (buffer[0] == byte.MaxValue && buffer[1] == 254)
                return Encoding.Unicode;
            return Encoding.Default;
        }

        /// <summary>
        /// 取得文件编码
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(string path)
        {
            var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();
            fileStream.Dispose();
            var fileEncoding = GetEncode(buffer);
            return fileEncoding;
        }

        /// <summary>
        /// 自适应编码读取文本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetTxt(string path)
        {
            var fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();
            fileStream.Dispose();
            return GetTxt(buffer, GetEncode(buffer));
        }

        /// <summary>
        /// 按指定编码方式读取文本
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetTxt(byte[] buffer, Encoding encoding)
        {
            if (Equals(encoding, Encoding.UTF8))
                return encoding.GetString(buffer, 3, buffer.Length - 3);
            if (Equals(encoding, Encoding.BigEndianUnicode) || Equals(encoding, Encoding.Unicode))
                return encoding.GetString(buffer, 2, buffer.Length - 2);
            return encoding.GetString(buffer);
        }

        /// <summary>
        /// 读取全部文件文件---效率处理低
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadFileAll(string filePath)
        {
            string dddd = null;
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            if (File.Exists(filePath))
            {
                //FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))//Encoding.GetEncoding("GB2312")
                    {
                        dddd = sr.ReadToEnd();
                    }
                }
            }
            string[] fff = dddd.Split(new char[] { '\r', '\n' });
            sw.Stop();
            LogInfoFile.WriteLog("ReadFileAll", $"执行毫秒数!  {sw.ElapsedMilliseconds}", "FileRWTest");
            return dddd;
        }

        /// <summary>
        /// 读取全部文件按行进行--效率高
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<string> ReadFileLine(string filePath)
        {
            List<string> fl = new List<string>();
            Stopwatch sw = new Stopwatch();
            sw.Restart();
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
            sw.Stop();
            LogInfoFile.WriteLog("ReadFileLine", $"执行毫秒数!  {sw.ElapsedMilliseconds}", "FileRWTest");
            return fl;
        }

        #region 备份操作

        /// <summary>
        /// 复制文件夹中的所有文件夹与文件到另一个文件夹，可以设置 是否忽略的文件夹，是否复制文件，及文件名列表过滤
        /// </summary>
        /// <param name="sourcePath">源文件夹</param>
        /// <param name="destPath">目标文件夹</param>
        /// <param name="zongshu"></param>
        /// <param name="hulvepath">不复制忽略的的文件夹</param>
        /// <param name="copyfolder">是否复制文件夹，默认复制</param>
        /// <param name="tiqufilelist">是否进行相同文件名称拷贝，默认空的拷贝所有文件</param>
        public static void CopyFolder(string sourcePath, string destPath, ref int zongshu, string hulvepath = "", bool copyfolder = true, List<string> tiqufilelist = null)
        {
            if (Directory.Exists(sourcePath) && (hulvepath != "" && !PathContains(sourcePath, hulvepath)))//忽略指定文件夹
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                //zongshu = zongshu + files.Count; ;
                int dqsl = 0;
                files.ForEach(c =>
                {
                    if (tiqufilelist == null || tiqufilelist.Exists(t => t == Path.GetFileName(c)))
                    {
                        string destFile = Path.Combine(new string[] { destPath, sourcePath.Substring(sourcePath.LastIndexOf('\\') + 1) + "_" + Path.GetFileName(c) });
                        File.Copy(c, destFile, true);//覆盖模式
                        dqsl++;
                    }
                });
                zongshu = zongshu + dqsl;

                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
                foreach (var c in folders)
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //采用递归的方法实现
                    if (!copyfolder)
                    {
                        destDir = destPath;
                    }
                    CopyFolder(c, destDir, ref zongshu, hulvepath, copyfolder, tiqufilelist);
                }
                //folders.ForEach(c =>
                //{
                //    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //    //采用递归的方法实现
                //    CopyFolder(c, destDir,ref zongshu, hulvepath);
                //});
            }
            else
            {
                LogInfoFile.WriteLog("不复制文件夹", $"源文件夹【{sourcePath}】被【{hulvepath}】过滤掉");
                //throw new DirectoryNotFoundException("源目录不存在！");
            }
        }

        /// <summary>
        /// 删除指定时间前后的文件，默认删除指定时间前的文件
        /// </summary>
        /// <param name="sourcePath">源文件夹</param>
        /// <param name="destPath">目标文件夹</param>
        /// <param name="dthou"></param>
        /// <param name="hulvepath">不复制的文件夹</param>
        public static void DelNewFiles(string sourcePath, string destPath, ref int zongshu, DateTime dthou, string hulvepath = "", bool isqian = true)
        {
            if (Directory.Exists(sourcePath) && (hulvepath != "" && !PathContains(sourcePath, hulvepath)))//忽略指定文件夹
            {
                //文件夹判断生成
                //if (!Directory.Exists(destPath))
                //{
                //    //目标目录不存在则创建
                //    try
                //    {
                //        Directory.CreateDirectory(destPath);
                //    }
                //    catch (Exception ex)
                //    {
                //        throw new Exception("创建目标目录失败：" + ex.Message);
                //    }
                //}

                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                //zongshu = zongshu + files.Count; ;

                foreach (var c in files)
                {
                    FileInfo fi = new FileInfo(c);
                    if (isqian)//删除旧文件
                    {
                        if (fi.LastWriteTime < dthou)
                        {
                            zongshu++;
                            fi.Delete();
                        }
                    }
                    else
                    {
                        if (fi.LastWriteTime > dthou)
                        {
                            zongshu++;
                            fi.Delete();
                        }
                    }
                }
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
                foreach (var c in folders)
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //采用递归的方法实现
                    DelNewFiles(c, destDir, ref zongshu, dthou, hulvepath);
                }
            }
            else
            {
                LogInfoFile.WriteLog("模板复制失败", "源目录不存在！");
                //throw new DirectoryNotFoundException("源目录不存在！");
            }
        }

        public static void GetFolder(string sourcePath, string destPath, ref int zongshu, string hulvepath = "")
        {
            StringBuilder sb = new StringBuilder();
            if (Directory.Exists(sourcePath) && (hulvepath != "" && !PathContains(sourcePath, hulvepath)))//忽略指定文件夹
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                zongshu = zongshu + files.Count; ;
                files.ForEach(c =>
                {
                    string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });

                    File.Copy(c, destFile, true);//覆盖模式
                });
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
                foreach (var c in folders)
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //采用递归的方法实现
                    CopyFolder(c, destDir, ref zongshu, hulvepath);
                }
                //folders.ForEach(c =>
                //{
                //    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //    //采用递归的方法实现
                //    CopyFolder(c, destDir,ref zongshu, hulvepath);
                //});
            }
            else
            {
                LogInfoFile.WriteLog("不复制文件夹", $"源文件夹【{sourcePath}】被【{hulvepath}】过滤掉");
                //throw new DirectoryNotFoundException("源目录不存在！");
            }
        }

        /// <summary>
        /// 获取指定时间后的文件
        /// </summary>
        /// <param name="sourcePath">源文件夹</param>
        /// <param name="destPath">目标文件夹</param>
        /// <param name="dthou"></param>
        /// <param name="hulvepath">不复制的文件夹</param>
        public static void GetNewFiles(string sourcePath, string destPath, ref int zongshu, DateTime dthou, string hulvepath = "")
        {
            if (Directory.Exists(sourcePath) && (hulvepath != "" && !PathContains(sourcePath, hulvepath)))//忽略指定文件夹
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                //zongshu = zongshu + files.Count; ;

                foreach (var c in files)
                {
                    FileInfo fi = new FileInfo(c);
                    if (fi.LastWriteTime > dthou)
                    {
                        zongshu++;
                        string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                        File.Copy(c, destFile, true);//覆盖模式
                    }
                }
                //files.ForEach(c =>
                //{
                //});
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
                foreach (var c in folders)
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //采用递归的方法实现
                    GetNewFiles(c, destDir, ref zongshu, dthou, hulvepath);
                }
                //folders.ForEach(c =>
                //{
                //    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //    //采用递归的方法实现
                //    GetNewFiles(c, destDir, ref zongshu, dthou, hulvepath);
                //});
            }
            else
            {
                LogInfoFile.WriteLog("模板复制失败", "源目录不存在！");
                //throw new DirectoryNotFoundException("源目录不存在！");
            }
        }

        /// <summary>
        /// 删除掉空文件夹 所有没有子“文件系统”的都将被删除
        /// </summary>
        /// <param name="storagepath"></param>
        public static void KillEmptyDirectory(String storagepath)
        {
            DirectoryInfo dir = new DirectoryInfo(storagepath);
            DirectoryInfo[] subdirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);
            foreach (DirectoryInfo subdir in subdirs)
            {
                FileSystemInfo[] subFiles = subdir.GetFileSystemInfos();
                if (subFiles.Count() == 0)
                {
                    subdir.Delete();
                }
            }
        }

        private static bool PathContains(string sourcePath, string hulvepath)
        {
            string[] ddd = hulvepath.Split(';');
            foreach (var item in ddd)
            {
                if (sourcePath.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion 备份操作

        #region 提取文件信息
        /// <summary>
        /// 获取目录中文件的信息，去除文件夹的，可以设置 是否忽略的文件夹，是否复制文件，及文件名列表过滤
        /// </summary>
        /// <param name="sourcePath">源文件夹</param>
        /// <param name="destPath">目标文件夹--这个参数暂时无用</param>
        /// <param name="zongshu">文件集合</param>
        /// <param name="hulvepath">不复制忽略的的文件夹</param>
        /// <param name="copyfolder">是否复制文件夹，默认复制</param>
        /// <param name="tiqufilelist">是否进行相同文件名称拷贝，默认空的拷贝所有文件</param>
        public static void GetFolder(string sourcePath, string destPath, ref List<string> zongshu, string hulvepath = "", bool copyfolder = true, List<string> tiqufilelist = null)
        {
            if (Directory.Exists(sourcePath) && (hulvepath != "" && !PathContains(sourcePath, hulvepath)))//忽略指定文件夹
            {

                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                foreach (var c in files)
                {
                    if (tiqufilelist == null || tiqufilelist.Exists(t => t == Path.GetFileName(c)))
                    {
                        zongshu.Add(c);
                    }
                }
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
                foreach (var c in folders)
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //采用递归的方法实现
                    if (!copyfolder)
                    {
                        destDir = destPath;
                    }
                    GetFolder(c, destDir, ref zongshu, hulvepath, copyfolder, tiqufilelist);
                }
                //folders.ForEach(c =>
                //{
                //    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //    //采用递归的方法实现
                //    CopyFolder(c, destDir,ref zongshu, hulvepath);
                //});
            }
            else
            {
                LogInfoFile.WriteLog("不复制文件夹", $"源文件夹【{sourcePath}】被【{hulvepath}】过滤掉");
                //throw new DirectoryNotFoundException("源目录不存在！");
            }
        }

        /// <summary>
        /// 将文件列表的文件复制到指定目录下，覆盖替换
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="filelist"></param>
        public static void CopyFileBack(string sourcePath, string destPath, List<string> filelist)
        {
            if (filelist == null)
            {
                return;
            }
            string newfile = "";
            string newDirectory = "";
            foreach (var item in filelist)
            {
                newfile = item.Replace(destPath, sourcePath);//将部署文件夹替换为生产环境文件夹
                newDirectory = Path.GetDirectoryName(newfile);
                if (!Directory.Exists(newDirectory))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(newDirectory);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                if (File.Exists(item))
                {
                    File.Copy(item, newfile, true);//覆盖模式
                }
            }
        }

        /// <summary>
        /// 将文件列表的文件复制到指定目录下，覆盖替换部署
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="filelist"></param>
        public static void CopyFileBuShu(string sourcePath, string destPath, List<string> filelist)
        {
            if (filelist == null)
            {
                return;
            }
            string newfile = "";
            string newDirectory = "";
            foreach (var item in filelist)
            {
                newfile = item.Replace(destPath, sourcePath);//将部署文件夹替换为生产环境文件夹
                newDirectory = Path.GetDirectoryName(newfile);
                if (!Directory.Exists(newDirectory))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(newDirectory);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                    File.Copy(item, newfile, true);//覆盖模式
                }
            }
        }


        #endregion
    }
}