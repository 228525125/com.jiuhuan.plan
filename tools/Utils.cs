using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.model;
using com.jiuhuan.plan.system;
using com.jiuhuan.plan.tools;
using com.jiuhuan.plan.view;
using QFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Process = System.Diagnostics.Process;

namespace com.jiuhuan.plan.tools
{
    public class Utils
    {


        public static void OpenFolder(string folderPath)
        {
            // 使用Process.Start打开文件夹
            Process.Start("explorer.exe", folderPath);
        }

        public static void OpenFile(string imagePath)
        {
            try
            {
                Process.Start(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开文件：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async static void ShowProgressWindow(Action action)
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show();

            await Task.Run(action);

            // 关闭进度窗口
            progressWindow.Close();
        }

        /// <summary>
        /// 生成自定义工厂日历
        /// </summary>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="machineTeam">设备组</param>
        /// <param name="workdays">工作日</param>
        /// <param name="classes">班次</param>
        /// <returns></returns>
        public static List<Workday> GenerateCustomWorkCalendar(DateTime begin, DateTime end, string machineTeam, List<string> workdays, Dictionary<string, string> classes, float factor)
        {
            List<Workday> workdaysList = new List<Workday>();
            DateTime current = begin.Date;
            end = end.Date;

            while (current <= end)
            {
                int dayOfWeek = (int)current.DayOfWeek;

                if (workdays.Contains(Utility.ToWeekDay(dayOfWeek)))
                {
                    foreach (var cls in classes)
                    {
                        Workday wd = new Workday();
                        wd.FWorkDate = current;
                        wd.FClasses = cls.Key;
                        //wd.FWorkCenter = department;
                        wd.FMachineTeam = machineTeam;
                        wd.FDurationByHour = int.Parse(cls.Value.Split(',')[0]);
                        wd.FMultiple = int.Parse(cls.Value.Split(',')[1]);
                        wd.FDuration = (int)(int.Parse(cls.Value.Split(',')[0]) * int.Parse(cls.Value.Split(',')[1]) * 60 * 60 * factor);
                        wd.FFactor = factor;
                        workdaysList.Add(wd);
                    }
                }
                else
                {
                    foreach (var cls in classes)
                    {
                        Workday wd = new Workday();
                        wd.FWorkDate = current;
                        wd.FClasses = cls.Key;
                        //wd.FWorkCenter = department;
                        wd.FMachineTeam = machineTeam;
                        wd.FDurationByHour = 0;
                        wd.FMultiple = int.Parse(cls.Value.Split(',')[1]);
                        wd.FDuration = 0;
                        wd.FFactor = factor;
                        workdaysList.Add(wd);
                    }
                }

                current = current.AddDays(1);
            }

            return workdaysList;
        }

        /// <summary>
        /// 会关闭当前应用程序
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parameters"></param>
        public static void InvokeExe(string path, string parameters = "")
        {
            try
            {
                if (File.Exists(path) == true)
                {
                    // 创建启动对象
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    // 设置运行文件
                    startInfo.FileName = path;
                    // 设置启动参数
                    //startInfo.Arguments = parameters;
                    //设置启动动作,确保以管理员身份运行
                    startInfo.Verb = "runas";
                    // 如果不是管理员，则启动UAC
                    System.Diagnostics.Process.Start(startInfo);
                    // 退出
                    Application.Exit();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.InnerException);
            }
        }

        /// <summary>
        /// 不会关闭当前应用程序
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parameters"></param>
        public static void InvokeExe2(string path, string parameters = "")
        {
            try
            {
                if (File.Exists(path) == true)
                {
                    // 创建启动对象
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    // 设置运行文件
                    startInfo.FileName = path;
                    // 设置启动参数
                    //startInfo.Arguments = parameters;
                    //设置启动动作,确保以管理员身份运行
                    startInfo.Verb = "runas";
                    // 如果不是管理员，则启动UAC
                    System.Diagnostics.Process.Start(startInfo);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.InnerException);
            }
        }

        public static string GetMachineName()
        {
            return Environment.MachineName;
        }

        public static string GetAppDirectory()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyLocation = assembly.Location;
            return Path.GetDirectoryName(assemblyLocation);
        }

        /// <summary>
        /// 返回安装路径下的模板目录：root/template/
        /// </summary>
        /// <returns></returns>
        public static string GetTemplateDirectory()
        {
            string appDirectory = GetAppDirectory();
            return appDirectory + "\\template\\";
        }

        public static IPAddress GetLocalIPAddress()
        {
            string hostName = Dns.GetHostName(); // 获取本机主机名
            IPAddress[] addresses = Dns.GetHostAddresses(hostName); // 获取本机IP地址列表

            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork) // 筛选出IPv4地址
                {
                    return address;
                }
            }

            return null;
        }
    }
}
