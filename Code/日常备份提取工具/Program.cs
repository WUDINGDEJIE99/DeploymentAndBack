using System;
using System.Configuration;
using System.Windows.Forms;
using 数据制作;

namespace 日常备份提取工具
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            int gongneng = 12;
            try
            {
                string huancun_zbry = ConfigurationManager.AppSettings["INITCANSHU"];
                gongneng = int.Parse(huancun_zbry);
            }
            catch (Exception ex)
            {
                gongneng = 1;
                LogInfoFile.WriteLog("Main", $"启动初始化异常：{ex.Message}{Environment.NewLine}{ ex.StackTrace}", "Main");
            }
            switch (gongneng)
            {
                case 1: Application.Run(new 日常备份提取工具()); ; break;
                default:
                    break;
            }
        }
    }
}