using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 数据制作
{
    public partial class 档案整理 : Form
    {
        public static 档案整理 ZDBL = null;

        private static string clname = "档案整理";

        public 档案整理()
        {
            InitializeComponent();
            ZDBL = this;
            listViewAlarmInfo.ListViewItemSorter = new ListViewIndexComparer();
            //初始化插入标记
            listViewAlarmInfo.InsertionMark.Color = Color.Red;
            listViewAlarmInfo.AllowDrop = true;
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
                        if (ZDBL.textBox1.Lines.GetUpperBound(0) > 600)
                        {
                            ZDBL.textBox1.Text = "";
                        }
                        string sz = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}: { o} ";
                        ZDBL.textBox1.AppendText(sz + Environment.NewLine);
                        if (ZDBL.checkBoxTBXS.Checked)
                        {
                            XXTZDL.ADD(clname + "——同步输出：" + sz + Environment.NewLine);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                LogInfoFile.WriteLog("回显异常", ex.Message);
            }
        }

        private static void ProcessFileBack(object a)
        {
            string[] arr = a as string[];
            string YUAN = arr[0], MUBIAO = arr[1], HULVE = arr[2];
            int zongshu = 0;
            List<string> ls = new List<string>();
            ls.Add("0.jpg");
            ls.Add("00.jpg");
            ls.Add("000.jpg");
            ls.Add("0000.jpg");
            FileHelper.CopyFolder(YUAN, MUBIAO, ref zongshu, HULVE, false, ls);
            shuchu($"提取完成,文件总数为{zongshu}");
            System.Diagnostics.Process.Start(MUBIAO);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            paixu();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string lujing = System.IO.Path.GetDirectoryName(textBox2.Text.Trim());//
            shuchu($"提取开始,请等待。。");
            string YUAN = lujing;
            string MUBIAO = lujing + "\\" + "tiqu";
            string HULVE = "tiqu";// textBoxHULVE.Text.Trim().Trim(';');
            //if (checkBoxJT.Checked)
            {
                MUBIAO += "\\" + DateTime.Now.ToString("F").Replace(":", "_");// DateTime.Now.ToString("yyyyMMdd HHmmss fff");
            }
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ProcessFileBack), new string[] { YUAN, MUBIAO, HULVE });
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //创建JObject对象
            JObject jo = new JObject();
            jo["textBoxpeizhi"] = textBoxpeizhi.Text.Trim();
            jo["checkBoxTBXS"] = checkBoxTBXS.Checked.ToString();
            jo["textBoxQKHS"] = textBoxQKHS.Text.Trim();
            jo["checkBoxCRBC"] = checkBoxCRBC.Checked.ToString();

            string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented);//格式化为标准格式
            LogInfoFile.WriteFile(josnString, clname + ".json");
            shuchu("保存配置成功");
        }

        private void button3_Click(object sender, EventArgs e)//***修改
        {
            JObject jo = new JObject();
            jo["textBoxSJ1"] = textBoxSJ1.Text.Trim();
            jo["textBoxSJ2"] = textBoxSJ2.Text.Trim();
            jo["textBoxSJ3"] = textBoxSJ3.Text.Trim();

            string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented);//格式化为标准格式
            textBox4.Text = josnString;

            数据JSON js = new 数据JSON()
            {
                时间 = DateTime.Now,
                标题 = textBoxBT.Text.Trim(),
                内容 = textBox4.Text.Trim(),
                排序 = int.Parse(textBoxSX.Text.Trim())
            };
            listViewAlarmInfo.Items.Insert(listViewAlarmInfo.SelectedItems[0].Index, new ListViewItem(new string[] { js.时间.ToString(), js.标题, js.内容 }));
            listViewAlarmInfo.Items.Remove(listViewAlarmInfo.SelectedItems[0]);
            shuchu("数据修改成功");
            if (checkBoxCRBC.Checked)
            {
                SaveData();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            JObject jo = new JObject();
            jo["textBoxSJ1"] = textBoxSJ1.Text.Trim();
            jo["textBoxSJ2"] = textBoxSJ2.Text.Trim();
            jo["textBoxSJ3"] = textBoxSJ3.Text.Trim();

            string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented);//格式化为标准格式
            textBox4.Text = josnString;

            数据JSON js = new 数据JSON()
            {
                时间 = DateTime.Now,
                标题 = textBoxBT.Text.Trim(),
                内容 = textBox4.Text.Trim(),
                排序 = int.Parse(textBoxSX.Text.Trim())
            };
            listViewAlarmInfo.Items.Add(new ListViewItem(new string[] { js.时间.ToString(), js.标题, js.内容 }));
            shuchu("数据插入成功");
            if (checkBoxCRBC.Checked)
            {
                SaveData();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            listViewAlarmInfo.Items.Remove(listViewAlarmInfo.SelectedItems[0]);
            if (checkBoxCRBC.Checked)
            {
                SaveData();
            }
        }

        private void button8_Click(object sender, EventArgs e)//数据查询
        {
            string clnameData = System.AppDomain.CurrentDomain.BaseDirectory + "\\MLog\\" + clname + "Data.json";

            var lglist = LogInfoFile.ReadFileALL(clnameData);
            //读取json文件数据到string
            //string josnString = LogInfoFile.GetNR(lglist);

            List<数据JSON> lt = Newtonsoft.Json.JsonConvert.DeserializeObject<List<数据JSON>>(lglist);
            lt = lt.Where(t => t.标题.Contains(textBoxBT.Text.Trim())).ToList();
            listViewAlarmInfo.Items.Clear();
            foreach (var item in lt)
            {
                listViewAlarmInfo.Items.Add(new ListViewItem(new string[] { item.时间.ToString(), item.标题, item.内容 }));
            }
            shuchu($"数据读取总数[{lt.Count}]成功");
        }

        private void buttonDQ_Click(object sender, EventArgs e)//数据读取
        {
            string filename = System.AppDomain.CurrentDomain.BaseDirectory + "MLog\\" + clname + "Data.json";
            var lglistALL = LogInfoFile.ReadFileALL(filename);
            //读取json文件数据到string
            //string josnString = LogInfoFile.GetNR(lglist);
            if (lglistALL.Contains("文件不存在"))
            {
                shuchu($"数据读取失败[{lglistALL}]");
                return;
            }
            List<数据JSON> lt = Newtonsoft.Json.JsonConvert.DeserializeObject<List<数据JSON>>(lglistALL);
            listViewAlarmInfo.Items.Clear();
            foreach (var item in lt)
            {
                listViewAlarmInfo.Items.Add(new ListViewItem(new string[] { item.时间.ToString(), item.标题, item.内容 }));
            }
            shuchu($"数据读取总数[{lt.Count}]成功");
        }

        private void buttonhelp_Click(object sender, EventArgs e)
        {
            textBox4.Text = @"帮助
**排序使用说明： 重点是 开始位和结束位的关系 》》》 结束位是要移动的目标，移动到开始位的后面

";
        }

        private void buttonZX_Click(object sender, EventArgs e)
        {
            string biaoti = textBoxBT.Text.Trim();
            if (biaoti == "排序")
            {
                paixu();
            }
            else
            {
                shengcheng();
            }
        }

        private void listViewAlarmInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            JObject jo = JObject.Parse(listViewAlarmInfo.SelectedItems[0].SubItems[2].Text);

            if (jo["textBoxSJ1"] != null)
            {
                textBoxSJ1.Text = jo["textBoxSJ1"].ToString();
            }
            if (jo["textBoxSJ2"] != null)
            {
                textBoxSJ2.Text = jo["textBoxSJ2"].ToString();
            }
            if (jo["textBoxSJ3"] != null)
            {
                textBoxSJ3.Text = jo["textBoxSJ3"].ToString();
            }

            string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(jo, Formatting.Indented);//格式化为标准格式
            textBox4.Text = josnString;
            textBoxBT.Text = listViewAlarmInfo.SelectedItems[0].SubItems[1].Text;

            //textBox1.Text += listViewAlarmInfo.SelectedItems[0].SubItems[1].Text;
            //textBox1.Text += ",";
        }

        private void paixu()
        {
            try
            {
                string lujing = textBox2.Text.Trim();
                int kaishi = int.Parse(textBoxSJ1.Text.Trim());
                int jieshu = int.Parse(textBoxSJ2.Text.Trim());
                if (kaishi == jieshu)
                {
                    shuchu($"排序异常：开始和结束不能相等");
                    return;
                }
                var lglist = LogInfoFile.ReadFileLine(lujing);

                #region 进行识别和排序

                //获取开始和结束对应的文本,根据超链接进行排序
                string strkaishi = "";
                string strjieshu = "";

                for (int i = 0; i < lglist.Count; i++)//获取要排序的HTMl内容的开始位置
                {
                    if (lglist[i].Contains("<a href=\"" + kaishi + "\"><div class="))
                    {
                        kaishi = i;
                        strkaishi = lglist[i];
                        break;
                    }
                }
                for (int i = 0; i < lglist.Count; i++)//获取要排序的HTMl内容的开始位置
                {
                    if (lglist[i].Contains("<a href=\"" + jieshu + "\"><div class="))
                    {
                        jieshu = i;
                        strjieshu = lglist[i];
                        break;
                    }
                }

                if (kaishi < jieshu)
                {
                    lglist.Remove(strjieshu);
                    lglist.Insert(kaishi + 1, strjieshu);
                }
                else
                {
                    lglist.Remove(strjieshu);
                    lglist.Insert(kaishi, strjieshu);
                }

                #endregion 进行识别和排序

                LogInfoFile.WriteFile(string.Join(Environment.NewLine, lglist.ToArray()), lujing, true);
                shuchu($"排序完成[{textBoxSJ2.Text.Trim()}]===>>[{textBoxSJ1.Text.Trim()}]");
            }
            catch (Exception ex)
            {
                shuchu($"排序异常：{ex.Message}{Environment.NewLine}{ ex.StackTrace}");
            }
        }

        private void SaveData()
        {
            //string clnameData = clname + "Data.json";
            List<数据JSON> lt = new List<数据JSON>();
            int index = 0;
            foreach (ListViewItem item in listViewAlarmInfo.Items)
            {
                index++;
                数据JSON ks = new 数据JSON()
                {
                    时间 = DateTime.Parse(item.SubItems[0].Text),
                    标题 = item.SubItems[1].Text,
                    内容 = item.SubItems[2].Text,
                    排序 = index
                };
                lt.Add(ks);
            }
            string josnString = Newtonsoft.Json.JsonConvert.SerializeObject(lt, Formatting.Indented);//格式化为标准格式  Formatting.Indented
            LogInfoFile.WriteFile(josnString, clname + "Data.json");
            shuchu($"档案整理总数[{lt.Count}]成功");
        }

        private void shengcheng()
        {
            try
            {
                string lujing = textBox2.Text.Trim().Trim('\\') + "\\" + DateTime.Now.ToString("F").Replace(":", "_") + "\\";
                int kaishi = int.Parse(textBoxSJ1.Text.Trim());
                int jieshu = int.Parse(textBoxSJ2.Text.Trim());
                string muban = (textBoxSJ3.Text.Trim());
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                if (jieshu < kaishi)
                {
                    shuchu($"开始值不能大于结束值：{kaishi}>{ jieshu}");
                    return;
                }
                string wenjianjia = "";
                for (int i = kaishi; i <= jieshu; i++)
                {
                    wenjianjia = lujing + i;
                    //创建文件夹
                    if (!Directory.Exists(wenjianjia))
                    {
                        //目标目录不存在则创建
                        try
                        {
                            Directory.CreateDirectory(wenjianjia);
                            sb.AppendLine(muban.Replace("@XUHAO", i + ""));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(wenjianjia + "创建目标目录失败：" + ex.Message);
                        }
                    }
                }

                shuchu($"{sb.ToString()}");
                string clnameData = System.AppDomain.CurrentDomain.BaseDirectory + "\\dangan\\" + "imgindex.html";
                string DFF = LogInfoFile.ReadFileALL(clnameData);
                //var lglist = LogInfoFile.ReadFileLine(clnameData);

                LogInfoFile.WriteFile(DFF.Replace("@MUBAN", sb.ToString()), lujing + "ZHNANSHI.html", true);
                File.Copy(clnameData.Replace("html", "css"), lujing + "imgindex.css", true);//覆盖模式
                File.Copy(clnameData.Replace("html", "js"), lujing + "imgindex.js", true);//覆盖模式
                File.Copy(clnameData.Replace("imgindex.html", "jquery.js"), lujing + "jquery.js", true);//覆盖模式
                System.Diagnostics.Process.Start(lujing.Trim('\\'));
            }
            catch (Exception ex)
            {
                shuchu($"执行异常：{ex.Message}{Environment.NewLine}{ ex.StackTrace}");
            }
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            textBox2.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void textBox2_DragEnter(object sender, DragEventArgs e)
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

        private void 档案整理_FormClosing(object sender, FormClosingEventArgs e)
        {
            //核心缓存.DEl(clname);
        }

        private void 档案整理_Load(object sender, EventArgs e)
        {
            string filename = System.AppDomain.CurrentDomain.BaseDirectory + "MLog\\" + clname + ".json";
            try
            {
                var lglist = LogInfoFile.ReadFileLine(filename);
                //读取json文件数据到string
                string josnString = LogInfoFile.GetNR(lglist);
                shuchu(josnString);
                //创建JObject对象
                JObject jo = JObject.Parse(josnString);

                //读取json
                if (jo["textBoxYUAN"] != null)
                {
                    textBoxpeizhi.Text = jo["textBoxYUAN"].ToString();
                }
                if (jo["checkBoxTBXS"] != null)
                {
                    checkBoxTBXS.Checked = jo["checkBoxTBXS"].ToString() == "True" ? true : false;
                }
                if (jo["textBoxQKHS"] != null)
                {
                    textBoxQKHS.Text = jo["textBoxQKHS"].ToString();
                }
                if (jo["checkBoxCRBC"] != null)
                {
                    checkBoxCRBC.Checked = jo["checkBoxCRBC"].ToString() == "True" ? true : false;
                }
            }
            catch (Exception ex)
            {
                shuchu($"配置加载异常：{ex.Message}{Environment.NewLine}{ ex.StackTrace}");
            }
        }

        #region ListView拖拽排序

        private void listViewAlarmInfo_DragDrop(object sender, DragEventArgs e)
        {
            // 返回插入标记的索引值
            int index = listViewAlarmInfo.InsertionMark.Index;
            // 如果插入标记不可见，则退出.
            if (index == -1)
            {
                return;
            }
            // 如果插入标记在项目的右面，使目标索引值加一
            if (listViewAlarmInfo.InsertionMark.AppearsAfterItem)
            {
                index++;
            }

            // 返回拖拽项
            ListViewItem item = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            //在目标索引位置插入一个拖拽项目的副本
            listViewAlarmInfo.Items.Insert(index, (ListViewItem)item.Clone());
            // 移除拖拽项目的原文件
            listViewAlarmInfo.Items.Remove(item);
        }

        private void listViewAlarmInfo_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        // 当鼠标离开控件时移除插入标记
        private void listViewAlarmInfo_DragLeave(object sender, EventArgs e)
        {
            listViewAlarmInfo.InsertionMark.Index = -1;
        }

        //像拖拽项目一样移动插入标记
        private void listViewAlarmInfo_DragOver(object sender, DragEventArgs e)
        {
            // 获得鼠标坐标
            Point point = listViewAlarmInfo.PointToClient(new Point(e.X, e.Y));
            // 返回离鼠标最近的项目的索引
            int index = listViewAlarmInfo.InsertionMark.NearestIndex(point);
            // 确定光标不在拖拽项目上
            if (index > -1)
            {
                Rectangle itemBounds = listViewAlarmInfo.GetItemRect(index);
                if (point.X > itemBounds.Left + (itemBounds.Width / 2))
                {
                    listViewAlarmInfo.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    listViewAlarmInfo.InsertionMark.AppearsAfterItem = false;
                }
            }
            listViewAlarmInfo.InsertionMark.Index = index;
        }

        // 当一个项目拖拽是启动拖拽操作
        private void listViewAlarmInfo_ItemDrag(object sender, ItemDragEventArgs e)
        {
            listViewAlarmInfo.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        // 对ListView里的各项根据索引进行排序
        private class ListViewIndexComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                return ((ListViewItem)x).Index - ((ListViewItem)y).Index;
            }
        }

        #endregion ListView拖拽排序
    }
}