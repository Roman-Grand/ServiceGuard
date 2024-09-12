using Newtonsoft.Json;
using System.Collections;
using System.Globalization;
using System.Text;

namespace GuardProtocol
{
    /// <summary>
    /// Протокол обмена сообщениями Guard устройств ISTOK
    /// </summary>
    public class Guard : IDisposable
    {
        /// <inheritdoc/>
        private bool Disposed = false;
        /// <summary>
        /// Список возможных каналов связи
        /// </summary>
        private readonly Dictionary<byte, string> ListChannels = new() { { 0x01, "Не используется" }, { 0x02, "SIM1 -> GPRS IP основной сервер" }, { 0x03, "SIM1 -> GPRS IP резервный сервер" }, { 0x04, "SIM1 -> CSD основной модем" }, { 0x05, "SIM1 -> CSD основной модем" }, { 0x06, "SIM1 -> CSD резервный модем" }, { 0x07, "SIM1 -> CSD резервный модем" }, { 0x08, "SIM2 -> GPRS IP основной сервер" }, { 0x09, "SIM2 -> GPRS IP резервный сервер" }, { 0x0a, "SIM2 -> CSD основной модем" }, { 0x0b, "SIM2 -> CSD основной модем" }, { 0x0c, "SIM2 -> CSD резервный модем" }, { 0x0d, "SIM2 -> CSD резервный модем" }, { 0x0e, "LAN -> IP основной сервер" }, { 0x0f, "LAN -> IP резервный сервер" } };
        /// <summary>
        /// Список типов устройств
        /// </summary>
        private readonly Dictionary<int, string> ListDeviceType = new() { { 0, "UnknownDevice" }, { 1, "ISTOK852" }, { 2, "ISTOK852E" }, { 3, "ISTOK852Pe" }, { 4, "ISTOK432" }, { 5, "SDZ" }, { 6, "ISTOK852U" }, { 7, "ISTOK430Rs" }, { 8, "ISTOK850Rs" }, { 9, "ISTOK1650Rs" }, { 10, "SKZ" }, { 13, "KipLora" }, { 14, "ISTOKDtr" }, { 15, "SkzLoraMost" }, { 36, "BolidS2000PP" }, { 80, "ISTOK432TM" }, { 250, "IstokNet" }, { 254, "MobileButton" }, { 255, "OtherDevices" } };
        /// <summary>
        /// Список состояния запроса к реле
        /// </summary>
        private readonly Dictionary<byte, string> ListRelayStatus = new() { { 0x00, "Запрос успешно обработан" }, { 0x01, "Реле отключено в конфигурации" }, { 0x02, "Неверная команда (неверные параметры запроса)" }, { 0x03, "Нет связи с расширителем по интерфейсу RS-485" }, { 0x04, "Объект с таким номером не подключен к передатчику" }, { 0x05, "Не поддерживается версией прошивки расширителя" }, { 0x06, "Выполняется предыдущая команда управления" }, { 0x07, "повторный запрос. (если пришел запрос с предыдущем значением поля  Count )" }, };
        /// <summary>
        /// Параметры декодированного входящего сообщения
        /// </summary>
        public class DecodeMessage
        {
            /// <summary>
            /// 
            /// </summary>
            public bool MessageProtocol { get; set; } = false;
            /// <summary>
            /// 
            /// </summary>
            public byte[] MessageCropped { get; set; } = null;
            /// <summary>
            /// 
            /// </summary>
            public byte MessageType { get; set; } = 0;
            /// <summary>
            /// 
            /// </summary>
            public short MessageCount { get; set; } = 0;
        }
        /// <summary>
        /// Переменная докодированного входящего сообщения
        /// </summary>
        private DecodeMessage DecodeMessages { get; set; } = null;
        /// <summary>
        /// Переменная сформированного ответного пакета сообщения
        /// </summary>
        private byte[] PacketMessage { get; set; } = null;
        /// <summary>
        /// Параметры разбора входящего сообщения
        /// </summary>
        public class ResultMessage
        {
            /// <summary>
            /// Версия протокола устройства
            /// </summary>
            public int VersionProtocol { get; set; } = 0;
            /// <summary>
            /// Версия и ревизия ПО контроллера
            /// </summary>
            public string VersionController { get; set; } = null;
            /// <summary>
            /// ID контроллера
            /// </summary>
            public string IDController { get; set; } = null;
            /// <summary>
            /// Пароль контроллера
            /// </summary>
            public uint Password { get; set; } = 0;
            /// <summary>
            /// Размер буфера сообщения
            /// </summary>
            public int SizeBufferMessage { get; set; } = 0;
            /// <summary>
            /// Количество зон устройства
            /// </summary>
            public int CountZone { get; set; } = 0;
            /// <summary>
            /// GPRS пинг тайм-аут
            /// </summary>
            public int GPRSPingTime { get; set; } = 0;
            /// <summary>
            /// CSD пинг тайм-аут
            /// </summary>
            public int CSDPingTime { get; set; } = 0;
            /// <summary>
            /// Уровень связи GSM
            /// </summary>
            public int LevelGSM { get; set; } = 0;
            /// <summary>
            /// Уровень ошибок GSM
            /// </summary>
            public int LevelErrorGSM { get; set; } = 0;
            /// <summary>
            /// Показатель напряжение устройства
            /// </summary>
            public string Voltage { get; set; } = null;
            /// <summary>
            /// Контроль тампера
            /// </summary>
            public string TamperСontrol { get; set; } = null;
            /// <summary>
            /// Состояние тампера
            /// </summary>
            public string TamperСondition { get; set; } = null;
            /// <summary>
            /// Контроль сети 220 вольт
            /// </summary>
            public string Control220v { get; set; } = null;
            /// <summary>
            /// Состояние сети 220 вольт
            /// </summary>
            public string Condition220v { get; set; } = null;
            /// <summary>
            /// Контроль напряжения 12 вольт
            /// </summary>
            public string Control12v { get; set; } = null;
            /// <summary>
            /// Состояние напряжения 12 вольт
            /// </summary>
            public string Condition12v { get; set; } = null;
            /// <summary>
            /// Резерв
            /// </summary>
            public byte Rezerve { get; set; } = 0;
            /// <summary>
            /// Активный канал связи
            /// </summary>
            public string CurrentChannel { get; set; } = null;
            /// <summary>
            /// Список установленых каналов связи
            /// </summary>
            public List<string> ListChannel { get; set; } = null;
            /// <summary>
            /// INEI GSM модуля
            /// </summary>
            public string IMEIGSM { get; set; } = null;
            /// <summary>
            /// Наименование SIM1
            /// </summary>
            public string SIM1 { get; set; } = null;
            /// <summary>
            /// Наименование SIM2
            /// </summary>
            public string SIM2 { get; set; } = null;
            /// <summary>
            /// Уникальный номер SIM1
            /// </summary>
            public string UCCIDSIM1 { get; set; } = null;
            /// <summary>
            /// Уникальный номер SIM2
            /// </summary>
            public string UCCIDSIM2 { get; set; } = null;
            /// <summary>
            /// MAC адрес ETHERNET
            /// </summary>
            public string MACETH { get; set; } = null;
            /// <summary>
            /// MAC адрес WIFI
            /// </summary>
            public string MACWIFI { get; set; } = null;
            /// <summary>
            /// Тип устройства
            /// </summary>
            public string TypeDevice { get; set; } = null;
            /// <summary>
            /// MCC код
            /// </summary>
            public string MCC { get; set; } = null;
            /// <summary>
            /// MNC код мобильной сети оператора
            /// </summary>
            public string MNC { get; set; } = null;
            /// <summary>
            /// LAC код локальной зоны
            /// </summary>
            public string LAC { get; set; } = null;
            /// <summary>
            /// CID идентификатор базовой станции
            /// </summary>
            public string CID { get; set; } = null;
            /// <summary>
            /// Версия и ревизия HW контроллера
            /// </summary>
            public string VersionHW { get; set; } = null;
            /// <summary>
            /// Номер объекта
            /// </summary>
            public int ObjectNumber { get; set; } = 0;
            /// <summary>
            /// RS адрес контроллера
            /// </summary>
            public int AddressController { get; set; } = 0;
            /// <summary>
            /// Количество параметров контроллера
            /// </summary>
            public int CountParams { get; set; } = 0;
            /// <summary>
            /// Тип канала параметра
            /// </summary>
            public byte TypeChannelParam { get; set; } = 0;
            /// <summary>
            /// Номер зоны
            /// </summary>
            public int NumberZone { get; set; } = 0;
            /// <summary>
            /// Номер раздела
            /// </summary>
            public int NumberChapter { get; set; } = 0;
            /// <summary>
            /// Неизвестные данные
            /// </summary>
            public byte[] UnknownParameter { get; set; } = null;
            /// <summary>
            /// Лист таблици интеграции контроллера
            /// </summary>
            public string ListTableIntegration { get; set; } = null;
            /// <summary>
            /// Номер записи в журнале контроллера
            /// </summary>
            public int NumberLogJournal { get; set; } = 0;
            /// <summary>
            /// Дата и время контроллера
            /// </summary>
            public string ControllerDateTime { get; set; } = null;
            /// <summary>
            /// Дата и время события
            /// </summary>
            public string EventDateTime { get; set; } = null;
            /// <summary>
            /// Номер формата сообщения
            /// </summary>
            public int NumberFormatMessage { get; set; } = 0;
            /// <summary>
            /// Атрибут события
            /// </summary>
            public int AttributeEvent { get; set; } = 0;
            /// <summary>
            /// Код события
            /// </summary>
            public int CodeEvent { get; set; } = 0;
            /// <summary>
            /// Номер реле
            /// </summary>
            public int NumberRele { get; set; } = 0;
            /// <summary>
            /// Состояние реле
            /// </summary>
            public string RelayStatus { get; set; } = null;
        }
        /// <summary>
        /// Переменная разбора входящего сообщзения
        /// </summary>
        private ResultMessage ResultMessages { get; set; } = null;
        /// <summary>
        /// Реализация параметров таблицы интеграций устройства
        /// </summary>
        private class TableIntegration
        {
            /// <summary>
            /// RS адрес контроллера
            /// </summary>
            public int AddressController { get; set; }
            /// <summary>
            /// Тип устройства
            /// </summary>
            public string TypeDevice { get; set; }
            /// <summary>
            /// Номер объекта
            /// </summary>
            public int ObjectNumber { get; set; }
            /// <summary>
            /// Дата и время конфигурации
            /// </summary>
            public string DateTimeConfig { get; set; }
            /// <summary>
            /// Версия и ревизия ПО контроллера
            /// </summary>
            public string VersionController { get; set; }
            /// <summary>
            /// Регистры устройства
            /// </summary>
            public string ChangeRegistr { get; set; }
        }
        /// <summary>
        /// Предоставляет возможность конвертации массива байт в 16,32-битное число без знака, класс не наследуется
        /// </summary>
        private class ConvertUInt
        {
            /// <summary>
            /// Возвращает 16-битовое целое число без знака, преобразованное из массива байтов с указанной позицией в массив байтов
            /// </summary>
            public static ushort ToUInt16(byte[] data, int startIndex) => (ushort)((ushort)((uint)data[startIndex] << 8) | (uint)data[startIndex + 1]);
            /// <summary>
            /// Возвращает 32-битовое целое число без знака, преобразованное из массива байтов с указанной позицией в массив байтов
            /// </summary>
            public static uint ToUInt32(byte[] data, int startIndex = 0)
            {
                uint uint32 = (uint)data[startIndex];
                for (int index = startIndex + 1; index < startIndex + 4 && index < data.Length; ++index) uint32 = uint32 << 8 | (uint)data[index];
                return uint32;
            }
        }
        /// <summary>
        /// Вычисление контрольной суммы <see langword="CRC16"/>
        /// </summary>
        private static int GetCheckSumm(byte[] IncomingBytes)
        {
            ushort crc = 0xFFFF;
            for (int x = 0; x < IncomingBytes.Length; x++)
            {
                crc ^= IncomingBytes[x];
                for (int y = 8; y != 0; y--)
                    if ((crc & 0x0001) != 0) { crc >>= 1; crc ^= 0xA001; }
                    else crc >>= 1;
            }
            return crc; // Возврат в виде lo-hi
            //return (ushort)((crc >> 8) | (crc << 8)); // Возврат в виде hi-lo
        }
        /// <summary>
        /// Обработка входящего сообщения
        /// </summary>
        public DecodeMessage ProcessingDecodeMessage(ArraySegment<byte> _Message)
        {
            DecodeMessages = new DecodeMessage();
            List<byte> _BufferMessage, _TempBufferMessage, _ActualBufferMessage, _TempBufferCheckSum;
            byte[] _CheckSum, _CroppedMessage;
            byte _TypeMessage;
            short _DeclaredSize, _MessageCount;
            if (_Message.Count == 0 || _Message == null) throw new Exception("invalid message format");
            _BufferMessage =
            [
                .. _Message.ToArray(),//Оригинальное входящие сообщение
            ]; //Буфер массива сообщения
            _TempBufferMessage = _BufferMessage.GetRange(1, _BufferMessage.Count - 1);
            _DeclaredSize = Convert.ToInt16(_BufferMessage[0]) != _TempBufferMessage.Count ? BitConverter.ToInt16([_BufferMessage[1], _BufferMessage[0]], 0) : Convert.ToInt16(_BufferMessage[0]);
            _ActualBufferMessage = _DeclaredSize != _TempBufferMessage.Count ? _BufferMessage.GetRange(2, _BufferMessage.Count - 2) : _TempBufferMessage;
            bool ProtocolNew = _DeclaredSize != _TempBufferMessage.Count;
            _TempBufferCheckSum =
            [
                0x00,
                0x00,
                .. _TempBufferMessage,
            ];
            _CheckSum = BitConverter.GetBytes(GetCheckSumm(_TempBufferCheckSum.ToArray()));//Вычиления контрольной суммы сообщения 
            //if (_ActualBufferMessage[0] == _CheckSum[1] && _ActualBufferMessage[1] == _CheckSum[0]) throw new Exception("the checksum is incorrect");//Сравнения контрольной суммы сообщения
            _ActualBufferMessage = !ProtocolNew ? _BufferMessage.GetRange(3, 2) : _BufferMessage.GetRange(4, 2);
            _MessageCount = BitConverter.ToInt16(_ActualBufferMessage.ToArray().Reverse().ToArray(), 0);//Расчет номера входящего сообщения
            _TypeMessage = !ProtocolNew ? _BufferMessage.GetRange(5, 2).ToArray()[0] : _BufferMessage.GetRange(6, 2).ToArray()[0];
            _CroppedMessage = !ProtocolNew ? _BufferMessage.GetRange(6, _BufferMessage.Count - 6).ToArray() : _BufferMessage.GetRange(7, _BufferMessage.Count - 7).ToArray();
            DecodeMessages.MessageProtocol = ProtocolNew;
            DecodeMessages.MessageCount = _MessageCount;
            DecodeMessages.MessageType = _TypeMessage;
            DecodeMessages.MessageCropped = _CroppedMessage;
            return DecodeMessages;
        }
        /// <summary>
        /// Обработка входящего сообщения асинхронно
        /// </summary>
        public async Task<DecodeMessage> ProcessingDecodeMessageAsync(ArraySegment<byte> _Message)
        {
            DecodeMessages = new DecodeMessage();
            List<byte> _BufferMessage, _TempBufferMessage, _ActualBufferMessage, _TempBufferCheckSum;
            byte[] _CheckSum, _CroppedMessage;
            byte _TypeMessage;
            short _DeclaredSize, _MessageCount;
            if (_Message.Count == 0 || _Message == null) return null;
            _BufferMessage =
            [
                .. _Message.ToArray(),//Оригинальное входящие сообщение
            ]; //Буфер массива сообщения
            _TempBufferMessage = _BufferMessage.GetRange(1, _BufferMessage.Count - 1);
            _DeclaredSize = Convert.ToInt16(_BufferMessage[0]) != _TempBufferMessage.Count ? BitConverter.ToInt16([_BufferMessage[1], _BufferMessage[0]], 0) : Convert.ToInt16(_BufferMessage[0]);
            _ActualBufferMessage = _DeclaredSize != _TempBufferMessage.Count ? _BufferMessage.GetRange(2, _BufferMessage.Count - 2) : _TempBufferMessage;
            bool ProtocolNew = _DeclaredSize != _TempBufferMessage.Count;
            _TempBufferCheckSum =
            [
                0x00,
                0x00,
                .. _TempBufferMessage,
            ];
            _CheckSum = BitConverter.GetBytes(GetCheckSumm(_TempBufferCheckSum.ToArray()));//Вычиления контрольной суммы сообщения 
            //if (_ActualBufferMessage[0] == _CheckSum[1] && _ActualBufferMessage[1] == _CheckSum[0]) throw new Exception("the checksum is incorrect");//Сравнения контрольной суммы сообщения
            _ActualBufferMessage = !ProtocolNew ? _BufferMessage.GetRange(3, 2) : _BufferMessage.GetRange(4, 2);
            _MessageCount = BitConverter.ToInt16(_ActualBufferMessage.ToArray().Reverse().ToArray(), 0);//Расчет номера входящего сообщения
            _TypeMessage = !ProtocolNew ? _BufferMessage.GetRange(5, 2).ToArray()[0] : _BufferMessage.GetRange(6, 2).ToArray()[0];
            _CroppedMessage = !ProtocolNew ? _BufferMessage.GetRange(6, _BufferMessage.Count - 6).ToArray() : _BufferMessage.GetRange(7, _BufferMessage.Count - 7).ToArray();
            DecodeMessages.MessageProtocol = ProtocolNew;
            DecodeMessages.MessageCount = _MessageCount;
            DecodeMessages.MessageType = _TypeMessage;
            DecodeMessages.MessageCropped = _CroppedMessage;
            return await Task.FromResult(DecodeMessages);
        }
        /// <summary>
        /// Формирование параметров входящего сообщения
        /// </summary>
        /// <param name="_CroppedMessage">Сокращенное входящие сообщение</param>
        /// <param name="_TypeMessage">Флаг типа <see cref="byte"/> для определения типа запроса и формирования исходящего пакета, например: <see langword="0x0a"/>, <see langword="0x06"/>, <see langword="0x0e"/></param>
        /// <param name="_ProtocolNew">Флаг типа <see cref="bool"/> указывающий на актуальнойть версии протокола ( <see langword="true"/> - Актуальная версия, <see langword="false"/> - Старая версия )</param>
        /// <returns></returns>
        public ResultMessage ReturnMessage(byte[] _CroppedMessage, byte _TypeMessage, bool _ProtocolNew)
        {
            ResultMessages = new ResultMessage();
            List<TableIntegration> _ListTableIntegration = [];
            List<byte> _BufferMessage = new List<byte>(_CroppedMessage.ToArray()), _GenerateListChannel, _BufferByteList = [];
            List<byte[]> _ResultByteList = [];
            List<string> _Channels = [];
            int _Count = 0;
            switch (_TypeMessage)
            {
                // Инициализация
                case 0x0a:
                    ResultMessages.VersionProtocol = _BufferMessage[0];
                    ResultMessages.VersionController = $"{_BufferMessage[1]}.{_BufferMessage[2]}";
                    ResultMessages.IDController = $"{_BufferMessage[3]}.{_BufferMessage[4]}.{_BufferMessage[5]}.{_BufferMessage[6]}";
                    ResultMessages.Password = !_ProtocolNew ? ConvertUInt.ToUInt32(_BufferMessage.GetRange(7, 3).ToArray(), 0) : ConvertUInt.ToUInt32(_BufferMessage.GetRange(7, 4).ToArray(), 0);
                    ResultMessages.SizeBufferMessage = !_ProtocolNew ? ConvertUInt.ToUInt16(_BufferMessage.GetRange(10, 2).ToArray(), 0) : ConvertUInt.ToUInt16(_BufferMessage.GetRange(11, 2).ToArray(), 0);
                    ResultMessages.CountZone = !_ProtocolNew ? _BufferMessage[12] : _BufferMessage[13];
                    ResultMessages.GPRSPingTime = !_ProtocolNew ? ConvertUInt.ToUInt16(_BufferMessage.GetRange(13, 2).ToArray(), 0) : ConvertUInt.ToUInt16(_BufferMessage.GetRange(14, 2).ToArray(), 0);
                    ResultMessages.CSDPingTime = !_ProtocolNew ? ConvertUInt.ToUInt16(_BufferMessage.GetRange(15, 2).ToArray(), 0) : ConvertUInt.ToUInt16(_BufferMessage.GetRange(16, 2).ToArray(), 0);
                    ResultMessages.LevelGSM = !_ProtocolNew ? _BufferMessage[17] * 100 / 32 : _BufferMessage[18] * 100 / 32;
                    ResultMessages.LevelErrorGSM = !_ProtocolNew ? _BufferMessage[18] * 100 / 9 : _BufferMessage[19] * 100 / 9;
                    ResultMessages.Voltage = !_ProtocolNew ? $"{Convert.ToSingle((decimal)_BufferMessage[19] / 10)} v" : $"{Convert.ToSingle((decimal)_BufferMessage[20] / 10)} v";
                    ResultMessages.TamperСontrol = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[0] ? "Контролируется " : "Не контролируется") : (new BitArray(new byte[] { _BufferMessage[21] })[0] ? "Контролируется " : "Не контролируется");
                    ResultMessages.TamperСondition = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[1] ? "Открыт " : "Закрыт") : (new BitArray(new byte[] { _BufferMessage[21] })[1] ? "Открыт " : "Закрыт");
                    ResultMessages.Control220v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[2] ? "Контролируется " : "Не контролируется") : (new BitArray(new byte[] { _BufferMessage[21] })[2] ? "Контролируется " : "Не контролируется");
                    ResultMessages.Condition220v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме") : (new BitArray(new byte[] { _BufferMessage[21] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме");
                    ResultMessages.Control12v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[4] ? "Контролируется" : "Не контролируется") : (new BitArray(new byte[] { _BufferMessage[21] })[4] ? "Контролируется" : "Не контролируется");
                    ResultMessages.Condition12v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме") : (new BitArray(new byte[] { _BufferMessage[21] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме");
                    ResultMessages.Rezerve = !_ProtocolNew ? _BufferMessage[21] : _BufferMessage[22];
                    _GenerateListChannel = !_ProtocolNew ? _BufferMessage.GetRange(23, 12) : _BufferMessage.GetRange(24, 12);
                    _GenerateListChannel.ToList().ForEach(x => _Channels.Add(ListChannels[x]));
                    ResultMessages.ListChannel = _Channels;
                    ResultMessages.CurrentChannel = ResultMessages.ListChannel != null ? !_ProtocolNew ? ResultMessages.ListChannel[_BufferMessage[22] - 1] : ResultMessages.ListChannel[_BufferMessage[23 - 1]] : null;
                    ResultMessages.IMEIGSM = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(36, 15).ToArray()) : null;
                    ResultMessages.SIM1 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(51, 16).ToArray()) : null;
                    ResultMessages.SIM2 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(67, 16).ToArray()) : null;
                    ResultMessages.UCCIDSIM1 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(83, 20).ToArray()) : null;
                    ResultMessages.UCCIDSIM2 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(103, 20).ToArray()) : null;
                    ResultMessages.MACETH = _ProtocolNew ? BitConverter.ToString(_BufferMessage.GetRange(123, 6).ToArray()).Replace('-', ':') : null;
                    ResultMessages.MACWIFI = _ProtocolNew ? BitConverter.ToString(_BufferMessage.GetRange(129, 6).ToArray()).Replace('-', ':') : null;
                    ResultMessages.TypeDevice = _ProtocolNew ? $"{ListDeviceType[_BufferMessage[135]]}" : null;
                    ResultMessages.MCC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(136, 3).ToArray()) : null;
                    ResultMessages.MNC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(139, 2).ToArray()) : null;
                    ResultMessages.LAC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(141, 4).ToArray()) : null;
                    ResultMessages.LAC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(145, 4).ToArray()) : null;
                    ResultMessages.VersionHW = _ProtocolNew ? $"{Convert.ToInt16(_BufferMessage[149])}.{Convert.ToInt16(_BufferMessage[150])} ver" : null;
                    break;
                // Новое сообщение
                case 0x02:
                    ResultMessages.NumberLogJournal = ConvertUInt.ToUInt16(_BufferMessage.GetRange(0, 2).ToArray(), 0);
                    ResultMessages.EventDateTime = $"{DateTime.Parse($"{_BufferMessage[2]}-{_BufferMessage[3]}-{_BufferMessage[4]} {_BufferMessage[5]}:{_BufferMessage[6]}:{_BufferMessage[7]}", new CultureInfo("ru-RU")):dd.MM.yyyy HH:mm:ss}";
                    ResultMessages.ControllerDateTime = $"{DateTime.Parse($"{_BufferMessage[8]}-{_BufferMessage[9]}-{_BufferMessage[10]} {_BufferMessage[11]}:{_BufferMessage[12]}:{_BufferMessage[13]}", new CultureInfo("ru-RU")):dd.MM.yyyy HH:mm:ss}";
                    ResultMessages.AddressController = _BufferMessage[14];
                    ResultMessages.ObjectNumber = ConvertUInt.ToUInt16(_BufferMessage.GetRange(15, 2).ToArray(), 0);
                    ResultMessages.NumberFormatMessage = _BufferMessage[17];
                    ResultMessages.AttributeEvent = _BufferMessage[18];
                    ResultMessages.CodeEvent = ConvertUInt.ToUInt16(_BufferMessage.GetRange(19, 2).ToArray(), 0);
                    ResultMessages.NumberChapter = _BufferMessage[21];
                    ResultMessages.NumberZone = _BufferMessage[22];
                    ResultMessages.CurrentChannel = _BufferMessage[23].ToString();
                    ResultMessages.LevelGSM = _BufferMessage[24] * 100 / 32;
                    ResultMessages.LevelErrorGSM = _BufferMessage[25] * 100 / 9;
                    ResultMessages.TamperСontrol = new BitArray(new byte[] { _BufferMessage[26] })[0] ? "Контролируется " : "Не контролируется";
                    ResultMessages.TamperСondition = new BitArray(new byte[] { _BufferMessage[26] })[1] ? "Открыт " : "Закрыт";
                    ResultMessages.Control220v = new BitArray(new byte[] { _BufferMessage[26] })[2] ? "Контролируется " : "Не контролируется";
                    ResultMessages.Condition220v = new BitArray(new byte[] { _BufferMessage[26] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме";
                    ResultMessages.Control12v = new BitArray(new byte[] { _BufferMessage[26] })[4] ? "Контролируется" : "Не контролируется";
                    ResultMessages.Condition12v = new BitArray(new byte[] { _BufferMessage[26] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме";
                    ResultMessages.Voltage = $"{Convert.ToSingle((decimal)_BufferMessage[27] / 10)} v";
                    ResultMessages.UnknownParameter = _BufferMessage.GetRange(28, 2).ToArray();
                    break;
                // Проверка существования открытого соединения
                case 0x06:
                    ResultMessages.LevelGSM = _BufferMessage[0] * 32 / 100;
                    ResultMessages.LevelErrorGSM = _BufferMessage[1] * 9 / 100;
                    ResultMessages.Voltage = $"{Convert.ToSingle((decimal)_BufferMessage[2] / 10)} v";
                    ResultMessages.TamperСontrol = new BitArray(new byte[] { _BufferMessage[3] })[0] ? "Контролируется " : "Не контролируется";
                    ResultMessages.TamperСondition = new BitArray(new byte[] { _BufferMessage[3] })[1] ? "Открыт " : "Закрыт";
                    ResultMessages.Control220v = new BitArray(new byte[] { _BufferMessage[3] })[2] ? "Контролируется " : "Не контролируется";
                    ResultMessages.Condition220v = new BitArray(new byte[] { _BufferMessage[3] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме";
                    ResultMessages.Control12v = new BitArray(new byte[] { _BufferMessage[3] })[4] ? "Контролируется" : "Не контролируется";
                    ResultMessages.Condition12v = new BitArray(new byte[] { _BufferMessage[3] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме";
                    ResultMessages.Rezerve = _BufferMessage[4];
                    break;
                // Таблица интеграции
                case 0x0e:
                    for (int i = 0; i < _BufferMessage.Count; i++)
                    {
                        _Count++;
                        _BufferByteList.Add(_BufferMessage[i]);
                        if (_Count == 12)
                        {
                            _ResultByteList.Add(_BufferByteList.ToArray());
                            _BufferByteList.Clear();
                            _Count = 0;
                            continue;
                        }
                    }
                    foreach (var _BufferByte in _ResultByteList)
                    {
                        var Registr = ConvertUInt.ToUInt16([_BufferByte[10], _BufferByte[11]], 0);
                        _ListTableIntegration.Add(new TableIntegration()
                        {
                            AddressController = _BufferByte[0],
                            TypeDevice = $"{ListDeviceType[_BufferByte[1]]}",
                            ObjectNumber = ConvertUInt.ToUInt16([_BufferByte[2], _BufferByte[3]], 0),
                            DateTimeConfig = (new byte[] { _BufferByte[4], _BufferByte[5], _BufferByte[6], _BufferByte[7] }).All(x => x == byte.MaxValue) ? DateTime.Now.ToString("dd.MM.yyyy H:mm:ss") : DateTimeOffset.FromUnixTimeSeconds(ConvertUInt.ToUInt32([_BufferByte[4], _BufferByte[5], _BufferByte[6], _BufferByte[7]], 0)).ToString("dd.MM.yyyy H:mm:ss"),
                            VersionController = $"{_BufferByte[8]}.{_BufferByte[9]}",
                            ChangeRegistr = $"isActive: {(_BufferByte[0] & 128) == 0}, isOnline: {(Registr & 1) > 0}, isConfigChanged: {(Registr & 4) > 0}",
                        });
                    }
                    ResultMessages.ListTableIntegration = JsonConvert.SerializeObject(_ListTableIntegration);
                    break;
                // Ошибка запроса. ( ошибка CRC, неверная длина пакета )
                case 0xff:
                    ResultMessages = null;
                    break;
                case 0x32:
                    ResultMessages.ObjectNumber = ConvertUInt.ToUInt16(_BufferMessage.GetRange(0, 2).ToArray(), 0);
                    ResultMessages.NumberRele = _BufferMessage[2];
                    ResultMessages.RelayStatus = ListRelayStatus[_BufferMessage[3]];
                    break;
                default:
                    ResultMessages = null;
                    break;
            }
            _ListTableIntegration = null; _BufferMessage = null; _GenerateListChannel = null; _BufferByteList = null; _ResultByteList = null; _Channels = null; _Count = 0;
            return ResultMessages;
        }
        /// <summary>
        /// Формирование параметров входящего сообщения асинхронно
        /// </summary>
        /// <param name="_CroppedMessage">Сокращенное входящие сообщение</param>
        /// <param name="_TypeMessage">Флаг типа <see cref="byte"/> для определения типа запроса и формирования исходящего пакета, например: <see langword="0x0a"/>, <see langword="0x06"/>, <see langword="0x0e"/></param>
        /// <param name="_ProtocolNew">Флаг типа <see cref="bool"/> указывающий на актуальнойть версии протокола ( <see langword="true"/> - Актуальная версия, <see langword="false"/> - Старая версия )</param>
        /// <returns></returns>
        public async Task<ResultMessage> ReturnMessageAsync(byte[] _CroppedMessage, byte _TypeMessage, bool _ProtocolNew)
        {
            ResultMessages = new ResultMessage();
            List<TableIntegration> _ListTableIntegration = [];
            List<byte> _BufferMessage = new List<byte>(_CroppedMessage.ToArray()), _GenerateListChannel, _BufferByteList = [];
            List<byte[]> _ResultByteList = [];
            List<string> _Channels = [];
            int _Count = 0;
            switch (_TypeMessage)
            {
                // Инициализация
                case 0x0a:
                    ResultMessages.VersionProtocol = _BufferMessage[0];
                    ResultMessages.VersionController = $"{_BufferMessage[1]}.{_BufferMessage[2]}";
                    ResultMessages.IDController = $"{_BufferMessage[3]}.{_BufferMessage[4]}.{_BufferMessage[5]}.{_BufferMessage[6]}";
                    ResultMessages.Password = !_ProtocolNew ? ConvertUInt.ToUInt32(_BufferMessage.GetRange(7, 3).ToArray(), 0) : ConvertUInt.ToUInt32(_BufferMessage.GetRange(7, 4).ToArray(), 0);
                    ResultMessages.SizeBufferMessage = !_ProtocolNew ? ConvertUInt.ToUInt16(_BufferMessage.GetRange(10, 2).ToArray(), 0) : ConvertUInt.ToUInt16(_BufferMessage.GetRange(11, 2).ToArray(), 0);
                    ResultMessages.CountZone = !_ProtocolNew ? _BufferMessage[12] : _BufferMessage[13];
                    ResultMessages.GPRSPingTime = !_ProtocolNew ? ConvertUInt.ToUInt16(_BufferMessage.GetRange(13, 2).ToArray(), 0) : ConvertUInt.ToUInt16(_BufferMessage.GetRange(14, 2).ToArray(), 0);
                    ResultMessages.CSDPingTime = !_ProtocolNew ? ConvertUInt.ToUInt16(_BufferMessage.GetRange(15, 2).ToArray(), 0) : ConvertUInt.ToUInt16(_BufferMessage.GetRange(16, 2).ToArray(), 0);
                    ResultMessages.LevelGSM = !_ProtocolNew ? _BufferMessage[17] * 100 / 32 : _BufferMessage[18] * 100 / 32;
                    ResultMessages.LevelErrorGSM = !_ProtocolNew ? _BufferMessage[18] * 100 / 9 : _BufferMessage[19] * 100 / 9;
                    ResultMessages.Voltage = !_ProtocolNew ? $"{Convert.ToSingle((decimal)_BufferMessage[19] / 10)} v" : $"{Convert.ToSingle((decimal)_BufferMessage[20] / 10)} v";
                    ResultMessages.TamperСontrol = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[0] ? "Контролируется " : "Не контролируется") : (new BitArray(new byte[] { _BufferMessage[21] })[0] ? "Контролируется " : "Не контролируется");
                    ResultMessages.TamperСondition = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[1] ? "Открыт " : "Закрыт") : (new BitArray(new byte[] { _BufferMessage[21] })[1] ? "Открыт " : "Закрыт");
                    ResultMessages.Control220v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[2] ? "Контролируется " : "Не контролируется") : (new BitArray(new byte[] { _BufferMessage[21] })[2] ? "Контролируется " : "Не контролируется");
                    ResultMessages.Condition220v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме") : (new BitArray(new byte[] { _BufferMessage[21] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме");
                    ResultMessages.Control12v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[4] ? "Контролируется" : "Не контролируется") : (new BitArray(new byte[] { _BufferMessage[21] })[4] ? "Контролируется" : "Не контролируется");
                    ResultMessages.Condition12v = !_ProtocolNew ? (new BitArray(new byte[] { _BufferMessage[20] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме") : (new BitArray(new byte[] { _BufferMessage[21] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме");
                    ResultMessages.Rezerve = !_ProtocolNew ? _BufferMessage[21] : _BufferMessage[22];
                    _GenerateListChannel = !_ProtocolNew ? _BufferMessage.GetRange(23, 12) : _BufferMessage.GetRange(24, 12);
                    _GenerateListChannel.ToList().ForEach(x => _Channels.Add(ListChannels[x]));
                    ResultMessages.ListChannel = _Channels;
                    ResultMessages.CurrentChannel = ResultMessages.ListChannel != null ? !_ProtocolNew ? ResultMessages.ListChannel[_BufferMessage[22] - 1] : ResultMessages.ListChannel[_BufferMessage[23 - 1]] : null;
                    ResultMessages.IMEIGSM = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(36, 15).ToArray()) : null;
                    ResultMessages.SIM1 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(51, 16).ToArray()) : null;
                    ResultMessages.SIM2 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(67, 16).ToArray()) : null;
                    ResultMessages.UCCIDSIM1 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(83, 20).ToArray()) : null;
                    ResultMessages.UCCIDSIM2 = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(103, 20).ToArray()) : null;
                    ResultMessages.MACETH = _ProtocolNew ? BitConverter.ToString(_BufferMessage.GetRange(123, 6).ToArray()).Replace('-', ':') : null;
                    ResultMessages.MACWIFI = _ProtocolNew ? BitConverter.ToString(_BufferMessage.GetRange(129, 6).ToArray()).Replace('-', ':') : null;
                    ResultMessages.TypeDevice = _ProtocolNew ? $"{ListDeviceType[_BufferMessage[135]]}" : null;
                    ResultMessages.MCC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(136, 3).ToArray()) : null;
                    ResultMessages.MNC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(139, 2).ToArray()) : null;
                    ResultMessages.LAC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(141, 4).ToArray()) : null;
                    ResultMessages.LAC = _ProtocolNew ? Encoding.ASCII.GetString(_BufferMessage.GetRange(145, 4).ToArray()) : null;
                    ResultMessages.VersionHW = _ProtocolNew ? $"{Convert.ToInt16(_BufferMessage[149])}.{Convert.ToInt16(_BufferMessage[150])} ver" : null;
                    break;
                // Новое сообщение
                case 0x02:
                    ResultMessages.NumberLogJournal = ConvertUInt.ToUInt16(_BufferMessage.GetRange(0, 2).ToArray(), 0);
                    ResultMessages.EventDateTime = $"{DateTime.Parse($"{_BufferMessage[2]}-{_BufferMessage[3]}-{_BufferMessage[4]} {_BufferMessage[5]}:{_BufferMessage[6]}:{_BufferMessage[7]}", new CultureInfo("ru-RU")):dd.MM.yyyy HH:mm:ss}";
                    ResultMessages.ControllerDateTime = $"{DateTime.Parse($"{_BufferMessage[8]}-{_BufferMessage[9]}-{_BufferMessage[10]} {_BufferMessage[11]}:{_BufferMessage[12]}:{_BufferMessage[13]}", new CultureInfo("ru-RU")):dd.MM.yyyy HH:mm:ss}";
                    ResultMessages.AddressController = _BufferMessage[14];
                    ResultMessages.ObjectNumber = ConvertUInt.ToUInt16(_BufferMessage.GetRange(15, 2).ToArray(), 0);
                    ResultMessages.NumberFormatMessage = _BufferMessage[17];
                    ResultMessages.AttributeEvent = _BufferMessage[18];
                    ResultMessages.CodeEvent = ConvertUInt.ToUInt16(_BufferMessage.GetRange(19, 2).ToArray(), 0);
                    ResultMessages.NumberChapter = _BufferMessage[21];
                    ResultMessages.NumberZone = _BufferMessage[22];
                    ResultMessages.CurrentChannel = _BufferMessage[23].ToString();
                    ResultMessages.LevelGSM = _BufferMessage[24] * 100 / 32;
                    ResultMessages.LevelErrorGSM = _BufferMessage[25] * 100 / 9;
                    ResultMessages.TamperСontrol = new BitArray(new byte[] { _BufferMessage[26] })[0] ? "Контролируется " : "Не контролируется";
                    ResultMessages.TamperСondition = new BitArray(new byte[] { _BufferMessage[26] })[1] ? "Открыт " : "Закрыт";
                    ResultMessages.Control220v = new BitArray(new byte[] { _BufferMessage[26] })[2] ? "Контролируется " : "Не контролируется";
                    ResultMessages.Condition220v = new BitArray(new byte[] { _BufferMessage[26] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме";
                    ResultMessages.Control12v = new BitArray(new byte[] { _BufferMessage[26] })[4] ? "Контролируется" : "Не контролируется";
                    ResultMessages.Condition12v = new BitArray(new byte[] { _BufferMessage[26] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме";
                    ResultMessages.Voltage = $"{Convert.ToSingle((decimal)_BufferMessage[27] / 10)} v";
                    ResultMessages.UnknownParameter = _BufferMessage.GetRange(28, 2).ToArray();
                    break;
                // Проверка существования открытого соединения
                case 0x06:
                    ResultMessages.LevelGSM = _BufferMessage[0] * 32 / 100;
                    ResultMessages.LevelErrorGSM = _BufferMessage[1] * 9 / 100;
                    ResultMessages.Voltage = $"{Convert.ToSingle((decimal)_BufferMessage[2] / 10)} v";
                    ResultMessages.TamperСontrol = new BitArray(new byte[] { _BufferMessage[3] })[0] ? "Контролируется " : "Не контролируется";
                    ResultMessages.TamperСondition = new BitArray(new byte[] { _BufferMessage[3] })[1] ? "Открыт " : "Закрыт";
                    ResultMessages.Control220v = new BitArray(new byte[] { _BufferMessage[3] })[2] ? "Контролируется " : "Не контролируется";
                    ResultMessages.Condition220v = new BitArray(new byte[] { _BufferMessage[3] })[3] ? "Нет сети 220 Вольт" : "Сеть 220 Вольт в норме";
                    ResultMessages.Control12v = new BitArray(new byte[] { _BufferMessage[3] })[4] ? "Контролируется" : "Не контролируется";
                    ResultMessages.Condition12v = new BitArray(new byte[] { _BufferMessage[3] })[5] ? "Напряжение питания ниже допустимого значения" : "Напряжение питания в норме";
                    ResultMessages.Rezerve = _BufferMessage[4];
                    break;
                // Таблица интеграции
                case 0x0e:
                    for (int i = 0; i < _BufferMessage.Count; i++)
                    {
                        _Count++;
                        _BufferByteList.Add(_BufferMessage[i]);
                        if (_Count == 12)
                        {
                            _ResultByteList.Add(_BufferByteList.ToArray());
                            _BufferByteList.Clear();
                            _Count = 0;
                            continue;
                        }
                    }
                    foreach (var _BufferByte in _ResultByteList)
                    {
                        var Registr = ConvertUInt.ToUInt16([_BufferByte[10], _BufferByte[11]], 0);
                        _ListTableIntegration.Add(new TableIntegration()
                        {
                            AddressController = _BufferByte[0],
                            TypeDevice = $"{ListDeviceType[_BufferByte[1]]}",
                            ObjectNumber = ConvertUInt.ToUInt16([_BufferByte[2], _BufferByte[3]], 0),
                            DateTimeConfig = (new byte[] { _BufferByte[4], _BufferByte[5], _BufferByte[6], _BufferByte[7] }).All(x => x == byte.MaxValue) ? DateTime.Now.ToString("dd.MM.yyyy H:mm:ss") : DateTimeOffset.FromUnixTimeSeconds(ConvertUInt.ToUInt32([_BufferByte[4], _BufferByte[5], _BufferByte[6], _BufferByte[7]], 0)).ToString("dd.MM.yyyy H:mm:ss"),
                            VersionController = $"{_BufferByte[8]}.{_BufferByte[9]}",
                            ChangeRegistr = $"isActive: {(_BufferByte[0] & 128) == 0}, isOnline: {(Registr & 1) > 0}, isConfigChanged: {(Registr & 4) > 0}",
                        });
                    }
                    ResultMessages.ListTableIntegration = JsonConvert.SerializeObject(_ListTableIntegration);
                    break;
                // Ошибка запроса. ( ошибка CRC, неверная длина пакета )
                case 0xff:
                    ResultMessages = null;
                    break;
                // Отправить команду
                case 0x32:
                    ResultMessages.ObjectNumber = ConvertUInt.ToUInt16(_BufferMessage.GetRange(0, 2).ToArray(), 0);
                    ResultMessages.NumberRele = _BufferMessage[2];
                    ResultMessages.RelayStatus = ListRelayStatus[_BufferMessage[3]];
                    break;
                default:
                    ResultMessages = null;
                    break;
            }
            _ListTableIntegration = null; _BufferMessage = null; _GenerateListChannel = null; _BufferByteList = null; _ResultByteList = null; _Channels = null; _Count = 0;
            return await Task.FromResult(ResultMessages);
        }
        /// <summary>
        /// Формирование ответного пакета
        /// </summary>
        /// <param name="_CroppedMessage">Сокращенное входящие сообщение</param>
        /// <param name="_MessageCount">Цело число, определение под каким номером пуступило входящее сообщение</param>
        /// <param name="_TypeMessage">Флаг типа <see cref="byte"/> для определения типа запроса и формирования исходящего пакета, например: <see langword="0x0a"/>, <see langword="0x06"/>, <see langword="0x0e"/></param>
        /// <param name="_ProtocolNew">Флаг типа <see cref="bool"/> указывающий на актуальнойть версии протокола ( <see langword="true"/> - Актуальная версия, <see langword="false"/> - Старая версия )</param>
        /// <returns></returns>
        public byte[] ReturnPacketMessage(byte[] _CroppedMessage, short _MessageCount, byte _TypeMessage, bool _ProtocolNew)
        {
            List<byte> _BufferMessage, _DataMessage, _BodyBufferMessage, _ResultArray;
            byte[] _CheckSumm, _InitSizeArray, _ResultSizeArray, _InitMessageCount;
            switch (_TypeMessage)
            {
                // Инициализация 0x0c
                case 0x0a:
                    _BufferMessage = new List<byte>(_CroppedMessage.ToArray());
                    _DataMessage =
                    [
                        Convert.ToByte(DateTime.Now.Day),
                        Convert.ToByte(DateTime.Now.Month),
                        Convert.ToByte(DateTime.Now.Year - 2000),
                        Convert.ToByte(DateTime.Now.TimeOfDay.Hours),
                        Convert.ToByte(DateTime.Now.TimeOfDay.Minutes),
                        Convert.ToByte(DateTime.Now.TimeOfDay.Seconds),
                    ];
                    if (_ProtocolNew) _DataMessage.AddRange(!_ProtocolNew ? _BufferMessage.GetRange(7, 3).ToArray() : _BufferMessage.GetRange(7, 4).ToArray());
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x0c,
                        .. _DataMessage, //Присвоение пользовательского тела сообщения
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Новое сообщение 0x04
                case 0x02:
                    _BufferMessage = new List<byte>(_CroppedMessage.ToArray());
                    _DataMessage =
                    [
                        _BufferMessage[0],
                        _BufferMessage[1],
                    ];
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x04,
                        .. _DataMessage, //Присвоение пользовательского тела сообщения
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Проверка существования открытого соединения 0x08
                case 0x06:
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x08,
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Таблица интеграции 0x10
                case 0x0e:
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x10,
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Ошибка запроса. ( ошибка CRC, неверная длина пакета ) 0xff
                case 0xff:
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0xff,
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                default:
                    PacketMessage = null;
                    break;
            }
            return PacketMessage;
        }
        /// <summary>
        /// Формирование ответного пакета асинхронно
        /// </summary>
        /// <param name="_CroppedMessage">Сокращенное входящие сообщение</param>
        /// <param name="_MessageCount">Цело число, определение под каким номером пуступило входящее сообщение</param>
        /// <param name="_TypeMessage">Флаг типа <see cref="byte"/> для определения типа запроса и формирования исходящего пакета, например: <see langword="0x0a"/>, <see langword="0x06"/>, <see langword="0x0e"/></param>
        /// <param name="_ProtocolNew">Флаг типа <see cref="bool"/> указывающий на актуальнойть версии протокола ( <see langword="true"/> - Актуальная версия, <see langword="false"/> - Старая версия )</param>
        public async Task<byte[]> ReturnPacketMessageAsync(byte[] _CroppedMessage, short _MessageCount, byte _TypeMessage, bool _ProtocolNew)
        {
            List<byte> _BufferMessage, _DataMessage, _BodyBufferMessage, _ResultArray;
            byte[] _CheckSumm, _InitSizeArray, _ResultSizeArray, _InitMessageCount;
            switch (_TypeMessage)
            {
                // Инициализация 0x0c
                case 0x0a:
                    _BufferMessage = new List<byte>(_CroppedMessage.ToArray());
                    _DataMessage =
                    [
                        Convert.ToByte(DateTime.Now.Day),
                        Convert.ToByte(DateTime.Now.Month),
                        Convert.ToByte(DateTime.Now.Year - 2000),
                        Convert.ToByte(DateTime.Now.TimeOfDay.Hours),
                        Convert.ToByte(DateTime.Now.TimeOfDay.Minutes),
                        Convert.ToByte(DateTime.Now.TimeOfDay.Seconds),
                    ];
                    if (_ProtocolNew) _DataMessage.AddRange(!_ProtocolNew ? _BufferMessage.GetRange(7, 3).ToArray() : _BufferMessage.GetRange(7, 4).ToArray());
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x0c,
                        .. _DataMessage, //Присвоение пользовательского тела сообщения
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Новое сообщение 0x04
                case 0x02:
                    _BufferMessage = new List<byte>(_CroppedMessage.ToArray());
                    _DataMessage =
                    [
                        _BufferMessage[0],
                        _BufferMessage[1],
                    ];
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x04,
                        .. _DataMessage, //Присвоение пользовательского тела сообщения
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Проверка существования открытого соединения 0x08
                case 0x06:
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x08,
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Таблица интеграции 0x10
                case 0x0e:
                    _InitMessageCount = BitConverter.GetBytes(_MessageCount);//Конвертация номера сообщения в массив байт
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        _InitMessageCount[1],
                        _InitMessageCount[0],
                        0x10,
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                // Ошибка запроса. ( ошибка CRC, неверная длина пакета ) 0xff
                case 0xff:
                    _BodyBufferMessage =
                    [
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0xff,
                    ];//Генерация тела сообщения
                    _CheckSumm = BitConverter.GetBytes(GetCheckSumm(_BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
                    _BodyBufferMessage[0] = _CheckSumm[1];//
                    _BodyBufferMessage[1] = _CheckSumm[0];//Присвоение контрольной суммы
                    _InitSizeArray = BitConverter.GetBytes(_BodyBufferMessage.Count);//Вычисления размера сообщения
                    _ResultSizeArray = _ProtocolNew ? [_InitSizeArray[1], _InitSizeArray[0]] : [_InitSizeArray[0]];//Определение размера сообщения для протокола
                    _ResultArray =
                    [
                        .. _ResultSizeArray,//Присвоение размера сообщения
                        .. _BodyBufferMessage,//Присвоение сгенерированного тела сообщения
                    ];//Генерация сообщения
                    PacketMessage = _ResultArray.ToArray();//Результат сгенерированного сообщения
                    break;
                default:
                    PacketMessage = null;
                    break;
            }
            return await Task.FromResult(PacketMessage);
        }
        /// <summary>
        /// Управление реле асинхронно
        /// </summary>
        /// <param name="Protocol"></param>
        /// <returns></returns>
        public async Task<byte[]> ReturnRelayMessageAsync(bool Protocol)
        {
            List<byte> DataMessage, BodyBufferMessage, ResultArray;
            byte[] CheckSumm, InitSizeArray, ResultSizeArray;
            //ConverObjectNumber = BitConverter.GetBytes(ObjectNumber);//Конвертация номера объекта
            DataMessage =
            [
                0x00, //ConverObjectNumber[1],
                0x00, //ConverObjectNumber[0],
                0x00,
                0x00,
                0x00,
                0x00,
            ];
            BodyBufferMessage =
            [
                0x00,
                0x00,
                0x00,
                0x00,
                0x30,
                .. DataMessage, //Присвоение пользовательского тела сообщения
            ];//Генерация тела сообщения
            CheckSumm = BitConverter.GetBytes(GetCheckSumm(BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
            BodyBufferMessage[0] = CheckSumm[1];//
            BodyBufferMessage[1] = CheckSumm[0];//Присвоение контрольной суммы
            InitSizeArray = BitConverter.GetBytes(BodyBufferMessage.Count);//Вычисления размера сообщения
            ResultSizeArray = Protocol ? [InitSizeArray[1], InitSizeArray[0]] : [InitSizeArray[0]];//Определение размера сообщения для протокола
            ResultArray =
            [
                .. ResultSizeArray,//Присвоение размера сообщения
                .. BodyBufferMessage,//Присвоение сгенерированного тела сообщения
            ];//Генерация сообщения
            return await Task.FromResult(ResultArray.ToArray());
        }
        /// <summary>
        /// Управление реле
        /// </summary>
        /// <param name="Protocol"></param>
        /// <returns></returns>
        public byte[] ReturnRelayMessage(bool Protocol)
        {
            List<byte> DataMessage, BodyBufferMessage, ResultArray;
            byte[] CheckSumm, InitSizeArray, ResultSizeArray;
            //ConverObjectNumber = BitConverter.GetBytes(ObjectNumber);//Конвертация номера объекта
            DataMessage =
            [
                0x00, //ConverObjectNumber[1],
                0x00, //ConverObjectNumber[0],
                0x00,
                0x00,
                0x00,
                0x00,
            ];
            BodyBufferMessage =
            [
                0x00,
                0x00,
                0x00,
                0x00,
                0x30,
                .. DataMessage, //Присвоение пользовательского тела сообщения
            ];//Генерация тела сообщения
            CheckSumm = BitConverter.GetBytes(GetCheckSumm(BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
            BodyBufferMessage[0] = CheckSumm[1];//
            BodyBufferMessage[1] = CheckSumm[0];//Присвоение контрольной суммы
            InitSizeArray = BitConverter.GetBytes(BodyBufferMessage.Count);//Вычисления размера сообщения
            ResultSizeArray = Protocol ? [InitSizeArray[1], InitSizeArray[0]] : [InitSizeArray[0]];//Определение размера сообщения для протокола
            ResultArray =
            [
                .. ResultSizeArray,//Присвоение размера сообщения
                .. BodyBufferMessage,//Присвоение сгенерированного тела сообщения
            ];//Генерация сообщения
            return ResultArray.ToArray();
        }
        /// <summary>
        /// Пакет сигнализирующий ошибку асинхронно
        /// </summary>
        /// <param name="Protocol"></param>
        /// <returns></returns>
        public Task<byte[]> ReturnErrorMessageAsync(bool Protocol)
        {
            List<byte> BodyBufferMessage, ResultArray;
            byte[] CheckSumm, InitSizeArray, ResultSizeArray;
            BodyBufferMessage =
            [
                0x00,
                0x00,
                0x00,
                0x00,
                0xff,
            ];//Генерация тела сообщения
            CheckSumm = BitConverter.GetBytes(GetCheckSumm(BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
            BodyBufferMessage[0] = CheckSumm[1];//
            BodyBufferMessage[1] = CheckSumm[0];//Присвоение контрольной суммы
            InitSizeArray = BitConverter.GetBytes(BodyBufferMessage.Count);//Вычисления размера сообщения
            ResultSizeArray = Protocol ? [InitSizeArray[1], InitSizeArray[0]] : [InitSizeArray[0]];//Определение размера сообщения для протокола
            ResultArray =
            [
                .. ResultSizeArray,//Присвоение размера сообщения
                .. BodyBufferMessage,//Присвоение сгенерированного тела сообщения
            ];//Генерация сообщения
            return Task.FromResult(ResultArray.ToArray());
        }
        /// <summary>
        /// Пакет сигнализирующий ошибку
        /// </summary>
        /// <param name="Protocol"></param>
        /// <returns></returns>
        public byte[] ReturnErrorMessage(bool Protocol)
        {
            List<byte> BodyBufferMessage, ResultArray;
            byte[] CheckSumm, InitSizeArray, ResultSizeArray;
            BodyBufferMessage =
            [
                0x00,
                0x00,
                0x00,
                0x00,
                0xff,
            ];//Генерация тела сообщения
            CheckSumm = BitConverter.GetBytes(GetCheckSumm(BodyBufferMessage.ToArray()));//Вычисление контрольной суммы
            BodyBufferMessage[0] = CheckSumm[1];//
            BodyBufferMessage[1] = CheckSumm[0];//Присвоение контрольной суммы
            InitSizeArray = BitConverter.GetBytes(BodyBufferMessage.Count);//Вычисления размера сообщения
            ResultSizeArray = Protocol ? [InitSizeArray[1], InitSizeArray[0]] : [InitSizeArray[0]];//Определение размера сообщения для протокола
            ResultArray =
            [
                .. ResultSizeArray,//Присвоение размера сообщения
                .. BodyBufferMessage,//Присвоение сгенерированного тела сообщения
            ];//Генерация сообщения
            return ResultArray.ToArray();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                DecodeMessages = null;
                PacketMessage = null;
                ResultMessages = null;
                Disposed = true;
            }
        }
        /// <inheritdoc/>
        ~Guard() => Dispose(false);
    }
}
