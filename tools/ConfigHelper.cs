
namespace com.jiuhuan.plan
{
    using System.Configuration;
    using System.Windows.Forms;

    internal class ConfigHelper
    {
        public static string log_upload_cloud = "log_upload_cloud";
        public static string log_server_ip = "log_server_ip";
        public static string buffer_workday_work_time_slot_1 = "buffer_workday_work_time_slot_1";
        public static string buffer_workday_work_time_slot_2 = "buffer_workday_work_time_slot_2";
        public static string buffer_current_account = "buffer_current_account";

        private static string ConfigPath = Application.StartupPath + "\\SystemInfo.config";

        public static void Init()
        {
            if (string.Empty.Equals(GetConfigKey(log_upload_cloud))) SetConfigKey(log_upload_cloud, "false");
            if (string.Empty.Equals(GetConfigKey(log_server_ip))) SetConfigKey(log_server_ip, "192.168.1.205");
            if (string.Empty.Equals(GetConfigKey(buffer_workday_work_time_slot_1))) SetConfigKey(buffer_workday_work_time_slot_1, "{}");
            if (string.Empty.Equals(GetConfigKey(buffer_workday_work_time_slot_2))) SetConfigKey(buffer_workday_work_time_slot_2, "{}");
            if (string.Empty.Equals(GetConfigKey(buffer_current_account))) SetConfigKey(buffer_current_account, "");
        }

        /// <summary>
        /// 获取配置文件指定的Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigKey(string key)
        {
            Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = ConfigPath
            }, ConfigurationUserLevel.None);

            if (ConfigurationInstance.AppSettings.Settings[key] != null)
                return ConfigurationInstance.AppSettings.Settings[key].Value;
            else
                return string.Empty;
        }

        /// <summary>
        /// 设置配置文件指定的Key，如果Key不存在则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="vls"></param>
        /// <returns></returns>
        public static bool SetConfigKey(string key, string vls)
        {
            try
            {
                Configuration ConfigurationInstance = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = ConfigPath
                }, ConfigurationUserLevel.None);

                if (ConfigurationInstance.AppSettings.Settings[key] != null)
                    ConfigurationInstance.AppSettings.Settings[key].Value = vls;
                else
                    ConfigurationInstance.AppSettings.Settings.Add(key, vls);

                ConfigurationInstance.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
