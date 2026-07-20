using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.jiuhuan.plan.tools {

    public class FileHelpler {

        public static string getFileName(string path)
        {
            var file = new FileInfo(path);
            return file.Name;
        }

        public static string reader(string filePath, string fileName)
        {
            var file = new FileStream(filePath + "/" + fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            try
            {
                var streamReader = new StreamReader(file);
                string content = streamReader.ReadToEnd();
                streamReader.Close();
                return content;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                file.Close();
            }

            return null;
        }

        public static string[] readerLines(string filePath, string fileName)
        {
            var file = new FileStream(filePath + "/" + fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            try
            {
                var streamReader = new StreamReader(file);
                List<string> list = new List<string>();
                while (true)
                {
                    string line = streamReader.ReadLine();
                    if (null == line)
                        break;
                    else
                        list.Add(line);
                }
                streamReader.Close();
                return list.ToArray();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                file.Close();
            }

            return null;
        }

        /// <summary>
        /// 判断路径是否存在，目录或文件都判断
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool isExists(string path)
        {
            var ret = Directory.Exists(path);
            if (ret)
                return true;
            
            return File.Exists(path);
        }

        public static bool isExists(string filePath, string fileName)
        {
            return isExists(filePath + "/" + fileName);
        }

        public static bool isDir(string filePath, string fileName)
        {
            return isDir(filePath + "/" + fileName);
        }

        public static bool isDir(string path)
        {
            return Directory.Exists(path);
        }

        public static bool isFile(string filePath, string fileName)
        {
            return isFile(filePath + "/" + fileName);
        }

        public static bool isFile(string path)
        {
            return File.Exists(path);
        }

        public static void create(string filePath, string fileName, string content, Encoding encoding)
        {
            var dir = new DirectoryInfo(filePath);

            if (!dir.Exists)
            {
                dir.Create();
            }

            var file = new FileStream(filePath + "/" + fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            
            try
            {
                var streamWriter = new StreamWriter(file, encoding);
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                file.Close();
            }
        }

        public static void writer(string filePath, string fileName, string content, Encoding encoding)
        {
            var file = new FileStream(filePath + "/" + fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

            try
            {
                var streamWriter = new StreamWriter(file, encoding);
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                file.Close();
            }
        }

        public static void append(string filePath, string fileName, string content, Encoding encoding)
        {
            try
            {
                var streamWriter = new StreamWriter(filePath + "/" + fileName, true, encoding);
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void delete(string filePath, string fileName)
        {
            filePath = filePath + "/" + fileName;
            FileInfo file = new FileInfo(filePath);
            if(file.Exists)
                file.Delete();
        }

        public static string getValue(string name, string[] lines)
        {
            string result = "";

            foreach (var line in lines)
            {
                var index = line.IndexOf(name);
                if (-1 != index)
                {
                    result = line.Substring(name.Length + 1);
                }
            }

            return result;
        }

        public static string getValue(string name, string filePath, string fileName)
        {
            string[] lines = readerLines(filePath, fileName);
            return getValue(name, lines);
        }
    }
}
