using Log;
using Newtonsoft.Json;
using Open.Nat;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Text;
using asyncServer;
using GuardProtocol;


namespace ServiceGuard
{
    public class StartService(string[] args = null)
    {
        /// <summary>
        /// Аргументы
        /// </summary>
        private string[] ARGS { get; set; } = args;
        /// <summary>
        /// Логирование
        /// </summary>
        private Logger Logger { get; set; }
        /// <summary>
        /// Параметры настройки сервера
        /// </summary>
        private class SettingsService
        {
            internal class LoadBalancerBroker
            {
                public string Host { get; set; } = "127.0.0.1";
                public int Port { get; set; } = 38889;
            }
            public int Port { get; set; } = 38888; //Порт сервера
            public bool AutoClearLog { get; set; } = true; //Очистить логи старше чем 90 дней
            public string TypeLog { get; set; } = "prod";
            public List<LoadBalancerBroker> LoadBalancerBrokers { get; set; } = [new LoadBalancerBroker()];
        }
        /// <summary>
        /// Настройки сервера
        /// </summary>
        private SettingsService settingsService { get; set; }
        /// <summary>
        /// Фоновая задача для запуска сервера
        /// </summary>
        private BackgroundWorker WorkThreadTCP { get; set; }
        /// <summary>
        /// Сервер
        /// </summary>
        private AsyncServer GuardApplication { get; set; }
        /// <summary>
        /// Параметры клиента
        /// </summary>
        private class ClientsParameter
        {
            public string IpPort { get; set; } = null; // IP адрес и порт клиента
            public string IDController { get; set; } = null; // ID клиента, сгенерированный сервером
            public int NumberObject { get; set; } = 0;
            public int Ping { get; set; } = 30;
        }
        /// <summary>
        /// Список клиентов
        /// </summary>
        private ConcurrentDictionary<string, ClientsParameter> ListConnectionClients { get; set; }
        /// <summary>
        /// Запуск приложения
        /// </summary>
        public void OnStart()
        {
            settingsService = new();
            if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}/Settings.json"))
            {
                using (var _LoadFile = new FileStream($@"{AppDomain.CurrentDomain.BaseDirectory}/Settings.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Read))
                {
                    using (var _ReaderFile = new StreamReader(_LoadFile, Encoding.UTF8))
                    {
                        settingsService = JsonConvert.DeserializeObject<SettingsService>(_ReaderFile.ReadToEnd());
                    }
                }
            }
            else
            {
                using (var _StreamFile = new FileStream($@"{AppDomain.CurrentDomain.BaseDirectory}/Settings.json", !File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}/Settings.json") ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (var _StreamWriteFile = new StreamWriter(_StreamFile, Encoding.UTF8))
                    {
                        lock (_StreamWriteFile) _StreamWriteFile.WriteLineAsync(JsonConvert.SerializeObject(settingsService));
                    }
                }
            }
            Logger = new Logger(true, settingsService.TypeLog, settingsService.AutoClearLog);
            if (GuardApplication != null && GuardApplication.Listening) { Logger.Info("ServiceGuard.OnStart", "the guard service is running"); return; }
            Logger.Info("ServiceGuard.OnStart", "initializing guard service");
            try
            {
                Logger.Info("ServiceGuard.OnStart", "launching the guard service");
                ListConnectionClients = [];
                OpenPort(settingsService.Port).ConfigureAwait(false);
                using (WorkThreadTCP = new BackgroundWorker())
                {
                    WorkThreadTCP.DoWork += (s, ev) =>
                    {
                        GuardApplication = new(IPAddress.Any, settingsService.Port)
                        {
                            KeepAliveMessageTime = 60,
                        };
                        GuardApplication.OnConnected += GuardApplication_OnConnected;
                        GuardApplication.OnDisconnected += GuardApplication_OnDisconnected;
                        GuardApplication.OnDataReceived += GuardApplication_OnDataReceived;
                        GuardApplication.OnStarted += GuardApplication_OnStarted;
                        GuardApplication.OnStopped += GuardApplication_OnStopped;
                        GuardApplication.OnError += GuardApplication_OnError;
                        GuardApplication.Start();
                    };
                    WorkThreadTCP.WorkerSupportsCancellation = true;
                    WorkThreadTCP.RunWorkerAsync();
                }
            }
            catch (Exception Ex) { Console.ForegroundColor = ConsoleColor.DarkRed; Logger.Fatal("ServiceGuard.OnStart", Ex.Message); Console.ResetColor(); }
        }
        /// <summary>
        /// Остановка приложения сервера
        /// </summary>
        public void OnStop()
        {
            if (!GuardApplication.Listening) { Logger.Info("ServiceGuard.OnStop", "the guard service is not running"); return; }
            Logger?.Info("ServiceGuard.OnStop", "stopped guard service");
            if (GuardApplication != null)
            {
                GuardApplication.Stop();
                GuardApplication.Dispose();
            }
            if (WorkThreadTCP != null)
            {
                WorkThreadTCP.CancelAsync();
                WorkThreadTCP.Dispose();
            }
            Logger.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        /// <summary>
        /// Открытие порта сервера
        /// </summary>
        private async Task OpenPort(int _port)
        {
            try
            {
                var _discoverer = new NatDiscoverer();
                var _cts = new CancellationTokenSource(10000);
                var _device = await _discoverer.DiscoverDeviceAsync(PortMapper.Upnp, _cts);
                await _device.CreatePortMapAsync(new Mapping(Protocol.Tcp, _port, _port, "ServiceGuard"));
                Logger.Info("ServiceGuard.OpenPort", $"the port {_port} is open");
            }
            catch (Exception) { Logger.Warn("ServiceGuard.OpenPort", $"the port {_port} could not be opened"); }
        }
        /// <summary>
        /// Событие ошибка на сервере
        /// </summary>
        private void GuardApplication_OnError(object sender, AsyncServer.ErrorEventArgs e) 
        {
            var _Message = settingsService.TypeLog == "prod" ? e.Exception.Message : e.Exception.ToString();
            Logger.Info("ServiceGuard.GuardApplication_OnError", $"IP: {e.IpPort}, Message: {_Message}"); 
        }
        /// <summary>
        /// Событие остановка сервера
        /// </summary>
        private void GuardApplication_OnStopped(object sender, AsyncServer.StoppedEventArgs e) => Logger.Info("ServiceGuard.GuardApplication_OnStopped", $"the guard service has been server stopped {e.IpPort}");
        /// <summary>
        /// Событие запуск сервера
        /// </summary>
        private void GuardApplication_OnStarted(object sender, AsyncServer.StartedEventArgs e) => Logger.Info("ServiceGuard.GuardApplication_OnStarted", $"the guard service has been server launched {e.IpPort}");
        /// <summary>
        /// Событие получение входящих данных
        /// </summary>
        private void GuardApplication_OnDataReceived(object sender, AsyncServer.DataReceivedEventArgs e)
        {
            if (e.Data.Length == 0) { GuardApplication.Disconnect(e.IpPort); return; }
            var _inData = Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
            if (_inData == "PING") Task.Factory.StartNew(() => MessageTest(sender, e), TaskCreationOptions.LongRunning);
            else Task.Factory.StartNew(() => MessageGuard(sender, e), TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// Событие отключения клиентов от сервера
        /// </summary>
        private void GuardApplication_OnDisconnected(object sender, AsyncServer.DisconnectedEventArgs e)
        {
            if (ListConnectionClients.TryGetValue(e.IpPort, out _)) ListConnectionClients.TryRemove(e.IpPort, out _);
            Logger.Info("ServiceGuard.GuardApplication_OnDisconnected", $"the client is disconnected IP: {e.IpPort}, Reason: {e.Reason}");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        /// <summary>
        /// Событие подключения клиентов к серверу
        /// </summary>
        private void GuardApplication_OnConnected(object sender, AsyncServer.ConnectedEventArgs e)
        {
            ListConnectionClients[e.IpPort] = new ClientsParameter() { IpPort = e.IPEndPoint.ToString() };
            if (e.IPEndPoint.Address.ToString() != "127.0.0.1") Logger.Info("ServiceGuard.GuardApplication_OnConnected", $"the client is connected IP: {e.IpPort}");
        }
        /// <summary>
        /// Обработка данных тестового сообщения
        /// </summary>
        private async void MessageTest(object sender, AsyncServer.DataReceivedEventArgs e)
        {
            await GuardApplication.SendBytesAsync(e.IpPort, Encoding.UTF8.GetBytes("PONG"), new CancellationTokenSource(new TimeSpan(0, 1, 0)).Token);
            if (GuardApplication.GetClient(e.IpPort).Connected) GuardApplication.Disconnect(e.IpPort);
        }
        /// <summary>
        /// Обработка данных сообщений протокола Guard
        /// </summary>
        private async void MessageGuard(object sender, AsyncServer.DataReceivedEventArgs e)
        {
            try
            {
                using (var Guard = new Guard())
                {
                    var _Messages = await Guard.ProcessingDecodeMessageAsync(e.Data);
                    if (_Messages != null)
                    {
                        var _Response = await Guard.ReturnPacketMessageAsync(_Messages.MessageCropped, _Messages.MessageCount, _Messages.MessageType, _Messages.MessageProtocol);
                        var _ResultMessage = await Guard.ReturnMessageAsync(_Messages.MessageCropped, _Messages.MessageType, _Messages.MessageProtocol);
                        if (_ResultMessage != null)
                        {
                            if (!ListConnectionClients.ContainsKey(e.IpPort)) ListConnectionClients[e.IpPort] = new ClientsParameter();
                            ListConnectionClients[e.IpPort].IpPort = e.IpPort;
                            ListConnectionClients[e.IpPort].IDController = _ResultMessage.IDController ?? ListConnectionClients[e.IpPort].IDController;
                            ListConnectionClients[e.IpPort].NumberObject = _ResultMessage.ObjectNumber != 0 ? _ResultMessage.ObjectNumber : ListConnectionClients[e.IpPort].NumberObject;
                            ListConnectionClients[e.IpPort].Ping = _ResultMessage.GPRSPingTime != 0 ? _ResultMessage.GPRSPingTime + 30 : ListConnectionClients[e.IpPort].Ping;
                            if (GuardApplication.GetClient(e.IpPort).DisconnectTimeOut != ListConnectionClients[e.IpPort].Ping) GuardApplication.GetClient(e.IpPort).DisconnectTimeOut = ListConnectionClients[e.IpPort].Ping;
                            if (_Messages.MessageType == 0x02)
                            {
                                Logger.Info("ServiceGuard.MessageGuard", $"{Environment.NewLine}============= IP: {e.IpPort}, TypeMessage: {BitConverter.ToString([_Messages.MessageType])} ============={Environment.NewLine}" +
                                    $"Входящая строка => {BitConverter.ToString(_Messages.MessageCropped)}{Environment.NewLine}" +
                                    $"Ответная строка => {BitConverter.ToString(_Response)}{Environment.NewLine}" +
                                    $"============= ============= =============");
                            }
                        }
                        if (_Response != null) await GuardApplication.SendBytesAsync(e.IpPort, _Response, new CancellationTokenSource(new TimeSpan(0, 1, 0)).Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (GuardApplication.GetClient(e.IpPort) != null && GuardApplication.GetClient(e.IpPort).Connected) GuardApplication.Disconnect(e.IpPort);
                Logger.Warn("ServiceGuard.MessageGuard", $"{ex.Message}");
            }
        }
    }
}
