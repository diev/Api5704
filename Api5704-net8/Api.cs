#region License
/*
Copyright 2022-2023 Dmitrii Evdokimov
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

namespace Api5704;

internal static class Api
{
    private static readonly TlsClient _client = new();

    private static bool SignFile => ConfigReader.GetBool(nameof(SignFile));
    private static bool CleanSign => ConfigReader.GetBool(nameof(CleanSign));

    #region public

    public const string certadd = nameof(certadd);
    public const string certrevoke = nameof(certrevoke);
    public const string dlanswer = nameof(dlanswer);
    public const string dlput = nameof(dlput);
    public const string dlputanswer = nameof(dlputanswer);
    public const string dlrequest = nameof(dlrequest);

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

        if (SignFile && Path.GetExtension(sign) != ".sig")
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
        int result = await WriteResultFileAsync(resultFile, response);

        string o = result switch
        {
            200 => "квитанция содержит информацию об успешной обработке запроса.",
            400 => "квитанция содержит информацию об ошибке.",
            _ => "недокументированный ответ.",
        };

        Console.WriteLine($"{result} - {o}");

        Environment.Exit(result);
    }

    /// <summary>
    /// POST
    /// dlput – передача от БКИ данных, необходимых для формирования и предоставления пользователям
    /// кредитных историй сведений о среднемесячных платежах Субъекта.
    /// dlrequest – запрос сведений о среднемесячных платежах Субъекта.
    /// </summary>
    /// <param name="cmd">Метод API ("dlput" или "dlrequest").</param>
    /// <param name="file">Имя файла с запросом в кодировке utf-8.
    /// Если в конфиге SignFile: true и файл без расширения .sig, то он будет подписан ЭП в формате PKCS#7
    /// (рядом появится файл с расширением .sig, который и будет отправлен).</param>
    /// <param name="resultFile">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <returns>
    /// 200 – результат запроса содержит информацию о результатах загрузки данных в базу данных КБКИ;
    /// 202 – результат запроса содержит квитанцию с идентификатором ответа;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке.
    /// </returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task PostRequestAsync(string cmd, string file, string resultFile)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("File not found.", file);
        }

        if (SignFile && Path.GetExtension(file) != ".sig")
        {
            string sign = file + ".sig";
            await PKCS7.SignFileAsync(file, sign);
            file = sign;
        }

        ByteArrayContent content = new(File.ReadAllBytes(file));
        using HttpResponseMessage response = await _client.PostAsync(cmd, content);
        int result = await WriteResultFileAsync(resultFile, response);

        string o = result switch
        {
            200 => "результат запроса содержит информацию о результатах загрузки данных в базу данных КБКИ.",
            202 => "результат запроса содержит квитанцию с идентификатором ответа.",
            400 => "результат запроса содержит квитанцию с информацией об ошибке.",
            _ => "недокументированный ответ.",
        };

        Console.WriteLine($"{result} - {o}");

        Environment.Exit(result);
    }

    /// <summary>
    /// GET
    /// dlanswer – получение сведений о среднемесячных платежах Субъекта по идентификатору ответа.
    /// dlputanswer – получение информации о результатах загрузки данных, необходимых для формирования
    /// и предоставления пользователям кредитных историй сведений о среднемесячных платежах Субъекта,
    /// в базу данных КБКИ.
    /// </summary>
    /// <param name="cmd">Метод API ("dlanswer" или "dlputanswer").</param>
    /// <param name="id">Идентификатор Guid (вида A6563526-A3F3-4D4E-A923-E41E93F1D921).</param>
    /// <param name="resultFile">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <returns>
    /// 200 – результат запроса содержит сведения о среднемесячных платежах Субъекта;
    /// 202 – результат запроса содержит квитанцию с информацией об ошибке «Ответ не готов»;
    /// 400 – результат запроса содержит квитанцию с информацией об ошибке, кроме ошибки «Ответ не готов».
    /// В случае получения ошибки «Ответ не готов» клиент должен повторить запрос не ранее, чем через 1 секунду.
    /// </returns>
    public static async Task GetAnswerAsync(string cmd, string id, string resultFile)
    {
        int result = 500;
        int retries = 0;

        while (++retries <= 10)
        { 
            using HttpResponseMessage response = await _client.GetAsync(cmd + "?id=" + id);
            result = await WriteResultFileAsync(resultFile, response);

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

        Console.WriteLine($"{result} - {o}");

        Environment.Exit(result);
    }

    #endregion public
    #region private

    /// <summary>
    /// Разобрать ответ сервера, записать файл с текстом полученной квитанции и вернуть код результата запроса.
    /// </summary>
    /// <param name="file">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <param name="response">Комплексный ответ сервера.</param>
    /// <returns>Код результата запроса, возвращенный сервером.</returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    private static async Task<int> WriteResultFileAsync(string file, HttpResponseMessage response)
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

        if (CleanSign)
        {
            await File.WriteAllBytesAsync(file + ".sig", data);
            data = PKCS7.CleanSign(data);
        }

        await File.WriteAllBytesAsync(file, data);
        
        return result;
    }

    #endregion private
}
