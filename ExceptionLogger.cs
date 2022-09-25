using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PartnersPlatform.Model.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PartnersPlatform.Utility
{
    public class ExceptionLogger
    {
        private static string _path { get; set; }
        private readonly AppSettings _appSettings;
        private static string _connString = DbConnectionHelper.GetConnectionString();
        public static string getDevelop = Environment.GetEnvironmentVariable("IsDevelopment");
        public static IConfiguration Configuration { get; set; }

        public ExceptionLogger(IOptions<AppSettings> options, IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("AXADBConnectionString2");
            _appSettings = options.Value;
            _path = FinalLogPath;
        }

        private static string BasePaths(string path)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(JsonConfigFile.AppSetting);
            Configuration = builder.Build();
            string apiBasePath = Configuration[path];
            return apiBasePath;
        }

        public static string GetLiveLogPath = BasePaths(JsonConfigFile.LiveLogPath);
        public static string GetTestLogPath = BasePaths(JsonConfigFile.TestLogPath);
        public static string FinalLogPath = GetLogPath();

        private static string GetLogPath()
        {
            //var getDevelop = Environment.GetEnvironmentVariable("IsDevelopment");

            if (getDevelop != "IsDevelopment")
                return _path = BasePaths(JsonConfigFile.LiveLogPath);
            else
                return _path = BasePaths(JsonConfigFile.TestLogPath);
        }

        public static bool LogExceptionsToDB(ExceptionBag exception)
        {
            bool state = false;
            using (SqlConnection con = new SqlConnection(_connString))
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand();
                    StringBuilder builder = new StringBuilder();
                    builder.Append("INSERT INTO dbo.ExceptionLogs(Date, ExecutingOperation, InnerException, Message)");
                    builder.Append(" VALUES(@date, @exeOp, @innerOp, @msg)");

                    cmd.CommandText = builder.ToString();
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@date", exception.Date);
                    cmd.Parameters.AddWithValue("@exeOp", exception.ExecutingOperation);
                    cmd.Parameters.AddWithValue("@innerOp", exception.InnerException);
                    cmd.Parameters.AddWithValue("@msg", exception.Message);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        state = true;

                }
                catch (SqlException ex)
                {
                    ExceptionBag bag = new ExceptionBag();
                    bag.Date = DateTime.Now;
                    bag.ExecutingOperation = "LogExceptionsToDB";
                    bag.InnerException = ex.InnerException == null ? string.Empty : ex.InnerException.ToString();
                    bag.Message = ex.Message;
                    LogToFileAsync(bag);
                }
            }

            return state;
        }

        public async static void LogToFileAsync(ExceptionBag obj)
        {
            string name = "Log_For_" + DateTime.Now.ToString("dd-MM-yyyy");
            string fileName = _path + string.Format("{0}{1}", name, ".txt");

            if (File.Exists(fileName))
            {
                try
                {
                    using (FileStream sw = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Write))
                    {
                        await LogWriterAsync(sw, obj);
                    }
                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                }
            }
            else
            {
                FileStream fs = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Write);
                await LogWriterAsync(fs, obj);

            }
        }

        private async static Task LogWriterAsync(FileStream sw, ExceptionBag obj)
        {
            StringBuilder logLine = new StringBuilder();
            logLine.AppendLine("=================================================================================================================");
            logLine.AppendLine(String.Format("Executing Operation : {0}", obj.ExecutingOperation));
            logLine.AppendLine(String.Format("Message : {0}", obj.Message));
            logLine.AppendLine(String.Format("Inner Exception : {0}", obj.InnerException));
            string date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            logLine.AppendLine(String.Format("Time : {0}", date.Substring(10)));
            logLine.AppendLine(Environment.NewLine);
            logLine.AppendLine("=================================================================================================================");
            byte[] byteData = null;
            byteData = Encoding.ASCII.GetBytes(logLine.ToString());
            await sw.WriteAsync(byteData, 0, byteData.Length);
        }

        public static Task WriteToFile<T>(string methodName, string dataName, T data, bool serializeData = true)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    string serialized = string.Empty;
                    if (serializeData)
                    {
                        serialized = JsonConvert.SerializeObject(data);
                    }
                    else
                    {
                        serialized = data.ToString();
                    }

                    await WriteToFile($"{DateTime.Now} ::=> {methodName} :: {dataName} -> {serialized}");

                }
                catch (Exception)
                {

                    //to nothing;
                }
            });
            return task;
        }

        public static Task WriteToFile(string message)
        {
            var task = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var filePath = Path.Combine(_path, "logs");
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        var date = DateTime.Now.ToString("ddMMyyyy");
                        var fileName = $"service-log-{date}.txt";
                        var fileNameAndPath = Path.Combine(filePath, fileName);

                        StringBuilder logLine = new StringBuilder();
                        logLine.AppendLine("=================================================================================================================");
                        logLine.AppendLine(String.Format("Time : {0}", DateTime.Now.ToLocalTime().ToString()));
                        //logLine.AppendLine(Environment.NewLine);
                        logLine.Append(message);
                        //logLine.AppendLine("-----------------------------------------------------------------------------------------------------------------");

                        //message = $"{message} \n\n";

                        await File.AppendAllTextAsync(fileNameAndPath, logLine.ToString());
                        break;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }

            });
            return task;
        }

        public static Task LogInfo<T>(string methodName, string userName, T data, bool serializeData = true)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    string serialized = string.Empty;
                    if (serializeData)
                    {
                        serialized = JsonConvert.SerializeObject(data);
                    }
                    else
                    {
                        serialized = data.ToString();
                    }

                    await WriteToFile($"{DateTime.Now} ::=> {methodName} :: {userName} -> {serialized}");

                }
                catch (Exception)
                {

                    //to nothing;
                }
            });
            return task;
        }

        public static Task LogExceptionError(string methodName, string userName, Exception exception)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await WriteToFile($"{DateTime.Now} ::=> {methodName} :: {userName} -> {exception.Message}");
                }
                catch (Exception)
                {

                    //to nothing;
                }
            });
            return task;
        }

        public static Task LogError<T>(string methodName, string userName, T data, bool serializeData = false, bool critical = false)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    string serialized = string.Empty;
                    if (serializeData)
                    {
                        serialized = JsonConvert.SerializeObject(data);
                    }
                    else
                    {
                        serialized = data.ToString();
                    }
                    if (critical)
                    {
                        //    SEND MAIL AND ESCALATE
                    }
                    await WriteToFile($"{DateTime.Now} ::=> {methodName} :: {userName} -> {serialized}");

                }
                catch (Exception ex)
                {
                    await WriteToFile($"A logError Exceptipon occured{DateTime.Now} ::=> {methodName} :: {userName} -> {ex.Message}");
                    //to nothing;
                }
            });
            return task;
        }

    }
}
