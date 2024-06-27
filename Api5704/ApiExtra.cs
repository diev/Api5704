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

using System.Xml;
using System.Text;
using System;

namespace Api5704;

public static class ApiExtra
{
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
            }

            Thread.Sleep(1000);
        }

        Console.WriteLine($"Сведений получено: {count}.");

        return ret;
    }
}
