#region License
/*
Copyright 2022-2025 Dmitrii Evdokimov
Open source software

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System.Text;
using System.Xml;

namespace Api6775;

/// <summary>
/// Кредитные истории.
/// Расширения для обработки API v2.0
/// </summary>
public static class ApiExtra
{
    private record Agreement(string Id, string Date, string Val, string Sum);

    /// <summary>
    /// Запрос (dlrequest) и получение (dlanswer) за один запуск,
    /// создание текстового сводного отчета по полученным сведениям
    /// XML (report).
    /// </summary>
    public const string auto = nameof(auto);

    /// <summary>
    /// Пакетная обработка запросов (auto) из папки.
    /// Это действие по умолчанию, если параметров не указано,
    /// но есть папка DirSource в конфиге.
    /// </summary>
    public const string dir = nameof(dir);

    /// <summary>
    /// Создание текстового сводного отчета по полученным
    /// сведениям XML.
    /// </summary>
    public const string report = nameof(report);

    /// <summary>
    /// Автоматическое прохождение всего запроса (auto).
    /// </summary>
    /// <param name="source">Исходный файл с запросом.</param>
    /// <param name="request">Путь сохранения отправленного запроса.</param>
    /// <param name="result">Путь сохранения полученной квитанции.</param> ЛИШНЕЕ?
    /// <param name="answer">Путь сохранения полученных сведений.</param>
    /// <param name="report">Путь сохранения сводного отчета.</param>
    /// <returns>Код возврата(0 - успех, 1 - файл не создан).</returns>
    public static async Task<int> AutoRequestAsync(string source,
        string request, string result, string answer, string report)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(request)!);
        Directory.CreateDirectory(Path.GetDirectoryName(result)!);
        Directory.CreateDirectory(Path.GetDirectoryName(answer)!);
        Directory.CreateDirectory(Path.GetDirectoryName(report)!);

        /* Абонент формирует и передает запрос на сервер КБКИ. */

        File.Copy(source, request, true);
        (int ret, string xml) = await Api.PostRequestAsync(Api.dlrequest, request, result);

        /* В случае отсутствия ошибок для запросов, требующих подготовки
         * ответа, КБКИ формирует и возвращает ответ в течение 4 секунд.
         * В остальных случаях, а также если подготовка ответа превышает
         * 4 секунды, КБКИ формирует и возвращает квитанцию.
         *
         * КБКИ формирует и возвращает квитанцию, которая в зависимости
         * от результатов проверки запроса может содержать информацию об
         * успешной загрузке запроса, об ошибке либо идентификатор ответа,
         * по которому ответ может быть получен после его готовности.
         * 
         * В случае запросов, требующих подготовки ответа, КБКИ готовит
         * ответ.
         * 
         * Для получения ответа (в том числе для повторного получения
         * ответа) абонент устанавливает новое соединение и GET запросом
         * передает полученный в квитанции (или в ранее полученном ответе)
         * идентификатор ответа.
         * 
         * В случае если ответ с указанным номером еще не готов, КБКИ
         * удерживает соединение до его готовности либо возвращает
         * квитанцию с кодом ошибки «Ответ не готов».
         * При возникновении ошибок возвращается квитанция с кодом и
         * описанием ошибки. Если во время ожидания ответа произойдет
         * разрыв соединения без возврата каких-либо сообщений,
         * процедура установки соединения повторяется.
         * 
         * Для снижения нагрузки в квитанцию, содержащую идентификатор
         * ответа, сервер может поместить атрибут «ВремяГотовности»,
         * указав в качестве значения время (в миллисекундах),
         * требующееся серверу на подготовку ответа.
         * При наличии атрибута «ВремяГотовности» клиент должен обращаться
         * за получением сведений о среднемесячных платежах Субъекта не
         * ранее, чем по истечении времени, указанного в атрибуте.
         * 
         * КБКИ формирует ответ с использованием символов кодировки UTF-8
         * в формате XML в соответствии с xsd-схемой ответа, подписывает
         * УЭП, присоединяет к ответу и возвращает абоненту в формате
         * PKCS#7.
         * 
         * Ответ по идентификатору должен быть доступен для получения
         * абонентом в течение не менее 8 часов после его формирования.
         */

        // 200 – результат запроса содержит информацию о результатах
        // загрузки данных в базу данных КБКИ
        // 202 – результат запроса содержит квитанцию с идентификатором
        // ответа
        // 400 - результат запроса содержит квитанцию с информацией об
        // ошибке

        //if (ret == 200)
        //{

        /*
        Пример ответа на запрос сведений о среднемесячных платежах Субъекта
        и (или) сведений о запрете (снятии запрета)
        (qcb_answer.xml)

        <?xml version="1.0" encoding="UTF-8"?>
        <ОтветНаЗапросСведений Версия="2.0"
        ИдентификаторЗапроса="ef15a678-637f-11ea-83b8-21758a33c94a"
        ДатаЗапроса="2024-07-14"
        ИдентификаторОтвета="6afbdd01-6380-11ea-83b8-21758a33c94a"
        ОГРН="1077333000003" ТипОтвета="1" РежимЗапроса="1">
            <Сведения ПорядковыйНомер="1">
                <ТитульнаяЧасть>
                    <ФИО>
                        <Фамилия>Иванова</Фамилия>
                        <Имя>Мария</Имя>
                        <Отчество>Ивановна</Отчество>
                    </ФИО>
                    <ФИО>
                        <Фамилия>Петрова</Фамилия>
                        <Имя>Мария</Имя>
                        <Отчество>Ивановна</Отчество>
                    </ФИО>
                    <ДатаРождения>1982-07-14</ДатаРождения>
                    <ДокументЛичности КодДУЛ="21">
                        <Серия>4003</Серия>
                        <Номер>123456</Номер>
                        <ДатаВыдачи>2003-07-18</ДатаВыдачи>
                        <Гражданство>643</Гражданство>
                    </ДокументЛичности>
                </ТитульнаяЧасть>
                <КБКИ ОГРН="1077333000003"
                ПоСостояниюНа="2024-03-11T13:37:47Z">
                    <Обязательства>
                        <БКИ ОГРН="1077123000003">
                            <Договор
                            УИД="74131ecf-6382-11ea-83b8-21758a33c94a-b"
                            Представлено="2024-03-01">
                                <СреднемесячныйПлатеж ДатаРасчета="2024-03-10"
                                Валюта="RUB">8125</СреднемесячныйПлатеж>
                            </Договор>
                        </БКИ>
                        <БКИ ОГРН="1078111111113">
                            <Договор
                            УИД="74131ecf-6382-11ea-83b8-21758a33c94a-b"
                            Представлено="2024-03-02">
                                <СреднемесячныйПлатеж ДатаРасчета="2024-03-10"
                                Валюта="RUB">8125</СреднемесячныйПлатеж>
                            </Договор>
                        </БКИ>
                    </Обязательства>
                    <УсловияЗапрета>
                        <Условие ДатаЗаявления="2024-02-01"
                        ВремяЗаявления="10:12:45"
                        НачалоДействия="2024-02-02">3</Условие>
                        <Условие ДатаЗаявления="2024-02-01"
                        ВремяЗаявления="10:12:45"
                        НачалоДействия="2024-02-02">4</Условие>
                    </УсловияЗапрета>
                </КБКИ>
            </Сведения>
        </ОтветНаЗапросСведений>
        */

        /*
        Пример ответа на запрос сведений о среднемесячных платежах Субъекта
        в случае отсутствия данных по запрашиваемому Субъекту
        (qcb_answer_nodata.xml)

        <?xml version="1.0" encoding="UTF-8"?>
        <ОтветНаЗапросСведений Версия="2.0"
        ИдентификаторЗапроса="ef15a678-637f-11ea-83b8-21758a33c94a"
        ДатаЗапроса="2024-07-14"
        ИдентификаторОтвета="6afbdd01-6380-11ea-83b8-21758a33c94a"
        ОГРН="1077333000003" ТипОтвета="1" РежимЗапроса="1">
	        <Сведения ПорядковыйНомер="1">
		        <ТитульнаяЧасть>
			        <ФИО>
				        <Фамилия>Иванова</Фамилия>
				        <Имя>Мария</Имя>
				        <Отчество>Ивановна</Отчество>
			        </ФИО>
			        <ФИО>
				        <Фамилия>Петрова</Фамилия>
				        <Имя>Мария</Имя>
				        <Отчество>Ивановна</Отчество>
			        </ФИО>
			        <ДатаРождения>1982-07-14</ДатаРождения>
			        <ДокументЛичности КодДУЛ="21">
				        <Серия>4003</Серия>
				        <Номер>123456</Номер>
				        <ДатаВыдачи>2003-07-18</ДатаВыдачи>
				        <Гражданство>643</Гражданство>
			        </ДокументЛичности>
		        </ТитульнаяЧасть>
		        <КБКИ ОГРН="1077333000003"
                ПоСостояниюНа="2024-03-11T13:37:47Z">
			        <СубъектНеНайден/>
		        </КБКИ>
	        </Сведения>
        </ОтветНаЗапросСведений>
        */

        //    // ok
        //    goto ready;
        //}

        if (ret == 202)
        {
            /*
            Пример квитанции, содержащей идентификатор ответа
            (qcb_result_aswer.xml)

            <?xml version="1.0" encoding="UTF-8"?>
            <Результат Версия="2.0" ОГРН="1077333000003">
	            <ИдентификаторОтвета
                ИдентификаторЗапроса="ef15a678-637f-11ea-83b8-21758a33c94a"
                ДатаЗапроса="2024-07-14"
                [ВремяГотовности="1000"]>
                6afbdd01-6380-11ea-83b8-21758a33c94a</ИдентификаторОтвета>
            </Результат>
            */

            //TODO Thread.Sleep(ВремяГотовности);
            (string id, int sleep) = ApiHelper.ParseIdSleepFromXml(xml);

            while (true)
            {
                // В случае получения ошибки «Ответ не готов» клиент должен
                // повторить запрос не ранее, чем через 1 секунду.
                Thread.Sleep(sleep > 1000 ? sleep : 1000);

                // В качестве значения параметра id передается значение
                // идентификатора ответа, содержащегося в квитанции,
                // полученной при направлении запроса.
                ret = await Api.GetAnswerAsync(Api.dlanswer, id, answer);

                // 200 – результат запроса содержит сведения о среднемесячных
                // платежах Субъекта
                // 202 – результат запроса содержит квитанцию с информацией
                // об ошибке «Ответ не готов»
                // 400 - результат запроса содержит квитанцию с информацией
                // об ошибке, кроме ошибки «Ответ не готов».

                if (ret != 200)
                    break;

                // doc.LoadXml(xml); //TODO 
                // Код 12
                // Ответ не готов Подготовка ответа по указанному
                // идентификатору не закончена.
                // Необходимо повторить запрос позднее
            }
        }

        if (ret == 400)
        {
            /*
            Пример отрицательной квитанции на запрос сведений о среднемесячных
            платежах Субъекта
            (qcb_result_reqerror.xml)

            <?xml version="1.0" encoding="UTF-8"?>
            <Результат Версия="2.0" ОГРН="1077333000003">
	            <Ошибка Код="9">Запрос не соответствует схеме:
            Attribute 'КодДУЛ' is required in element 'ДокументЛичности'
            Error location: ЗапросСведений / Запрос / Субъект /
            ДокументЛичности</Ошибка>
            </Результат>
            */

            // error
            (string errKod, string errMsg) = ApiHelper.ParseErrorFromXml(xml);
            throw new InvalidOperationException($"Ошибка {errKod} - {errMsg}");
        }

        ret = await MakeReportAsync(answer, report);

        return ret;
    }

    /// <summary>
    /// Пакетная обработка папок с запросами (Extra dir).
    /// </summary>
    /// <param name="source">Папка+маска с исходными запросами.</param>
    /// <param name="request">Папка+файл с отправленными запросами.</param>
    /// <param name="result">Папка+файл с полученными квитанциями.</param>
    /// <param name="answer">Папка+файл с полученными сведениями.</param>
    /// <param name="report">Имя файла для сводного отчета.</param>
    /// <returns>Код возврата(0 - успех, 1 - файл не создан).</returns>
    public static async Task<int> PostRequestFolderAsync(string source,
        string request, string result, string answer, string report)
    {
        //TODO collect every file to an 'yyyy-mm-dd' folder

        int ret = 1;

        if (string.IsNullOrEmpty(request) ||
            string.IsNullOrEmpty(result) ||
            string.IsNullOrEmpty(answer) ||
            string.IsNullOrEmpty(report))
        {
            Console.WriteLine("Не указаны параметры папок.");

            return 1;
        }

        (string dirSource, string maskSource) = ApiHelper.GetDirMask(source);

        string fmt = request + result + answer;
        bool nameRequired = fmt.Contains("{name}");
        bool dateRequired = fmt.Contains("{date}");
        bool guidRequired = fmt.Contains("{guid}");

        int count = 0;

        foreach (var file in Directory.GetFiles(dirSource, maskSource))
        {
            if (nameRequired)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                request = request.Replace("{name}", name);
                result = result.Replace("{name}", name);
                answer = answer.Replace("{name}", name);
                report = report.Replace("{name}", name);
            }

            if (dateRequired)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");

                request = request.Replace("{date}", date);
                result = result.Replace("{date}", date);
                answer = answer.Replace("{date}", date);
                report = report.Replace("{date}", date);
            }

            if (guidRequired)
            {
                byte[] data = File.ReadAllBytes(file);

                if (data[0] == 0x30)
                {
                    //data = PKCS7.CleanSign(data);
                    data = await ASN1.CleanSignAsync(data);
                }

                XmlDocument doc = new();
                doc.LoadXml(Encoding.UTF8.GetString(data));
                string guid = doc.DocumentElement!.GetAttribute("ИдентификаторЗапроса");

                request = request.Replace("{guid}", guid);
                result = result.Replace("{guid}", guid);
                answer = answer.Replace("{guid}", guid);
                report = report.Replace("{guid}", guid);
            }

            await AutoRequestAsync(file, request, result, answer, report);

            //File.Copy(file, request, true);

            //ret = await Api.PostRequestAsync(Api.dir, request, result, answer);

            //if (ret == 495)
            //{
            //    return ret;
            //}

            //if (File.Exists(answer))
            //{
            //    File.Delete(file);
            //    count++;

            //    string report = Path.ChangeExtension(answer, "txt");
            //    await MakeReportAsync(answer, report);
            //}

            Thread.Sleep(1000);
        }

        Console.WriteLine($"Файлов сведений получено: {count}.");

        return ret;
    }

    /// <summary>
    /// Получение сводного текстового отчета по сведениям в XML.
    /// </summary>
    /// <param name="answer">Полученный файл с информацией.</param>
    /// <param name="report">Имя файла для сводного отчета.</param>
    /// <returns>Код возврата (0 - успех, 1 - файл не создан).</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<int> MakeReportAsync(string answer, string report)
    {
        if (!File.Exists(answer))
        {
            throw new FileNotFoundException("File not found.", answer);
        }

        XmlDocument doc = new();
        doc.Load(answer);
        StringBuilder sb = new();

        // <ОтветНаЗапросСведений Версия="2.0"
        // ИдентификаторЗапроса="ef15a678-637f-11ea-83b8-21758a33c94a"
        // ДатаЗапроса="2024-07-14"
        // ИдентификаторОтвета="6afbdd01-6380-11ea-83b8-21758a33c94a"
        // ОГРН="1077333000003" ТипОтвета="1" РежимЗапроса="1">
        var root = doc.DocumentElement
            ?? throw new Exception("XML не содержит корневого элемента.");

        // <Сведения ПорядковыйНомер="1"> (до 10)
        foreach (XmlNode person in root.ChildNodes)
        {
            Dictionary<string, Agreement> list = [];
            long total = 0;

            foreach (XmlNode div in person.ChildNodes)
            {
                switch (div.LocalName)
                {
                    case "ТитульнаяЧасть":
                        // <ФИО>
                        var fio = div.FirstChild
                            ?? throw new Exception("XML не содержит ФИО в Титульной части.");

                        // <Фамилия>Иванова</Фамилия>
                        // <Имя>Мария</Имя>
                        // <Отчество>Ивановна</Отчество>
                        string fio3 =
                            $"{fio.ChildNodes[0]?.InnerText} {fio.ChildNodes[1]?.InnerText} {fio.ChildNodes[2]?.InnerText}"
                            .Trim();
                        sb.AppendLine(fio3).AppendLine();
                        break;

                    case "КБКИ":
                        // <КБКИ ОГРН="1077333000003" ПоСостояниюНа="2024-03-11T13:37:47Z">
                        foreach (XmlNode data in div.ChildNodes)
                        {
                            switch (data.LocalName)
                            {
                                case "Ошибка":
                                    sb.AppendLine("Ошибка в ответе КБКИ."); //TODO error message
                                    break;

                                case "СубъектНеНайден":
                                    sb.AppendLine("Субъект в соответствующем КБКИ не найден.");
                                    break;

                                case "ОбязательствНет":
                                    sb.AppendLine("В КБКИ отсутствует информация о действующих обязательствах.");
                                    break;

                                case "Обязательства":
                                    // <БКИ ОГРН="1077123000003">
                                    foreach (XmlNode bki in data.ChildNodes)
                                    {
                                        // <Договор УИД="74131ecf-6382-11ea-83b8-21758a33c94a-b"
                                        // Представлено="2024-03-01">
                                        foreach (XmlNode agreement in bki.ChildNodes)
                                        {
                                            // УИД="74131ecf-6382-11ea-83b8-21758a33c94a-b"
                                            string uid = agreement.Attributes!["УИД"]!.Value;

                                            // <СреднемесячныйПлатеж
                                            // ДатаРасчета="2024-03-10"
                                            // Валюта="RUB">8125</СреднемесячныйПлатеж>
                                            var details = agreement.FirstChild;

                                            if (details is null)
                                                continue;

                                            string date = details.Attributes!["ДатаРасчета"]!.Value;
                                            string val = details.Attributes["Валюта"]!.Value;
                                            string sum = details.InnerText;

                                            Agreement add = new(uid, date, val, sum);

                                            if (list.TryGetValue(uid, out var found))
                                            {
                                                if (found is null ||
                                                    string.Compare(date, found.Date, false) > 0)
                                                {
                                                    list.Remove(uid);
                                                    list.Add(uid, add);
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                list.Add(uid, add);
                                                continue;
                                            }
                                        }
                                    }
                                    break;

                                case "СведенияОЗапретеНеПредоставляются":
                                    sb.AppendLine("Сведения о запрете не предоставляются из-за отсутствия ИНН или т.п.");
                                    break;

                                case "СведенийОЗапретеНет":
                                    sb.AppendLine("Сведения о запрете субъекта отсутствуют.");
                                    break;

                                case "УсловияЗапрета":
                                    // <Условие ДатаЗаявления="2024-02-01" ВремяЗаявления="10:12:45"
                                    // НачалоДействия="2024-02-02">3</Условие>
                                    foreach (XmlNode details in data.ChildNodes)
                                    {
                                        string date = details.Attributes!["ДатаЗаявления"]!.Value;
                                        string time = details.Attributes["ВремяЗаявления"]!.Value;
                                        string from = details.Attributes!["НачалоДействия"]!.Value;
                                        string kind = details.InnerText;

                                        string text = int.Parse(kind) switch
                                        {
                                            0 => "0 - снят",
                                            1 => "1 - полный",
                                            2 => "2 - банкам",
                                            3 => "3 - мфо",
                                            4 => "4 - банкам дист.",
                                            5 => "5 - мфо дист.",
                                            _ => kind
                                        };

                                        sb.AppendLine($"Запрет {text} с {from} (заявление {date} {time})."); //TODO sort?
                                    }
                                    break;

                                default:
                                    throw new Exception("Неизвестный тэг в КБКИ.");
                            }
                        }
                        break;

                    default:
                        throw new Exception("Неизвестный тэг в Сведениях.");
                }
            }

            foreach (var item in list.Values.ToList())
            {
                sb.AppendLine($"Договор {item.Id} на {item.Date} {item.Val} {item.Sum}");
                total += long.Parse(item.Sum.Replace(".", "")); //TODO separate RUB, etc.
            }

            sb.AppendLine().AppendLine($"Total: {total / 100:#.00}").AppendLine(); //TODO XSLT
        }
        // </Сведения>
        // </ОтветНаЗапросСведений>

        await File.WriteAllTextAsync(report, sb.ToString(), Encoding.UTF8); //TODO Config Encoding
        
        return File.Exists(report) ? 0 : 1;
    }
}
