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

public class ApiHelper
{
    public static (string Dir, string Mask) GetDirMask(string path)
    {
        var dir = Path.GetDirectoryName(path);
        string mask = Path.GetFileName(path);

        if (string.IsNullOrEmpty(dir))
        {
            return (string.Empty, mask);
        }
        else
        {
            Directory.CreateDirectory(dir);
            return (dir, mask);
        }
    }

    /// <summary>
    /// Разобрать ответ сервера, записать файл с текстом полученной квитанции
    /// и вернуть код результата запроса
    /// и содержимое квитанции.
    /// </summary>
    /// <param name="file">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig
    /// в исходном формате PKCS#7.</param>
    /// <param name="response">Комплексный ответ сервера.</param>
    /// <returns>Код результата запроса и содержимое квитанции, возвращенные
    /// сервером.</returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static async Task<(int Result, string Content)>
        WriteResultFileAsync(string file, HttpResponseMessage response, bool clean)
    {
        if (File.Exists(file))
        {
            File.Delete(file);

            if (File.Exists(file))
            {
                throw new UnauthorizedAccessException("Result file not deleted.");
            }
        }

        int result = (int)response.StatusCode;
        Console.WriteLine($"Status code: {result} {response.StatusCode}");
        byte[] data = await response.Content.ReadAsByteArrayAsync();

        if (clean)
        {
            // Write signed file
            await File.WriteAllBytesAsync(file + ".sig", data);

            // Clean data
            //data = PKCS7.CleanSign(data);
            data = await ASN1.CleanSignAsync(data);

            // Write clean XML
            await File.WriteAllBytesAsync(file, data);
        }
        else
        {
            // Write signed file
            await File.WriteAllBytesAsync(file, data);

            // Clean data
            //data = PKCS7.CleanSign(data);
            data = await ASN1.CleanSignAsync(data);
        }

        string content = Encoding.UTF8.GetString(data);

        return (result, content);
    }

    /// <summary>
    /// Прочитать Идентификатор ответа и предполагаемое время ожидания.
    /// </summary>
    /// <param name="xml">Ответное сообщение.</param>
    /// <returns>Идентификатор ответа и предполагаемое время ожидания.</returns>
    public static (string Id, int Sleep) ParseIdSleepFromXml(string xml)
    {
        /* Пример квитанции, содержащей идентификатор ответа
         *
         * <?xml version="1.0" encoding="UTF-8"?>
         * <Результат Версия="1.2" ОГРН="1077333000003">
         * <ИдентификаторОтвета
         * ИдентификаторЗапроса="ef15a678-637f-11ea-83b8-21758a33c94a"
         * [ВремяГотовности="1000"]>
         * 6afbdd01-6380-11ea-83b8-21758a33c94a
         * </ИдентификаторОтвета>
         * </Результат>
         */

        XmlDocument doc = new();
        doc.LoadXml(xml);
        string id = doc.DocumentElement!.FirstChild!.InnerText;

        //TODO check where is this Attribute
        var time = doc.DocumentElement!.FirstChild!.Attributes?["ВремяГотовности"]?.Value;
        int sleep = time is null ? 1000 : int.Parse(time);

        return (id, sleep);
    }

    /// <summary>
    /// Прочитать код и текст ошибки их XML.
    /// </summary>
    /// <param name="xml">Ответное сообщение.</param>
    /// <returns>Код и текст ошибки.</returns>
    public static (string Result, string Message) ParseErrorFromXml(string xml)
    {
        /* Пример отрицательной квитанции на запрос сведений о среднемесячных
         * платежах Субъекта
         * 
         * <?xml version="1.0" encoding="UTF-8"?>
         * <Результат Версия="1.2" ОГРН="1077333000003">
         * <Ошибка Код="9">Запрос не соответствует схеме:
         * Attribute 'КодДУЛ' is required in element 'ДокументЛичности'
         * Error location:
         * ЗапросСведенийОПлатежах / Запрос / Субъект / ДокументЛичности
         * </Ошибка>
         * </Результат>
         */

        XmlDocument doc = new();
        doc.LoadXml(xml);
        string errKod = doc.DocumentElement!.FirstChild!.Attributes![0].Value;
        string errMsg = doc.DocumentElement!.FirstChild!.InnerText;

        return (errKod, errMsg);
    }
}
