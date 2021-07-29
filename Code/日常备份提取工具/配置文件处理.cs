using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 数据制作
{
    public partial class 配置文件处理 : Form
    {
        public static 配置文件处理 ZDBL = null;

        private static string clname = "配置文件处理";

        private List<string> lglist = new List<string>();

        public 配置文件处理()
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

        private void button14_Click(object sender, EventArgs e)
        {
            string filename = "";
            filename = SelectFile(filename);
            textBoxSJ1.Text = filename;
            textBoxBT.Text = Path.GetFileName(filename).Split('.')[0];

            lglist = LogInfoFile.ReadFileLine(filename);
            string sb = GetNR(lglist);
            textBox4.Text = sb;

            //textBoxYUAN.Text = Path.GetDirectoryName(filename);
        }

        private void button3_Click(object sender, EventArgs e)//***修改
        {
            JObject jo = new JObject();
            jo["textBoxSJ1"] = textBoxSJ1.Text.Trim();
            jo["textBoxSJ2"] = textBoxSJ2.Text.Trim();
            jo["textBoxSJ3"] = textBoxSJ3.Text;

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
            string clnameData = System.AppDomain.CurrentDomain.BaseDirectory + "\\MLog\\" + clname + "Data.json";

            var lglistALL = LogInfoFile.ReadFileALL(clnameData);
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
            textBox4.Text = @"帮助提示信息
**多个显示";
        }

        private void buttonZX_Click(object sender, EventArgs e)
        {
            //编列存储的数据，执行修改

            string clnameData = System.AppDomain.CurrentDomain.BaseDirectory + "\\MLog\\" + clname + "Data.json";

            var lglistALL = LogInfoFile.ReadFileALL(clnameData);
            //读取json文件数据到string
            //string josnString = LogInfoFile.GetNR(lglist);
            if (lglistALL.Contains("文件不存在"))
            {
                shuchu($"数据读取失败[{lglistALL}]");
                return;
            }
            List<数据JSON> lt = Newtonsoft.Json.JsonConvert.DeserializeObject<List<数据JSON>>(lglistALL);
            foreach (var item in lt)
            {
                //listViewAlarmInfo.Items.Add(new ListViewItem(new string[] { item.时间.ToString(), item.标题, item.内容 }));
                JObject jo = JObject.Parse(item.内容);

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
                string filename = textBoxSJ1.Text;
                lglist = LogInfoFile.ReadFileLine(filename);
                lglist[int.Parse(textBoxSJ2.Text) - 1] = textBoxSJ3.Text;
                string sb = GetNR(lglist, false);
                LogInfoFile.WriteFileALL(sb, filename);
                //shuchu($"数据保存总数[{lt.Count}]成功");
            }
            shuchu($"数据读取总数[{lt.Count}]成功");
        }

        private string GetNR(List<string> listwhere, bool showhh = true)
        {
            StringBuilder sb = new StringBuilder();
            if (showhh)
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

        private void SaveData()
        {
            string clnameData = clname + "Data.json";
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
            LogInfoFile.WriteFile(josnString, clnameData);
            shuchu($"数据保存总数[{lt.Count}]成功");
        }

        private void 配置文件处理_FormClosing(object sender, FormClosingEventArgs e)
        {
            //核心缓存.DEl(clname);
        }

        private void 配置文件处理_Load(object sender, EventArgs e)
        {
            string filename = System.AppDomain.CurrentDomain.BaseDirectory + "\\MLog\\" + clname + ".json";
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