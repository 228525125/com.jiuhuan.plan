using com.jiuhuan.plan.domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace com.jiuhuan.plan.tools {

    public class DaoTemplate {
        
        public static SqlException SqlException { get; set; }
        public static string SqlString { get; set; }

        public static bool ConnectionTest()
        {
            bool isCanConnectioned = false;

            SqlConnection con = new SqlConnection(GetConnectionString());
            try
            {
                //Open DataBase
                //打开数据库
                con.Open();
                isCanConnectioned = true;
            }
            catch (SqlException ex)
            {
                //Can not Open DataBase
                //打开不成功 则连接不成功
                isCanConnectioned = false;
            }
            finally
            {
                //Close DataBase
                //关闭数据库连接
                con.Close();
            }

            return isCanConnectioned;
        }

        public static Dictionary<string, object> Test(string sqlstr)
        {
            SqlException = null;
            SqlString = "";
            Dictionary<string, object> row = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            SqlDataReader data = null;
            try
            {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                data = cmd.ExecuteReader();
                if (data.Read()) return ReadRecord(data);               //循环读取SqlDataReader对象中的数据
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                SqlException = ex;
                SqlString = sqlstr;
                //MessageBox.Show(ex.ToString());
            }
            finally
            {
                data?.Close();                      //关闭SqlDataReader对象
                con?.Close();                         //关闭数据库连接
            }

            return row;
        }

        /// <summary>
        /// 格式化 SQL 参数值
        /// </summary>
        /// <param name="value">参数值</param>
        /// <returns>格式化后的 SQL 值字符串</returns>
        public static string FormatSqlValue(object value)
        {
            if (value == null)
                return "NULL";

            Type valueType = value.GetType();

            if (valueType == typeof(string))
            {
                // 字符串需要用单引号包裹，并转义单引号
                string strValue = value.ToString().Replace("'", "''");
                return $"'{strValue}'";
            }
            else if (valueType == typeof(DateTime))
            {
                // 日期时间格式化为 SQL 兼容的字符串
                DateTime dtValue = (DateTime)value;
                return $"'{dtValue:yyyy-MM-dd HH:mm:ss}'";
            }
            else if (valueType == typeof(bool))
            {
                // 布尔值转换为 1 或 0
                return (bool)value ? "1" : "0";
            }
            else
            {
                // 数值类型直接转换为字符串
                return value.ToString();
            }
        }

        public static int Save<T>(List<T> list) where T : Entity, new()
        {
            int count = 0;
            foreach (var t in list)
                count += Save(t);

            return count;
        }

        ///<summary>
        /// 插入或更新数据,无论新旧；
        /// 如果是新增对象，则依据[Keyword]判断数据库是否有这个对象
        /// </summary>
        public static int Save<T>(T obj) where T : Entity,new ()
        {
            //obj如果是新的
            if(0 == obj.FID)
            {
                //依据[Keyword]判断数据库是否有这个对象
                var bean = FindOne(obj);

                if (null != bean)
                    obj.FID = bean.FID;
            }

            return IsExist(obj) ? Update(obj) : Insert(obj);
        }

        ///<summary>
        /// 判断数据是否存在
        /// </summary>
        private static bool IsExist<T>(T obj) where T : Entity, new()
        {
            Type type = obj.GetType();
            string tableName = HandleTableName<T>(type.Name);
            string sql = $"SELECT 1 FROM  {tableName}  WHERE FID={obj.FID};";
            return IsExist(sql);
        }

        ///<summary>
        /// 更新数据
        /// </summary>
        private static int Update<T>(T obj) where T : Entity, new()
        {
            Type type = obj.GetType();
            string tableName = HandleTableName<T>(type.Name);
            List<string> content = new List<string>();

            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(IgnoreAttribute)) || Attribute.IsDefined(prop, typeof(PrimaryAttribute)))
                {
                    continue; // 跳过带 Ignore 标记的属性
                }

                object value = prop.GetValue(obj);
                if (value != null)
                {
                    // 处理字典类型属性，序列化为JSON字符串
                    if (Attribute.IsDefined(prop, typeof(SerializationAttribute)))
                    {
                        value = "'" + JsonHelper.toJson(value) + "'";
                    }

                    //判断属性是否为字符串类型或者日期类型
                    if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                    {
                        value = "'" + value + "'";
                    }

                    //判断属性是否为布尔类型
                    if (prop.PropertyType == typeof(bool))
                    {
                        value = value.ToString().ToLower() == "true" ? 1 : 0;
                    }

                    content.Add($"{prop.Name}={value}");
                }
            }

            string sql = $"UPDATE  {tableName}  SET {string.Join(",", content)} WHERE FID={obj.FID};";
            return ExecuteNonQuery(sql);
        }

        ///<summary>
        /// 插入数据
        /// </summary>
        private static int Insert<T>(T obj) where T : Entity, new()
        {
            Type type = obj.GetType();
            string tableName = HandleTableName<T>(type.Name);
            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(IgnoreAttribute)) || Attribute.IsDefined(prop, typeof(PrimaryAttribute)))
                {
                    continue; // 跳过带 Ignore、Primary 标记的属性，后者由数据库产生
                }

                object value = prop.GetValue(obj);
                if (value != null)
                {
                    // 处理字典类型属性，序列化为JSON字符串
                    if (Attribute.IsDefined(prop, typeof(SerializationAttribute)))
                    {
                        value = JsonHelper.toJson(value);
                    }

                    //判断属性是否为字符串类型或者日期类型
                    if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                    {
                        value = value.ToString();
                    }

                    //判断属性是否为布尔类型
                    if (prop.PropertyType == typeof(bool))
                    {
                        value = value.ToString().ToLower() == "true" ? 1 : 0;
                    }

                    columns.Add(prop.Name);
                    values.Add($"'{value.ToString().Replace("'", "''")}'");
                }
            }

            string sql = $"INSERT INTO  {tableName}  ({string.Join(",", columns)}) VALUES ({string.Join(",", values)});";
            return ExecuteNonQuery(sql);
        }

        ///<summary>
        /// 删除数据
        /// </summary>
        public static int Delete<T>(T obj) where T : Entity, new()
        {
            Type type = obj.GetType();
            string tableName = HandleTableName<T>(type.Name);
            string sql = $"DELETE FROM  {tableName}  WHERE FID={obj.FID};";
            return ExecuteNonQuery(sql);
        }

        public static int DeleteAll<T>() where T : Entity, new()
        {
            Type type = typeof(T);
            string tableName = HandleTableName<T>(type.Name);
            string sql = $"DELETE {tableName} ;";
            return ExecuteNonQuery(sql);
        }

        public static int DeleteAll<T>(string userName) where T : Entity, new()
        {
            Type type = typeof(T);
            string tableName = HandleTableName<T>(type.Name);
            string sql = $"DELETE  {tableName}  WHERE FUser='{userName}';";
            return ExecuteNonQuery(sql);
        }

        public static List<T> FindAll<T>() where T : Entity, new()
        {
            Type type = typeof(T);
            string tableName = HandleTableName<T>(type.Name);
            string sql = $"SELECT * FROM  {tableName} ;";
            return FindAll<T>(sql);
        }

        public static List<T> FindAllByUser<T>(string userName) where T : Entity, new()
        {
            Type type = typeof(T);
            string tableName = HandleTableName<T>(type.Name);
            string sql = $"SELECT * FROM  {tableName}  WHERE FUser='{userName}';";
            return FindAll<T>(sql);
        }

        ///<summary>
        /// 执行SQL语句，返回受影响的行数
        /// </summary>
        public static int ExecuteNonQuery(string sqlstr)
        {
            SqlException = null;
            SqlString = "";
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            int count = 0;
            try
            {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                count = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                //MessageBox.Show(ex.ToString());
                SqlException = ex;
                SqlString = sqlstr;
                count = -1;
            }
            finally
            {
                con?.Close();                         //关闭数据库连接
            }

            return count;
        }


        public static List<T> FindAll<T>(string sqlstr) where T : new()
        {
            SqlException = null;
            SqlString = "";
            List<T> list = new List<T>();
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            SqlDataReader data = null;
            try {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                data = cmd.ExecuteReader();
                if (data.HasRows)                  //判断SqlDataReader对象中是否有数据
                {
                    while (data.Read())               //循环读取SqlDataReader对象中的数据
                    {
                        list.Add(ReadRecord<T>(data));
                    }
                }
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                SqlException = ex;
                SqlString = sqlstr;
                //MessageBox.Show(ex.ToString());
            }
            finally {
                data?.Close();                      //关闭SqlDataReader对象
                con?.Close();                         //关闭数据库连接
            }

            return list;
        }

        public static List<Dictionary<string,object>> FindAll(string sqlstr)
        {
            SqlException = null;
            SqlString = "";
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            SqlDataReader data = null;
            try
            {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                data = cmd.ExecuteReader();
                if (data.HasRows)                  //判断SqlDataReader对象中是否有数据
                {
                    while (data.Read())               //循环读取SqlDataReader对象中的数据
                    {
                        list.Add(ReadRecord(data));
                    }
                }
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                SqlException = ex;
                SqlString = sqlstr;
                //MessageBox.Show(ex.ToString());
            }
            finally
            {
                data?.Close();                      //关闭SqlDataReader对象
                con?.Close();                         //关闭数据库连接
            }

            return list;
        }

        public static T FindOne<T>(T obj) where T : Entity, new()
        {
            Type type = obj.GetType();
            string tableName = HandleTableName<T>(type.Name);
            List<string> content = new List<string>();

            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(KeywordAttribute)))
                {
                    object value = prop.GetValue(obj);
                    if (value != null)
                    {
                        //判断属性是否为字符串类型或者日期类型
                        if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                        {
                            value = "'" + value + "'";
                        }

                        //判断属性是否为布尔类型
                        if (prop.PropertyType == typeof(bool))
                        {
                            value = value.ToString().ToLower() == "true" ? 1 : 0;
                        }

                        content.Add($" {prop.Name}={value}");
                    }
                }
            }

            if (0 == content.Count)
                return null;

            string sql = $"SELECT * FROM  {tableName}  WHERE {string.Join("AND", content)};";
            return FindOne<T>(sql);
        }

        public static T FindOne<T>(string sqlstr) where T : Entity, new()
        {
            T row = default(T);
            SqlException = null;
            SqlString = "";
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            SqlDataReader data = null;
            try {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                data = cmd.ExecuteReader();
                if (data.Read()) return ReadRecord<T>(data);               //循环读取SqlDataReader对象中的数据
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                SqlException = ex;
                SqlString = sqlstr;
                row = default;
                //MessageBox.Show(ex.ToString());
            }
            finally {
                data?.Close();                      //关闭SqlDataReader对象
                con?.Close();                         //关闭数据库连接
            }

            return row;
        }

        public static Dictionary<string, object> FindOne(string sqlstr)
        {
            SqlException = null;
            SqlString = "";
            Dictionary<string, object> row = new Dictionary<string, object>();
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            SqlDataReader data = null;
            try
            {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                data = cmd.ExecuteReader();
                if (data.Read()) return ReadRecord(data);               //循环读取SqlDataReader对象中的数据
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                SqlException = ex;
                SqlString = sqlstr;
                //MessageBox.Show(ex.ToString());
            }
            finally
            {
                data?.Close();                      //关闭SqlDataReader对象
                con?.Close();                         //关闭数据库连接
            }

            return row;
        }

        public static object FindOne(string sqlstr, string fieldName)
        {
            var bean = FindOne(sqlstr);
            if (bean.Count > 0)
            {
                var obj = bean[fieldName];
                return obj;
            }

            return null;
        }

        public static bool IsExist(string sqlstr)
        {
            SqlException = null;
            SqlString = "";
            SqlConnection con = new SqlConnection(GetConnectionString());
            SqlCommand cmd = null;
            SqlDataReader data = null;
            try
            {
                cmd = new SqlCommand(sqlstr, con);//创建SqlCommand对象
                if (con.State == ConnectionState.Closed) con.Open();
                data = cmd.ExecuteReader();
                return data.Read();               
            }
            catch (SqlException ex)                     //捕获数据库异常
            {
                Debug.WriteLine(ex.ToString());         //输出异常信息
                SqlException = ex;
                SqlString = sqlstr;
                //MessageBox.Show(ex.ToString());
            }
            finally
            {
                data?.Close();                      //关闭SqlDataReader对象
                con?.Close();                         //关闭数据库连接
            }

            return false;
        }

        public static string CreateTable(Type type)
        {
            // 获取ConfigurationAttribute以确定数据库名称
            string databaseName = Utility.GetAttributeValueByClass(type,"Entity", "Database") as string;
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = "plan"; // 默认使用plan
            }

            // 获取表名（类名）
            string tableName = type.Name;

            // 开始构建SQL - 移除所有GO命令，因为ExecuteNonQuery不支持
            StringBuilder sql = new StringBuilder();
            sql.AppendLine($"USE [{databaseName}]");
            sql.AppendLine($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{tableName}]') AND type in (N'U'))");
            sql.AppendLine($"DROP TABLE [{tableName}]");
            sql.AppendLine();
            sql.AppendLine($"/****** Object:  Table [{tableName}]    Script Date: {DateTime.Now:yyyy/MM/dd HH:mm:ss} ******/");
            sql.AppendLine("SET ANSI_NULLS ON");
            sql.AppendLine();
            sql.AppendLine("SET QUOTED_IDENTIFIER ON");
            sql.AppendLine();
            sql.AppendLine($"CREATE TABLE [{tableName}](");

            // 收集所有列定义
            List<string> columns = new List<string>();
            List<string> defaultConstraints = new List<string>();
            string primaryKeyColumn = "FID"; // 默认主键

            // 获取所有属性（包括基类的属性）
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (var property in properties)
            {
                // 检查是否带有Ignore特性
                var ignoreAttr = GetIgnoreAttribute(property);
                if (ignoreAttr != null)
                {
                    continue; // 跳过带有Ignore特性的属性
                }

                string columnName = property.Name;
                string columnType = GetSqlDbType(property.PropertyType);
                bool isNullable = IsNullableProperty(property.PropertyType);

                // 处理特殊列
                if (columnName == "FID")
                {
                    // FID作为自增主键
                    columns.Add("\t[FID] [int] IDENTITY(1,1) NOT NULL");
                    continue;
                }
                else if (columnName == "FDate")
                {
                    columns.Add("\t[FDate] [datetime] NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_FDate]  DEFAULT (getdate()) FOR [FDate]");
                    continue;
                }
                else if (columnName == "FUser" || columnName == "FKeyNo" || columnName == "FNote")
                {
                    if (columnName == "FNote")
                    {
                        columns.Add("\t[FNote] [nvarchar](max) NULL");
                    }
                    else
                    {
                        columns.Add($"\t[{columnName}] [nvarchar](50) NULL");
                    }
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ('') FOR [{columnName}]");
                    continue;
                }
                else if (columnName == "FStatus")
                {
                    columns.Add("\t[FStatus] [int] NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_FStatus]  DEFAULT ((0)) FOR [FStatus]");
                    continue;
                }
                else if (columnName == "FChecked")
                {
                    columns.Add("\t[FChecked] [bit] NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_FChecked]  DEFAULT ((0)) FOR [FChecked]");
                    continue;
                }

                // 根据属性类型确定SQL类型和默认值
                string defaultValue = GetDefaultValue(property.PropertyType);

                if (property.PropertyType == typeof(string))
                {
                    columns.Add($"\t[{columnName}] [nvarchar](50) NULL");
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ('{defaultValue}') FOR [{columnName}]");
                    }
                    else
                    {
                        defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ('') FOR [{columnName}]");
                    }
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(long))
                {
                    columns.Add($"\t[{columnName}] [int] NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ((0)) FOR [{columnName}]");
                }
                else if (property.PropertyType == typeof(bool))
                {
                    columns.Add($"\t[{columnName}] [bit] NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ((0)) FOR [{columnName}]");
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    columns.Add($"\t[{columnName}] [datetime] NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT (getdate()) FOR [{columnName}]");
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(double))
                {
                    columns.Add($"\t[{columnName}] [decimal](18, 2) NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ((0)) FOR [{columnName}]");
                }
                else
                {
                    // 其他类型默认为nvarchar(50)
                    columns.Add($"\t[{columnName}] [nvarchar](50) NULL");
                    defaultConstraints.Add($"ALTER TABLE [{tableName}] ADD  CONSTRAINT [DF_{tableName}_{columnName}]  DEFAULT ('') FOR [{columnName}]");
                }
            }

            // 添加所有列定义
            for (int i = 0; i < columns.Count; i++)
            {
                sql.Append(columns[i]);
                if (i < columns.Count - 1)
                {
                    sql.AppendLine(",");
                }
                else
                {
                    sql.AppendLine();
                }
            }

            // 添加主键约束
            sql.AppendLine(" CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED ");
            sql.AppendLine("(");
            sql.AppendLine("\t[FID] ASC");
            sql.AppendLine(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]");
            sql.AppendLine(") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");
            sql.AppendLine();

            // 添加默认约束
            foreach (var constraint in defaultConstraints)
            {
                sql.AppendLine(constraint);
                sql.AppendLine();
            }

            ExecuteNonQuery(sql.ToString());

            return sql.ToString();
        }

        /// <summary>
        /// 生成创建数据库表的SQL语句
        /// </summary>
        /// <returns>生成的SQL语句</returns>
        public static void CreateTable<T>() where T : Entity
        {
            Type type = typeof(T);
            CreateTable(type);
        }

        /// <summary>
        /// 获取属性或字段的Ignore特性，如果存在则返回该特性，否则返回null
        /// 同时检查属性和字段，支持特性名称省略"Attribute"后缀
        /// </summary>
        /// <param name="memberInfo">成员信息（属性或字段）</param>
        /// <returns>Ignore特性对象，如果不存在则返回null</returns>
        private static IgnoreAttribute GetIgnoreAttribute(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                return null;

            // 尝试从属性获取Ignore特性
            var ignoreAttr = memberInfo.GetCustomAttributes(typeof(IgnoreAttribute), true).FirstOrDefault() as IgnoreAttribute;

            return ignoreAttr;
        }

        /// <summary>
        /// 获取.NET类型对应的SQL数据类型
        /// </summary>
        /// <param name="type">.NET类型</param>
        /// <returns>SQL数据类型字符串</returns>
        private static string GetSqlDbType(Type type)
        {
            if (type == typeof(int) || type == typeof(long))
                return "int";
            else if (type == typeof(bool))
                return "bit";
            else if (type == typeof(DateTime))
                return "datetime";
            else if (type == typeof(string))
                return "nvarchar(50)";
            else if (type == typeof(decimal) || type == typeof(double))
                return "decimal(18, 2)";
            else
                return "nvarchar(50)";
        }

        /// <summary>
        /// 判断类型是否可为null
        /// </summary>
        /// <param name="type">.NET类型</param>
        /// <returns>是否可为null</returns>
        private static bool IsNullableProperty(Type type)
        {
            // 值类型默认不可为null，除非是Nullable<T>
            if (type.IsValueType)
            {
                return Nullable.GetUnderlyingType(type) != null;
            }
            // 引用类型可以为null
            return true;
        }

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        /// <param name="type">.NET类型</param>
        /// <returns>默认值字符串</returns>
        private static string GetDefaultValue(Type type)
        {
            if (type == typeof(string))
                return "";
            else if (type == typeof(int) || type == typeof(long))
                return "0";
            else if (type == typeof(bool))
                return "false";
            else if (type == typeof(DateTime))
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            else if (type == typeof(decimal) || type == typeof(double))
                return "0";
            else
                return "";
        }

        private static string HandleTableName<T>(string databaseName)
        {
            string database = Utility.GetAttributeValueByClass<T>("Entity", "Database") as string;
            if (string.IsNullOrEmpty(database))
            {
                return "[" + databaseName + "]";
            }
            else
            {
                return "[" + database + "].dbo.[" + databaseName + "]";
            }
        }

        private static string GetConnectionString(string databaseServer = null, string databaseUser = null, string databasePassword = null, string databaseName = null) {
            return "Server=" + (null == databaseServer ? Config.Default.database_server : databaseServer) +
                   ";uid=" + (null == databaseUser ? Config.Default.database_user : databaseUser) +
                   ";pwd=" + (null == databasePassword ? Config.Default.database_password : databasePassword) +
                   ";database=" + (null == databaseName ? Config.Default.database_name : databaseName) +
                   ";Connect Timeout="+Config.Default.database_timeout;
        }

        private static T ReadRecord<T>(SqlDataReader data) where T : new()
        {
            T record = new T();
            for (int i = 0; i < data.FieldCount; i++)
            {
                string fieldName = data.GetName(i).Trim();
                Type type = typeof(T);
                foreach (PropertyInfo info in type.GetProperties())
                {
                    if (fieldName.Equals(info.Name))
                    {
                        object fieldValue = data[fieldName];
                        Type t = fieldValue.GetType();
                        if (t.Equals(typeof(Decimal)))
                        {
                            fieldValue = Convert.ToSingle(string.Format("{0:F3}", fieldValue));
                        }
                        if (t.Equals(typeof(DBNull)))
                        {
                            fieldValue = null;
                        }
                        // 处理字典类型属性，从JSON字符串反序列化
                        if (Attribute.IsDefined(info, typeof(SerializationAttribute)))
                        {
                            if (fieldValue != null && fieldValue is string jsonString)
                            {
                                try
                                {
                                    var serializationAttr = info.GetCustomAttribute<SerializationAttribute>();
                                    Type deserializeType = serializationAttr?.DeserializeType;

                                    if (deserializeType != null && deserializeType.IsGenericType &&
                                        deserializeType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                                    {
                                        // 获取泛型参数类型
                                        var keyType = deserializeType.GetGenericArguments()[0];
                                        var valueType = deserializeType.GetGenericArguments()[1];

                                        // 根据类型进行反序列化
                                        if (keyType == typeof(DateTime) && valueType == typeof(int))
                                        {
                                            var dict = JsonHelper.toObject<Dictionary<DateTime, int>>(jsonString);
                                            info.SetValue(record, dict);
                                        }
                                        else if (keyType == typeof(string) && valueType == typeof(int))
                                        {
                                            var dict = JsonHelper.toObject<Dictionary<string, int>>(jsonString);
                                            info.SetValue(record, dict);
                                        }
                                        else if (keyType == typeof(int) && valueType == typeof(int))
                                        {
                                            var dict = JsonHelper.toObject<Dictionary<int, int>>(jsonString);
                                            info.SetValue(record, dict);
                                        }
                                        // 可以继续添加其他类型的支持
                                    }
                                    else
                                    {
                                        // 默认处理方式，尝试反序列化为Dictionary<string, object>
                                        var dict = JsonHelper.toObject<Dictionary<string, object>>(jsonString);
                                        info.SetValue(record, dict);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"反序列化字典字段 {info.Name} 失败: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            info.SetValue(record, fieldValue);
                        }
                    }
                }
            }

            return record;
        }

        private static Dictionary<string, object> ReadRecord(SqlDataReader data)
        {
            Dictionary<string, object> record = new Dictionary<string, object>();
            for (int i = 0; i < data.FieldCount; i++)
            {
                string fieldName = data.GetName(i).Trim();
                record.Add(fieldName, data[fieldName]);
            }

            return record;
        }

        public static bool HandleException(string msg, Action action)
        {
            bool result = true;
            int num = 0;
            int max = Config.Default.database_reconnect_time;
            while (null != SqlException && num < max)    //发生异常
            {
                num++;
                msg += (null != SqlException ? SqlException.ToString() + "\r\n ErorrSql:" + SqlString : "");
                printMessage(msg);
                msg = $"5秒后重新尝试连接数据库...第{num}次";
                printMessage(msg);
                for (int i = 5; i > 0; i--)
                {
                    msg = i.ToString();
                    printMessage(msg);
                    Thread.Sleep(1000);
                }
                action.Invoke();
            }

            if (num >= 5)
            {
                printError($"重新连接数据库{max}次失败！");
                result = false;
            }

            return result;
        }

        private static void printMessage(string msg)
        {
            Logger.Info(msg);
        }

        private static void printError(string msg)
        {
            msg += "\r\n";
            msg += ">>>>>程序已停止！<<<<<\r\n";
            msg += (null != SqlException ? SqlException.ToString() + "\r\n ErorrSql:" + SqlString : "");
            printMessage(msg);
        }
    }
}
