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

namespace Api5704;

public class Api
{
    private static readonly TlsClient _client = new();
    private static readonly Config _config = Program.Config;

    // API
    public const string certadd = nameof(certadd);
    public const string certrevoke = nameof(certrevoke);
    public const string dlanswer = nameof(dlanswer);
    public const string dlput = nameof(dlput);
    public const string dlputanswer = nameof(dlputanswer);
    public const string dlrequest = nameof(dlrequest);

    // Extra
    public const string auto = nameof(auto); // dlrequest + dlanswer
    public const string dir = nameof(dir); // dlrequest + dlanswer

    /// <summary>
    /// POST
    /// certadd – добавление нового сертификата абонента.
    /// certrevoke – отзыв сертификата абонента.
    /// </summary>
    /// <param name="cmd">Метод API ("certadd" или "certrevoke").</param>
    /// <param name="id">Идентификатор Guid (вида A6563526-A3F3-4D4E-A923-E41E93F1D921).</param>
    /// <param name="cert">Имя файла с сертификатом в кодировке DER (с расширением .cer).</param>
    /// <param name="sign">Имя файла с сертификатом в кодировке DER.
    /// Если в конфиге SignFile: true и файл без расширения .sig, то он будет подписан ЭП в формате PKCS#7
    /// (рядом появится файл с расширением .cer.sig, который и будет отправлен).</param>
    /// <param name="resultFile">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <returns>
    /// 200 – квитанция содержит информацию об успешной обработке запроса;
    /// 400 – квитанция содержит информацию об ошибке.
    /// </returns>
    public static async Task PostCertAsync(string cmd, string id, string cert, string sign, string resultFile)
    {
        if (!File.Exists(cert))
        {
            throw new FileNotFoundException("Cert file not found.", cert);
        }

        if (!File.Exists(sign))
        {
            throw new FileNotFoundException("Sign file not found.", sign);
        }

        if (_config.SignFile && Path.GetExtension(sign) != ".sig")
        {
            await PKCS7.SignFileAsync(sign, sign + ".sig");
            sign += ".sig";
        }

        MultipartFormDataContent content = new()
        {
            { new StringContent(id), nameof(id) },
            { new ByteArrayContent(File.ReadAllBytes(cert)), nameof(cert) },
            { new ByteArrayContent(File.ReadAllBytes(sign)), nameof(sign) }
        };

        using HttpResponseMessage response = await _client.PostAsync(cmd, content);
        (int result, _) = await ApiHelper.WriteResultFileAsync(resultFile, response, _config.CleanSign);

        string o = result switch
        {
            200 => "квитанция содержит информацию об успешной обработке запроса.",
            400 => "квитанция содержит информацию об ошибке.",
            _ => "недокументированный ответ.",
        };

        Console.WriteLine($"Ответ {result} - {o}");

        Environment.Exit(result);
    }

    /// <summary>
    /// POST
    /// dlput – передача от БКИ данных, необходимых для формирования и предоставления пользователям
    /// кредитных историй сведений о среднемесячных платежах Субъекта.
    /// dlrequest – запрос сведений о среднемесячных платежах Субъекта.
    /// Extra
    /// auto - запрос и получение сведений (dlrequest + dlanswer) за один запуск.
    /// </summary>
    /// <param name="cmd">Метод API ("dlput" или "dlrequest") или Extra "auto".</param>
    /// <param name="file">Имя файла с запросом в кодировке utf-8.
    /// Если в конфиге SignFile: true и файл без расширения .sig, то он будет подписан ЭП в формате PKCS#7
    /// (рядом появится файл с расширением .sig, который и будет отправлен).</param>
    /// <param name="resultFile">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <param name="answerFile">Имя файла для получения информации (Extra, только при "dlauto").
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <returns>
    /// 200 – результат запроса содержит информацию о результатах загрузки данных в базу данных КБКИ;
    /// 202 – результат запроса содержит квитанцию с идентификатором ответа;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке;
    /// 495 - сервер не признает наш сертификат - доступ отказан.
    /// Extra (dlanswer):
    /// 200 – результат запроса содержит сведения о среднемесячных платежах Субъекта;
    /// 202 – результат запроса содержит квитанцию с информацией об ошибке «Ответ не готов»;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке, кроме ошибки «Ответ не готов».
    /// В случае получения ошибки «Ответ не готов» клиент должен повторить запрос не ранее, чем через 1 секунду.
    /// 404 - неправильный идентификатор.
    /// </returns>
    public static async Task PostRequestAsync(string cmd, string file, string resultFile, string? answerFile = null)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("File not found.", file);
        }

        if (_config.SignFile && Path.GetExtension(file) != ".sig")
        {
            string sign = file + ".sig";
            await PKCS7.SignFileAsync(file, sign);
            file = sign;
        }

        ByteArrayContent content = new(File.ReadAllBytes(file));
        using HttpResponseMessage response = await _client.PostAsync(cmd.Equals(auto) ? dlrequest : cmd, content);

        if ((int)response.StatusCode == 495)
        {
            Console.WriteLine("495 - сервер не признает наш сертификат - доступ отказан.");

            Environment.Exit(495);
        }

        (int result, string xml) = await ApiHelper.WriteResultFileAsync(resultFile, response, _config.CleanSign);
        response.Dispose();

        string o = result switch
        {
            200 => "результат запроса содержит информацию о результатах загрузки данных в базу данных КБКИ.",
            202 => "результат запроса содержит квитанцию с идентификатором ответа.",
            400 => "результат запроса содержит квитанцию с информацией об ошибке.",
            _ => "недокументированный ответ.",
        };

        Console.WriteLine($"Ответ {result} - {o}");

        if (!cmd.Equals(auto))
        {
            Environment.Exit(result);
        }

        if (result != 202)
        {
            Console.WriteLine("Автоматическое получение ответа невозможно.");

            Environment.Exit(result);
        }

        // GetAnswerAsync

        Console.WriteLine("Этап 2 - получение сведений...");

        XmlDocument doc = new();
        doc.LoadXml(xml);
        string id = doc.DocumentElement!.FirstChild!.InnerText;

        if (answerFile is null)
        {
            throw new ArgumentNullException(answerFile, "Answer filename required.");
        }

        await GetAnswerAsync(dlanswer, id, answerFile);
    }

    /// <summary>
    /// GET
    /// dlanswer – получение сведений о среднемесячных платежах Субъекта по идентификатору ответа.
    /// dlputanswer – получение информации о результатах загрузки данных, необходимых для формирования
    /// и предоставления пользователям кредитных историй сведений о среднемесячных платежах Субъекта,
    /// в базу данных КБКИ.
    /// </summary>
    /// <param name="cmd">Метод API ("dlanswer" или "dlputanswer").</param>
    /// <param name="id">Идентификатор Guid (вида A6563526-A3F3-4D4E-A923-E41E93F1D921)
    /// или файл результата отправки запроса, где его можно взять.</param>
    /// <param name="resultFile">Имя файла для получения информации.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <returns>
    /// 200 – результат запроса содержит сведения о среднемесячных платежах Субъекта;
    /// 202 – результат запроса содержит квитанцию с информацией об ошибке «Ответ не готов»;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке, кроме ошибки «Ответ не готов».
    /// В случае получения ошибки «Ответ не готов» клиент должен повторить запрос не ранее, чем через 1 секунду.
    /// 404 - неправильный идентификатор.
    /// </returns>
    public static async Task GetAnswerAsync(string cmd, string id, string resultFile)
    {
        int result = 500;
        int retries = 0;

        if (File.Exists(id))
        {
            XmlDocument doc = new();
            doc.Load(id);
            id = doc.DocumentElement!.FirstChild!.InnerText;
        }

        if (id.Length != 36) // check Guid
        {
            Console.WriteLine($"Неправильный id - '{id}'.");

            Environment.Exit(404);
        }

        while (++retries <= _config.MaxRetries)
        {
            using HttpResponseMessage response = await _client.GetAsync(cmd + "?id=" + id);
            (result, _) = await ApiHelper.WriteResultFileAsync(resultFile, response, _config.CleanSign);
            response.Dispose();

            if (result != 202) break;

            Thread.Sleep(1000);
            Console.WriteLine($"Ответ не готов - попытка {retries}...");
        }

        string o = result switch
        {
            200 => "результат запроса содержит сведения о среднемесячных платежах Субъекта.",
            202 => "результат запроса содержит квитанцию с информацией об ошибке «Ответ не готов».",
            400 => "результат запроса содержит квитанцию с информацией об ошибке.",
            _ => "недокументированный ответ.",
        };

        Console.WriteLine($"Ответ {result} - {o}");

        Environment.Exit(result);
    }
}
