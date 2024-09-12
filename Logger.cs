using System.Text;

namespace Log
{
    /// <summary>
    /// Логирование
    /// </summary>
    public class Logger : IDisposable
    {
        private bool disposed = false;
        private bool? IsFile { get; set; }
        private bool? AutoClearLogs {  get; set; }
        private string Type { get; set; }
        private Timer LoggerClearTimer { get; set; }
        /// <summary>
        /// Инициализирует новый экземпляр <see cref='Logger'/> класса логирования
        /// </summary>
        /// <param name="isFile">Флаг типа <see cref="bool"/> указывает, запускать запись лога в файл ( <see langword="true"/> - записывает в файл, <see langword="false"/> - не записывает в файл )</param>
        /// <param name="AutoClear">Количество дней для автоотчистки логов, не более 45 дней ( 0 - автоотчистка отключена )</param>
        public Logger(bool isFile = false, string type = "prod", bool autoClearLogs = true)
        {
            IsFile = isFile; AutoClearLogs = autoClearLogs; Type = type;
            if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}Logs")) Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}Logs");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Info")) Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Info");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Warn")) Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Warn");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Error")) Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Error");
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Fatal")) Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/Fatal");
            if ((bool)AutoClearLogs) LoggerClearTimer = new Timer(Clear, null, new TimeSpan(30, 0, 0, 0), new TimeSpan(30, 0, 0, 0));
        }
        /// <summary>
        /// Добавление информационной записи
        /// </summary>
        /// <param name="Target">Цель лога</param>
        /// <param name="Message">Тело сообщения</param>
        public void Info(string Target, string Message)
        {
            string ResultMessage = $"{DateTime.Now:dd.MM.yyyy H:mm:ss}|INFO|{Target}|{Message}";
            Console.WriteLine(ResultMessage);
            if (IsFile == true && Type == "debug") SaveFile("Info", ResultMessage);
        }
        /// <summary>
        /// Добавление предупреждающей записи, в консоли помечен желтым цветом
        /// </summary>
        /// <param name="Target">Цель лога</param>
        /// <param name="Message">Тело сообщения</param>
        public void Warn(string Target, string Message)
        {
            string ResultMessage = $"{DateTime.Now:dd.MM.yyyy H:mm:ss}|WARN|{Target}|{Message}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(ResultMessage);
            Console.ResetColor();
            if (IsFile == true ) SaveFile("Warn", ResultMessage);
        }
        /// <summary>
        /// Добавление записи об ошибка, в консоли помечен красным цветом
        /// </summary>
        /// <param name="Target">Цель лога</param>
        /// <param name="Message">Тело сообщения</param>
        public void Error(string Target, string Message)
        {
            string ResultMessage = $"{DateTime.Now:dd.MM.yyyy H:mm:ss}|ERROR|{Target}|{Message}";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ResultMessage);
            Console.ResetColor();
            if (IsFile == true) SaveFile("Error", ResultMessage);
        }
        /// <summary>
        /// Добавление записи критической ошибки, в консоли помечен темно-красным цветом
        /// </summary>
        /// <param name="Target">Цель лога</param>
        /// <param name="Message">Тело сообщения</param>
        public void Fatal(string Target, string Message)
        {
            string ResultMessage = $"{DateTime.Now:dd.MM.yyyy H:mm:ss}|FATAL|{Target}|{Message}";
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(ResultMessage);
            Console.ResetColor();
            if (IsFile == true) SaveFile("Fatal", ResultMessage);
        }
        /// <summary>
        /// Добавление записи, тип указывается в ручную, при этом директория создается на основе указанного типа
        /// </summary>
        /// <param name="TypeWrite">Тип лога, наименование директории</param>
        /// <param name="Target">Цель лога</param>
        /// <param name="Message">Тело сообщения</param>
        public void Write(string TypeWrite, string Target, string Message)
        {
            string ResultMessage = $"{DateTime.Now:dd.MM.yyyy H:mm:ss}|{TypeWrite.ToUpper()}|{Target}|{Message}";
            Console.WriteLine(ResultMessage);
            if (IsFile == true) SaveFile(TypeWrite, ResultMessage);
        }
        /// <summary>
        /// Добавление лога в файл
        /// </summary>
        /// <param name="Cat">Тип лога и наименование каталога</param>
        /// <param name="Message">Тело сообщения</param>
        private static void SaveFile(string Cat, string Message)
        {
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/{Cat}/{DateTime.Now:MM}")) Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/{Cat}/{DateTime.Now:MM}");
            try
            {
                using (var StreamLog = new FileStream($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/{Cat}/{DateTime.Now:MM}/{DateTime.Now:dd_MM_yyyy}.log", !File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs/{Cat}/{DateTime.Now:MM}/{DateTime.Now:dd_MM_yyyy}.log") ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (var StreamWrite = new StreamWriter(StreamLog, Encoding.UTF8))
                    {
                        lock (StreamWrite) StreamWrite.WriteLineAsync(Message);
                    }
                }
            }
            catch (IOException) { }
            catch (Exception Ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy H:mm:ss}|FATAL|SaveLogFile|{Ex.Message}|{Ex.StackTrace}");
                Console.ResetColor();
            }
        }
        /// <summary>
        /// Очистка логово старше clearOldDays дней
        /// </summary>
        public void Clear(object obj = null)
        {
            try
            {
                foreach (var Dir in Directory.GetDirectories($@"{AppDomain.CurrentDomain.BaseDirectory}Logs"))
                {
                    try
                    {
                        foreach (var Catalog in Directory.GetDirectories(Dir))
                        {
                            var _NumberMonth = Path.GetFileNameWithoutExtension(Catalog);
                            var _NumberDelete = Convert.ToInt32(DateTime.Now.ToString("MM")) - 3;
                            if (Convert.ToInt32(_NumberMonth) <= _NumberDelete) Directory.Delete(Catalog, true);
                        }
                    }
                    catch (Exception Ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"{DateTime.Now:dd.MM.yyyy H:mm:ss}|FATAL|ClearLogFile|{Ex.Message}|{Ex.StackTrace}");
                        Console.ResetColor();
                    }
                    Info("LogClear", "Logs have been cleared");
                }
            }
            catch (Exception Ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy H:mm:ss}|FATAL|ClearLogFile|{Ex.Message}|{Ex.StackTrace}");
                Console.ResetColor();
            }
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && LoggerClearTimer != null) LoggerClearTimer.Dispose();
                IsFile = null;
                AutoClearLogs = null;
                disposed = true;
                Type = null;
            }
        }
        /// <inheritdoc/>
        ~Logger() => Dispose(false);
    }
}
