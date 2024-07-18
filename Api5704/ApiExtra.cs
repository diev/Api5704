#region License
/*
Copyright 2022-2024 Dmitrii Evdokimov
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

namespace Api5704;

public static class ApiExtra
{
    private record Agreement(string Id, string Date, string Val, string Sum);

    /// <summary>
    /// Пакетная обработка папок с запросами (Extra dir).
    /// </summary>
    /// <param name="source">Папка+маска с исходными запросами.</param>
    /// <param name="request">Папка+файл с отправленными запросами.</param>
    /// <param name="result">Папка+файл с полученными квитанциями.</param>
    /// <param name="answer">Папка+файл с полученными сведениями.</param>
    /// <returns>
    /// API этап 1 (dlrequest):
    /// 200 – результат запроса содержит информацию о результатах загрузки данных в базу данных КБКИ;
    /// 202 – результат запроса содержит квитанцию с идентификатором ответа;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке;
    /// 495 - сервер не признает наш сертификат - доступ отказан.
    /// API этап 2 (dlanswer):
    /// 200 – результат запроса содержит сведения о среднемесячных платежах Субъекта;
    /// 202 – результат запроса содержит квитанцию с информацией об ошибке «Ответ не готов»;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке, кроме ошибки «Ответ не готов».
    /// В случае получения ошибки «Ответ не готов» клиент должен повторить запрос не ранее, чем через 1 секунду.
    /// 404 - неправильный идентификатор.
    /// </returns>
    public static async Task<int> PostRequestFolderAsync(string source, string request, string result, string answer)
    {
        int ret = 1;

        if (string.IsNullOrEmpty(request) || string.IsNullOrEmpty(result) || string.IsNullOrEmpty(answer))
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
            }

            if (dateRequired)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");

                request = request.Replace("{date}", date);
                result = result.Replace("{date}", date);
                answer = answer.Replace("{date}", date);
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
            }

            Directory.CreateDirectory(Path.GetDirectoryName(request)!);
            Directory.CreateDirectory(Path.GetDirectoryName(result)!);
            Directory.CreateDirectory(Path.GetDirectoryName(answer)!);

            File.Copy(file, request, true);

            ret = await Api.PostRequestAsync(Api.dir, request, result, answer);

            if (ret == 495)
            {
                return ret;
            }

            if (File.Exists(answer))
            {
                File.Delete(file);
                count++;

                string report = Path.ChangeExtension(answer, "txt");
                await MakeReportAsync(answer, report);
            }

            Thread.Sleep(1000);
        }

        Console.WriteLine($"Сведений получено: {count}.");

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
        XmlDocument doc = new();
        doc.Load(answer);

        // СведенияОПлатежах
        var root = doc.DocumentElement
            ?? throw new Exception("XML не содержит корневого элемента.");

        // Титульная Часть
        var title = root.FirstChild
            ?? throw new Exception("XML не содержит Титульную часть."); ;

        // ФИО
        var fio = title.FirstChild
            ?? throw new Exception("XML не содержит ФИО."); //TODO XPath

        string fio3 =
            $"{fio.ChildNodes[0]?.InnerText} {fio.ChildNodes[1]?.InnerText} {fio.ChildNodes[2]?.InnerText}"
            .Trim();

        Dictionary<string,Agreement> list = [];

        foreach (XmlNode node in root.ChildNodes)
        {
            // КБКИ
            if (!node.LocalName.Equals("КБКИ", StringComparison.Ordinal))
                continue;

            // Обязательства
            var duties = node.FirstChild;

            // ОбязательствНет
            if (duties is null || !duties.HasChildNodes)
                continue;

            // БКИ
            var bki = duties.FirstChild;

            if (bki is null)
                continue;

            // Договор
            foreach (XmlNode agreement in bki.ChildNodes)
            {
                // УИД
                string id = agreement.Attributes!["УИД"]!.Value;
                
                // СреднемесячныйПлатеж
                var details = agreement.FirstChild;

                if (details is null)
                    continue;

                string date = details.Attributes!["ДатаРасчета"]!.Value;
                string val = details.Attributes["Валюта"]!.Value;
                string sum = details.InnerText;

                Agreement add = new(id, date, val, sum);

                if (list.TryGetValue(id, out var found))
                {
                    if (found is null ||
                        string.Compare(date, found.Date, false) > 0)
                    {
                        list.Remove(id);
                        list.Add(id, add);
                        continue;
                    }
                }
                else
                {
                    list.Add(id, add);
                    continue;
                }
            }
        }

        StringBuilder sb = new();
        sb.AppendLine(fio3).AppendLine();
        long total = 0;

        foreach (var item in list.Values.ToList())
        {
            sb.AppendLine($"Договор {item.Id} на {item.Date} {item.Val} {item.Sum}");
            total += long.Parse(item.Sum.Replace(".", "")); //TODO separate RUB, etc.
        }

        sb.AppendLine().AppendLine($"Total: {total/100:#.00}"); //TODO XSLT

        await File.WriteAllTextAsync(report, sb.ToString(), Encoding.UTF8); //TODO Config Encoding
        
        return File.Exists(report) ? 0 : 1;
    }
}
