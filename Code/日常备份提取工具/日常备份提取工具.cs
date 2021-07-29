using Newtonsoft.Json;
using Shell32;//点击并添加引用COM组件“Microsoft Shell Controls And Automation” 实现媒体歌曲属性获取
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using 数据制作;

namespace 日常备份提取工具
{
    public partial class 日常备份提取工具 : Form
    {
        public static 日常备份提取工具 ZDBL = null;

        private static string clname = "日常备份提取工具";

        private List<string> lglist = new List<string>();

        public 日常备份提取工具()
        {
            InitializeComponent();
            ZDBL = this;
        }

        public static void shuchu(string o)
        {
            if (o == "")
            {
                return;
            }
            try
            {
                if (ZDBL.IsHandleCreated)
                {
                    ZDBL.Invoke(new Action(() =>
                    {
                        ZDBL.textBox1.AppendText($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}: { o} " + Environment.NewLine);
                    }));
                }
            }
            catch (Exception ex)
            {
                LogInfoFile.WriteLog("回显异常", ex.Message);
            }
        }


        private static void ProcessGetFileBack(object a)
        {
            string[] arr = a as string[];
            string YUAN = arr[0], MUBIAO = arr[1], HULVE = arr[2];

            List<string> zongshu = new List<string>();
            FileHelper.GetFolder(YUAN, YUAN, ref zongshu, HULVE);//第二个参数暂时无用
            shuchu($"提取备份文件完成,文件总数为{zongshu.Count}");




            shuchu($"备份完成,文件总数为{zongshu.Count}");
            shuchu(Environment.NewLine + string.Join(Environment.NewLine, zongshu.ToArray()));
            //System.Diagnostics.Process.Start(MUBIAO);
        }

        /// <summary>
        /// 打包异步
        /// </summary>
        /// <param name="a"></param>
        private static void ProcessDaBao(object a)
        {
            shuchu($"打包开始");
            string[] arr = a as string[];
            string YUAN = arr[0].Trim('\\'), MUBIAO = arr[1].Trim('\\'), HULVE = arr[2];
            List<string> zongshu = new List<string>(arr[3].Split(new string[] { "\r\n" }, StringSplitOptions.None));
            //FileHelper.GetFolder(MUBIAO, MUBIAO, ref zongshu, HULVE);//第二个参数暂时无用
            shuchu($"提取打包文件完成,文件总数为--------------{zongshu.Count}");
            shuchu(string.Join(Environment.NewLine, zongshu.ToArray()));
            FileHelper.CopyFileBack(MUBIAO, YUAN, zongshu);//部署替换
            shuchu($"打包完成");
            System.Diagnostics.Process.Start(MUBIAO);
        }



        /// <summary>
        /// 部署异步
        /// </summary>
        /// <param name="a"></param>
        private static void ProcessBuShu(object a)
        {
            string[] arr = a as string[];
            string YUAN = arr[0].Trim('\\'), MUBIAO = arr[1].Trim('\\'), HULVE = arr[2];
            string backdir = "zdback_";
            List<string> zongshu = new List<string>();
            FileHelper.GetFolder(MUBIAO, MUBIAO, ref zongshu, HULVE);//第二个参数暂时无用
            shuchu($"提取部署文件完成,文件总数为--------------{zongshu.Count}");
            shuchu(string.Join(Environment.NewLine, zongshu.ToArray()));
            zongshu.RemoveAll(t => t.Contains(MUBIAO + "\\" + backdir));
            shuchu($"部署文件清除备份后,文件总数为--------------{zongshu.Count}");
            shuchu(string.Join(Environment.NewLine, zongshu.ToArray()));

            List<string> zongshuback = new List<string>();
            foreach (var item in zongshu)
            {
                zongshuback.Add(item.Replace(MUBIAO, YUAN));//建立生产环境文件备份
            }

            FileHelper.CopyFileBack(MUBIAO + "\\" + backdir + DateTime.Now.ToString("yyyy-MM-dd"), YUAN, zongshuback);//备份原a始文件

            FileHelper.CopyFileBack(YUAN, MUBIAO, zongshu);//部署替换


            //System.Diagnostics.Process.Start(MUBIAO);
        }


        private static void ProcessFileBack(object a)
        {
            string[] arr = a as string[];
            string YUAN = arr[0], MUBIAO = arr[1], HULVE = arr[2];
            int zongshu = 0;
            FileHelper.CopyFolder(YUAN, MUBIAO, ref zongshu, HULVE);
            shuchu($"备份完成,文件总数为{zongshu}");
            System.Diagnostics.Process.Start(MUBIAO);
        }

        private static void ProcessFileDelNew(object a)
        {
            string[] arr = a as string[];
            string YUAN = arr[0], MUBIAO = arr[1], HULVE = arr[2], ZDSJ = arr[3], checkBoxDELKWJJ = arr[4], isqian = arr[5];
            int zongshu = 0;

            FileHelper.DelNewFiles(YUAN, MUBIAO, ref zongshu, DateTime.Parse(ZDSJ), HULVE, bool.Parse(isqian));
            //if (bool.Parse(checkBoxDELKWJJ))
            //{
            //    for (int i = 0; i < 20; i++)//删除空的文件夹
            //    {
            //        FileHelper.KillEmptyDirectory(MUBIAO);
            //    }
            //}
            shuchu($"删除文件完成,文件总数为{zongshu}");
            if (Directory.Exists(MUBIAO))
            {
                System.Diagnostics.Process.Start(MUBIAO);
            }
            else
            {
                shuchu($"文件完不存在：{zongshu}");
            }

        }

        private static void ProcessFileGetNew(object a)
        {
            string[] arr = a as string[];
            string YUAN = arr[0], MUBIAO = arr[1], HULVE = arr[2], ZDSJ = arr[3], checkBoxDELKWJJ = arr[4];
            int zongshu = 0;

            FileHelper.GetNewFiles(YUAN, MUBIAO, ref zongshu, DateTime.Parse(ZDSJ), HULVE);
            if (bool.Parse(checkBoxDELKWJJ))
            {
                for (int i = 0; i < 20; i++)//删除空的文件夹
                {
                    FileHelper.KillEmptyDirectory(MUBIAO);
                }
            }
            shuchu($"提取改动文件完成,文件总数为{zongshu}");
            System.Diagnostics.Process.Start(MUBIAO);
        }

        private static string SelectFile(string filename)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = dialog.FileName;
            }

            return filename;
        }

        private static string SelectFolder(string filename)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filename = dialog.SelectedPath;
            }

            return filename;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = textBox1.Text.Trim().Replace("\\", "");
            dynamic jo = Newtonsoft.Json.JsonConvert.DeserializeObject(str);
            string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo);//格式化为标准格式
            textBox4.Text = josnString.Replace("\"", "\\\"");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int zongshu = 0;
            shuchu($"智能重命名开始");
            string sourcePath = textBoxFolder.Text.Trim();
            string destPath = sourcePath + "\\智能重命名" + DateTime.Now.ToString("F").Replace(":", "_");

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

            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            Shell32.Shell sh = new Shell();
            shuchu($"智能重命名前总数{files.Count}");
            files.ForEach(c =>
            {
                string fnmae = System.IO.Path.GetFileName(c);
                //if (fnmae.Contains("-"))
                {
                    Folder dir = sh.NameSpace(System.IO.Path.GetDirectoryName(c));

                    FolderItem item = dir.ParseName(System.IO.Path.GetFileName(c));
                    string newname = dir.GetDetailsOf(item, 20) + " - " + dir.GetDetailsOf(item, 21) + ".mp3";

                    if (dir.GetDetailsOf(item, 20) == "" && dir.GetDetailsOf(item, 21) == "")
                    {
                        newname = fnmae + ".mp3";
                    }
                    //string newname = fnmae.Replace(qcmc, "");
                    string destFile = Path.Combine(new string[] { destPath, newname });
                    File.Copy(c, destFile, true);//覆盖模式
                    zongshu++;
                }
            });

            shuchu($"智能重命名完成{zongshu}");
            System.Diagnostics.Process.Start(destPath);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            配置文件处理 f2 = new 配置文件处理();
            f2.Show();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //int zongshu = 0;
            string YUAN = textBoxYUAN.Text.Trim();
            string MUBIAO = textBoxMUBIAO.Text.Trim();
            string HULVE = textBoxHULVE.Text.Trim().Trim(';');
            if (checkBoxJT.Checked)
            {
                MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
            }
            try
            {
                DateTime ZDSJ = DateTime.Parse(textBoxZDSJ.Text.Trim());
                shuchu($"提取改动文件开始");
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessFileGetNew),
                       new string[] { YUAN, MUBIAO, HULVE, ZDSJ.ToString(), checkBoxDELKWJJ.Checked.ToString() });
            }
            catch (Exception ex)
            {
                shuchu($"时间格式不对{textBoxZDSJ.Text.Trim()}");
                textBoxZDSJ.Focus();
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string filename = textBoxLog.Text.Trim();
            if (string.IsNullOrEmpty(filename))
            {
                filename = SelectFile(filename);
                textBoxLog.Text = filename;
            }
            lglist = LogInfoFile.ReadFileLine(filename);
            string sb = GetNR(lglist);
            textBox4.Text = sb;
            //textBox4.Text = string.Join(Environment.NewLine, lglist.ToArray()); //用空格串联字符串   ;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (lglist.Count == 0)
            {
                shuchu($"请先选择日志文件");
            }
            string where = textBoxwhere.Text.Trim();
            shuchu($"日志文件匹配总条件：{where}");
            string[] arrsr = where.Split(';');

            var listwhere = lglist;
            shuchu($"日志文件总行数：{listwhere.Count}");
            foreach (string item in arrsr)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    listwhere = listwhere.Where(t => t.Contains(item)).ToList();
                    shuchu($"日志文件匹配子条件[{item}]后符合的行数：{listwhere.Count}");
                }
            }
            string sb = GetNR(listwhere);

            textBox4.Text = sb;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string filename = "";
            filename = SelectFolder(filename);
            textBoxYUAN.Text = filename;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string filename = "";
            filename = SelectFolder(filename);
            textBoxMUBIAO.Text = filename;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            shuchu($"备份开始,请等待。。");
            string YUAN = textBoxYUAN.Text.Trim();
            string MUBIAO = textBoxMUBIAO.Text.Trim();
            string HULVE = textBoxHULVE.Text.Trim().Trim(';');
            if (checkBoxJT.Checked)
            {
                MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
            }
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessFileBack), new string[] { YUAN, MUBIAO, HULVE });
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string filename = textBoxFile.Text.Trim();
            if (!System.IO.File.Exists(filename))
            {
                shuchu($"文件不存在：{filename}");
                return;
            }
            FileInfo fi = new FileInfo(filename);
            shuchu($"创建时间：{fi.CreationTime.ToString()} 写入文件的时间:{ fi.LastWriteTime }访问的时间:{ fi.LastAccessTime} 文件大小：{System.Math.Ceiling(fi.Length / (1024 * 1024) * 1.0)} MB");

            string[] Info = new string[7];

            Shell32.Shell sh = new Shell();

            Folder dir = sh.NameSpace(System.IO.Path.GetDirectoryName(filename));

            FolderItem item = dir.ParseName(System.IO.Path.GetFileName(filename));

            Info[0] = "歌曲名：" + dir.GetDetailsOf(item, 21);   // MP3 歌曲名

            Info[1] = "艺术家：" + dir.GetDetailsOf(item, 20);  //Authors

            Info[2] = "专  辑：" + dir.GetDetailsOf(item, 14);  // MP3 专辑

            Info[3] = dir.GetDetailsOf(item, 27);  // 获取歌曲时长

            Info[3] = "时  长：" + Info[3].Substring(Info[3].IndexOf(":") + 1);

            Info[4] = "类  型：" + dir.GetDetailsOf(item, 9);

            Info[5] = "比特率：" + dir.GetDetailsOf(item, 22);

            Info[6] = "备  注：" + dir.GetDetailsOf(item, 24);
            shuchu(string.Join(Environment.NewLine, Info));

            string newname = dir.GetDetailsOf(item, 20) + " - " + dir.GetDetailsOf(item, 21) + ".mp3";
        }

        private void button19_Click(object sender, EventArgs e)
        {
            int zongshu = 0;
            shuchu($"改名称开始");
            string sourcePath = textBoxFolder.Text.Trim();
            string destPath = sourcePath + "\\改名称" + DateTime.Now.ToString("F").Replace(":", "_");
            string qcmc = ".mp3";
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

            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            shuchu($"改名称前总数{files.Count}");
            files.ForEach(c =>
            {
                string fnmae = System.IO.Path.GetFileName(c);
                //if (fnmae.Contains("-"))
                {
                    string newname = fnmae.Replace(qcmc, " (Live).mp3");
                    string destFile = Path.Combine(new string[] { destPath, newname });
                    File.Copy(c, destFile, true);//覆盖模式
                    zongshu++;
                }
            });
            shuchu($"改名称完成{zongshu}");
            System.Diagnostics.Process.Start(destPath);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(textBoxFile.Text);
            sb.AppendLine(textBoxFolder.Text);
            sb.AppendLine("-------------");// (textBoxzxcs.Text)

            sb.AppendLine(textBoxLog.Text);
            sb.AppendLine(textBoxwhere.Text);
            sb.AppendLine(checkBoxLINE.Checked.ToString());

            sb.AppendLine(textBoxYUAN.Text);
            sb.AppendLine(textBoxMUBIAO.Text);
            sb.AppendLine(textBoxHULVE.Text);
            sb.AppendLine(checkBoxDELKWJJ.Checked.ToString());
            sb.AppendLine(textBoxZDSJ.Text);
            sb.AppendLine(checkBoxJT.Checked.ToString());

            shuchu($"页面配置保存完毕！");
            LogInfoFile.WriteFile(sb.ToString(), clname);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            int zongshu = 0;
            shuchu($"名称替换开始");
            string sourcePath = textBoxFolder.Text.Trim();
            string destPath = sourcePath + "\\名称替换" + DateTime.Now.ToString("F").Replace(":", "_");
            string qcmc = textBoxMCGZ.Text;//旧的
            string xmc = textBoxTHXM.Text;//新的名称

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

            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            shuchu($"名称替换前总数{files.Count}");
            files.ForEach(c =>
            {
                string fnmae = System.IO.Path.GetFileName(c);
                //if (fnmae.Contains("-"))
                {
                    string newname = fnmae.Replace(qcmc, xmc);
                    string destFile = Path.Combine(new string[] { destPath, newname });
                    File.Copy(c, destFile, true);//覆盖模式
                    zongshu++;
                }
            });
            shuchu($"名称替换完成{zongshu}");
            System.Diagnostics.Process.Start(destPath);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            string path = textBoxFile.Text;
            LoadChapter(path);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            档案整理 f2 = new 档案整理();
            f2.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            shuchu($"获取目录结构信息开始");

            string YUAN = textBoxYUAN.Text.Trim();
            string MUBIAO = textBoxMUBIAO.Text.Trim();
            string HULVE = textBoxHULVE.Text.Trim().Trim(';');
            //if (checkBoxJT.Checked)
            //{
            //    MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
            //}
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessGetFileBack), new string[] { YUAN, MUBIAO, HULVE });

            shuchu($"获取目录结构信息完成");
            //System.Diagnostics.Process.Start(destPath);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            shuchu($"名称互换开始");
            string sourcePath = textBoxFolder.Text.Trim();
            string destPath = sourcePath + "\\new";
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

            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            shuchu($"名称互换总数{files.Count}");
            files.ForEach(c =>
            {
                string fnmae = System.IO.Path.GetFileName(c);
                if (fnmae.Contains("-"))
                {
                    //string newname = fnmae.Substring(fnmae.IndexOf("-") + 1, fnmae.IndexOf(".") - fnmae.IndexOf("-") - 1).Trim() + " - "
                    //+ fnmae.Substring(0, fnmae.IndexOf("-")).Trim() + fnmae.Substring(fnmae.IndexOf("."));//旧的不合适，多个-可能冲突

                    //下面去掉.mp3 用截取长度更合适吧
                    string newname = fnmae.Substring(fnmae.IndexOf("-")).Replace("-", "").Trim().Replace(".mp3", "") + " - " +
 fnmae.Substring(0, fnmae.IndexOf("-")).Trim() + ".mp3";

                    string destFile = Path.Combine(new string[] { destPath, newname });
                    File.Copy(c, destFile, true);//覆盖模式
                }
            });
            shuchu($"名称互换完成");
            System.Diagnostics.Process.Start(destPath);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定开始执行删除操作吗?", "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                string YUAN = textBoxYUAN.Text.Trim();
                string MUBIAO = textBoxMUBIAO.Text.Trim();
                string HULVE = textBoxHULVE.Text.Trim().Trim(';');
                if (checkBoxJT.Checked)
                {
                    MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
                }
                bool isqian = checkBoxisqian.Checked;
                try
                {
                    DateTime ZDSJ = DateTime.Parse(textBoxZDSJ.Text.Trim());
                    shuchu($"提取改动文件开始");
                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessFileDelNew),
                           new string[] { YUAN, MUBIAO, HULVE, ZDSJ.ToString(), checkBoxDELKWJJ.Checked.ToString(), isqian.ToString() });
                }
                catch (Exception ex)
                {
                    shuchu($"时间格式不对{textBoxZDSJ.Text.Trim()}");
                    textBoxZDSJ.Focus();
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int zongshu = 0;
            shuchu($"去除名称开始");
            string sourcePath = textBoxFolder.Text.Trim();
            string destPath = sourcePath + "\\去除名称" + DateTime.Now.ToString("F").Replace(":", "_");
            string qcmc = textBoxMCGZ.Text;
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

            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            shuchu($"去除名称前总数{files.Count}");
            files.ForEach(c =>
            {
                string fnmae = System.IO.Path.GetFileName(c);
                //if (fnmae.Contains("-"))
                {
                    string newname = fnmae.Replace(qcmc, "");
                    string destFile = Path.Combine(new string[] { destPath, newname });
                    File.Copy(c, destFile, true);//覆盖模式
                    zongshu++;
                }
            });
            shuchu($"去除名称完成{zongshu}");
            System.Diagnostics.Process.Start(destPath);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string filename = "";
            filename = SelectFile(filename);
            textBoxFile.Text = filename;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string filename = "";
            filename = SelectFolder(filename);
            textBoxFolder.Text = filename;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //string hulvepath = textBoxHULVE.Text.Trim().Trim(';');
            //         string[] dd = hulvepath.Split(';');
            string MUBIAO = textBoxMUBIAO.Text.Trim();
            shuchu($"删除开始");
            FileHelper.KillEmptyDirectory(MUBIAO);
            shuchu($"删除完成");
        }

        private void buttonJSON_Click(object sender, EventArgs e)
        {
            try
            {
                string str = textBox1.Text.Trim().Replace("\\", "");
                dynamic jo = Newtonsoft.Json.JsonConvert.DeserializeObject(str);
                string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented);//格式化为标准格式
                textBox4.Text = josnString;
            }
            catch (Exception ex)
            {
                textBox4.Text = ($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}: {ex.Message}{Environment.NewLine}");
            }
        }

        private void buttonJSONNO_Click(object sender, EventArgs e)
        {
            try
            {
                string str = textBox1.Text.Trim().Replace("\\", "");
                dynamic jo = Newtonsoft.Json.JsonConvert.DeserializeObject(str);
                string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo);//格式化为标准格式
                textBox4.Text = josnString;
            }
            catch (Exception ex)
            {
                textBox4.Text = ($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}: {ex.Message}{Environment.NewLine}");
            }
        }

        /// <summary>
        /// 窗体大小改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)//最小化
            {
                this.ShowInTaskbar = false;
                this.notifyIcon1.Visible = true;
            }
        }

        private string GetNR(List<string> listwhere)
        {
            StringBuilder sb = new StringBuilder();
            if (checkBoxLINE.Checked)
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
                foreach (var item in listwhere)
                {
                    sb.AppendLine($"{item}");
                }
                //sb.AppendLine($"{string.Join(Environment.NewLine, listwhere.ToArray())}");//用空格串联字符串   ;
            }

            return sb.ToString();
        }

        private void LoadChapter(string file)
        {
            try
            {
                shuchu($"{file}章节提取开始");
                StringBuilder sb = new StringBuilder();
                StringBuilder sbneirong = new StringBuilder();
                int lineno = 1;
                //this.listBox1.Items.Clear();
                string[] patArr = { "[0-9]{1,}", "第(.*?)", "(.*?)章", "(.*?)节", "第(.*?)章", "第(.*?)回", "第(.*?)集", "第(.*?)卷", "第(.*?)部", "第(.*?)篇", "第(.*?)节", "第(.*?)季" };
                if (File.Exists(file))
                {
                    FileStream fs = new FileStream(file, FileMode.Open);
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        string strLine = sr.ReadLine(); //读取文件中的一行
                        string zhangjie = "";
                        while (strLine != null)
                        {
                            foreach (var pattern in patArr)
                            {
                                Regex reg = new Regex(pattern);
                                if (reg.IsMatch(strLine))
                                {
                                    if (strLine.Trim().Replace("    ", "").Length < 20)//超过20个字的标题，可能是小说里面出现的“书中书、文中文”...
                                    {
                                        zhangjie = ("# " + strLine.Trim().Replace("\r\n", "").Replace("\r", "").Replace("\n", "") + "  " + lineno);//添加标题行
                                    }
                                    break;
                                }
                            }
                            if (zhangjie == "")
                            {
                                sbneirong.AppendLine(strLine);
                            }
                            else
                            {
                                sbneirong.AppendLine(zhangjie);
                                sb.AppendLine(zhangjie);
                            }
                            lineno++;
                            zhangjie = "";
                            strLine = sr.ReadLine();
                            //Thread.Sleep(1);
                        }
                    }
                }

                shuchu($"章节提取完成{sb.ToString()}");

                //LogInfoFile.WriteFile(sb.ToString(), Path.GetFileNameWithoutExtension(file));
                LogInfoFile.WriteFile(sbneirong.ToString(), file.Replace(".txt", ".MD"), true);
            }
            catch (Exception ex)
            {
                shuchu($"章节提取完成{ex.Message}");
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                mymenu.Show();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void textBoxLog_DragDrop(object sender, DragEventArgs e)
        {
            textBoxLog.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void textBoxLog_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void tuichu_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Application.Exit();
        }

        private void 日常备份提取工具_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("是否退出？选否,最小化到托盘", "操作提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Dispose();
                Application.Exit();
            }
            else
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Visible = false;
                this.notifyIcon1.Visible = true;
            }
        }

        private void 日常备份提取工具_Load(object sender, EventArgs e)
        {
            try
            {
                //最小化到托盘
                // this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
                // notifyIcon1.Icon = new Icon("2221.ico");//指定一个图标
                notifyIcon1.Visible = false;
                notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
                this.SizeChanged += new System.EventHandler(this.FrmMain_SizeChanged);

                string filename = System.AppDomain.CurrentDomain.BaseDirectory + "\\MLog\\" + clname;
                if (string.IsNullOrEmpty(filename))
                {
                    filename = SelectFile(filename);
                    textBoxLog.Text = filename;
                }
                lglist = LogInfoFile.ReadFileLine(filename);
                string sb = GetNR(lglist);

                string[] ini = sb.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                textBoxFile.Text = ini[0];
                textBoxFolder.Text = ini[1];
                //textBoxzxcs.Text = ini[2];

                textBoxLog.Text = ini[3];
                textBoxwhere.Text = ini[4];
                checkBoxLINE.Checked = ini[5] == "True" ? true : false;

                textBoxYUAN.Text = ini[6];
                textBoxMUBIAO.Text = ini[7];
                textBoxHULVE.Text = ini[8];
                checkBoxDELKWJJ.Checked = ini[9] == "True" ? true : false;
                textBoxZDSJ.Text = ini[10];
                checkBoxJT.Checked = ini[11] == "True" ? true : false;

                shuchu(sb);
            }
            catch (Exception ex)
            {
                shuchu($"配置加载异常：{ex.Message}{Environment.NewLine}{ ex.StackTrace}");
            }
        }

        #region MyRegion 拖拽的属性和事件设置

        //将一个文件拖拽到窗体的某个控件时，将该控件的路径显示在该控件上，只要拿到了路径自然可以读取文件中的内容了
        //将一个控件的属性AllowDrop设置为true，然后添加DragDrop、DragEnter时间处理函数，如下：

        private void textBoxFile_DragDrop(object sender, DragEventArgs e)
        {
            textBoxFile.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void textBoxFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        #endregion MyRegion 拖拽的属性和事件设置

        private void button23_Click(object sender, EventArgs e)
        {
            string destPath = textBoxMUBIAO.Text.Trim();
            if (!Directory.Exists(destPath))
            {
                //目标目录不存在则创建
                try
                {
                    Directory.CreateDirectory(destPath);
                    shuchu($"创建目标目录：【{destPath}{ "】成功"}");
                }
                catch (Exception ex)
                {
                    shuchu($"创建目标目录失败：{ex.Message}{Environment.NewLine}{ ex.StackTrace}");
                }
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            shuchu($"部署开始");

            string YUAN = textBoxYUAN.Text.Trim();
            string MUBIAO = textBoxMUBIAO.Text.Trim();
            string HULVE = textBoxHULVE.Text.Trim().Trim(';');
            //if (checkBoxJT.Checked)
            //{
            //    MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
            //}
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessBuShu), new string[] { YUAN, MUBIAO, HULVE });

            shuchu($"部署完成");
            //System.Diagnostics.Process.Start(destPath);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            //将文本框内部的文件路径中的   源文件  替换为  目标文件的新路径， 然复制文件到新的路径里面


            string YUAN = textBoxYUAN.Text.Trim();
            string MUBIAO = textBoxMUBIAO.Text.Trim();
            string HULVE = textBoxHULVE.Text.Trim().Trim(';');
            string BWJ = textBox1.Text.Trim();
            if (checkBoxJT.Checked)
            {
                MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
            }
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessDaBao), new string[] { YUAN, MUBIAO, HULVE, BWJ });

            //System.Diagnostics.Process.Start(destPath);

        }

        private void button26_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}