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

using System.Xml;

namespace Api6775;

/// <summary>
/// Кредитные истории.
/// Передача сведений о среднемесячных платежах.
/// https://www.cbr.ru/ckki/transfer_inform/
/// </summary>
public class Api
{
    private const string _sig = ".sig";

    private static readonly TlsClient _client = new();
    private static readonly Config _config = Program.Config;

    /// <summary>
    /// Добавление нового сертификата абонента.
    /// </summary>
    public const string certadd = nameof(certadd);

    /// <summary>
    /// Отзыв сертификата абонента.
    /// </summary>
    public const string certrevoke = nameof(certrevoke);

    /// <summary>
    /// Получение сведений о среднемесячных платежах Субъекта по
    /// идентификатору ответа.
    /// </summary>
    public const string dlanswer = nameof(dlanswer);

    /// <summary>
    /// Передача от БКИ данных, необходимых для формирования и
    /// предоставления пользователям кредитных историй сведений о
    /// среднемесячных платежах Субъекта.
    /// </summary>
    public const string dlput = nameof(dlput);

    /// <summary>
    /// Получение информации о результатах загрузки данных, необходимых
    /// для формирования и предоставления пользователям кредитных историй
    /// сведений о среднемесячных платежах Субъекта, в базу данных КБКИ.
    /// </summary>
    public const string dlputanswer = nameof(dlputanswer);

    /// <summary>
    /// Запрос сведений о среднемесячных платежах Субъекта.
    /// </summary>
    public const string dlrequest = nameof(dlrequest);

    /// <summary>
    /// POST
    /// <list>
    /// <item>certadd – добавление нового сертификата абонента.</item>
    /// <item>certrevoke – отзыв сертификата абонента.</item>
    /// </list>
    /// </summary>
    /// <param name="cmd">Метод API ("certadd" или "certrevoke").</param>
    /// <param name="id">Идентификатор Guid
    /// (вида A6563526-A3F3-4D4E-A923-E41E93F1D921).</param>
    /// <param name="cert">Имя файла с сертификатом в кодировке DER
    /// (с расширением .cer).</param>
    /// <param name="sign">Имя файла с сертификатом в кодировке DER.
    /// Если в конфиге SignFile: true и файл без расширения .sig,
    /// то он будет подписан ЭП в формате PKCS#7 (рядом появится файл
    /// с расширением .cer.sig, который и будет отправлен).</param>
    /// <param name="resultFile">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig
    /// в исходном формате PKCS#7.</param>
    /// <returns>
    /// <list>
    /// <item>200 – квитанция содержит информацию об успешной обработке
    /// запроса;</item>
    /// <item>400 - квитанция содержит информацию об ошибке.</item>
    /// </list>
    /// </returns>
    public static async Task<int> PostCertAsync(string cmd,
        string id, string cert, string sign, string resultFile)
    {
        /* certadd – добавление нового сертификата абонента.
         * Метод передачи запроса – POST.
         * Параметры запроса – id, cert, sign.
         * Способ передачи параметров – в теле запроса в формате
         * multipart/form-data.
         * В качестве значения параметра id передается идентификатор запроса.
         * В качестве значения параметра cert передается новый сертификат
         * абонента в кодировке DER.
         * В качестве значения параметра sign передается УЭП нового
         * сертификата абонента, выполненная с использованием другого
         * действующего сертификата абонента.
         * Результат запроса – квитанция, сформированная в соответствии со
         * схемой qcb_result.xsd
         * В зависимости от содержания квитанции, устанавливаются следующие
         * коды состояния HTTP:
         * 200 – квитанция содержит информацию об успешной обработке запроса;
         * 400 – квитанция содержит информацию об ошибке.
         */

        /* certrevoke – отзыв сертификата абонента.
         * Метод передачи запроса – POST.
         * Параметры запроса – id, cert, sign.
         * Способ передачи параметров – в теле запроса в формате
         * multipart/form-data.
         * В качестве значения параметра id передается идентификатор запроса.
         * В качестве значения параметра cert передается отзываемый сертификат
         * абонента в кодировке DER.
         * В качестве значения параметра sign передается УЭП отзываемого
         * сертификата абонента, выполненная с использованием любого
         * действующего сертификата абонента (в том числе и отзываемого).
         * Результат запроса – квитанция, сформированная в соответствии со
         * схемой qcb_result.xsd
         * В зависимости от содержания квитанции, устанавливаются следующие
         * коды состояния HTTP:
         * 200 – квитанция содержит информацию об успешной обработке запроса;
         * 400 – квитанция содержит информацию об ошибке.
         * При отзыве последнего действующего сертификата абонента установка
         * новых сертификатов возможна только посредством подписания
         * дополнительного соглашения к договору присоединения между абонентом
         * и КБКИ, устанавливающего новый (новые) сертификат (сертификаты)
         * абонента.
         */

        if (!File.Exists(cert))
            throw new FileNotFoundException("Cert file not found.", cert);

        if (!File.Exists(sign))
            throw new FileNotFoundException("Sign file not found.", sign);

        if (_config.SignFile &&
            !Path.GetExtension(sign).Equals(_sig, StringComparison.OrdinalIgnoreCase))
        {
            await PKCS7.SignFileAsync(sign, sign + _sig);
            sign += _sig;
        }

        MultipartFormDataContent content = new()
        {
            { new StringContent(id), nameof(id) },
            { new ByteArrayContent(File.ReadAllBytes(cert)), nameof(cert) },
            { new ByteArrayContent(File.ReadAllBytes(sign)), nameof(sign) }
        };

        using HttpResponseMessage response = await _client.PostAsync(cmd, content);
        (int result, _) = await ApiHelper.WriteResultFileAsync(resultFile, response, _config.CleanSign);

        switch (result)
        {
            case 200:
                Console.WriteLine(
                    "200 - квитанция содержит информацию " +
                    "об успешной обработке запроса.");
                break;
            case 400:
                Console.WriteLine(
                    "400 - квитанция содержит информацию " +
                    "об ошибке.");
                break;
            default:
                throw new InvalidOperationException(
                    $"{result} - недокументированный ответ.");
        }

        return result;
    }

    /// <summary>
    /// POST
    /// <list>
    /// <item>dlput – передача от БКИ данных, необходимых для формирования и
    /// предоставления пользователям кредитных историй сведений о
    /// среднемесячных платежах Субъекта.</item>
    /// <item>dlrequest – запрос сведений о среднемесячных платежах Субъекта.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="cmd">Методы API: "dlput", "dlrequest".</param>
    /// <param name="file">Имя файла с запросом в кодировке utf-8.
    /// Если в конфиге SignFile: true и файл без расширения .sig,
    /// то он будет подписан ЭП в формате PKCS#7
    /// (рядом появится файл с расширением .sig, который и будет
    /// отправлен).</param>
    /// <param name="resultFile">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig
    /// в исходном формате PKCS#7.</param>
    /// <returns>
    /// Код результата запроса и содержимое квитанции, возвращенные сервером.
    /// <list>
    /// <item>200 – результат запроса содержит информацию о результатах
    /// загрузки данных в базу данных КБКИ;</item>
    /// <item>202 – результат запроса содержит квитанцию с идентификатором
    /// ответа;</item>
    /// <item>400 - результат запроса содержит квитанцию с информацией об
    /// ошибке.</item>
    /// </list>
    /// </returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<(int Result, string Xml)> PostRequestAsync(string cmd,
        string file, string resultFile)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException("File not found.", file);

        /* Запросы формируются с использованием символов кодировки UTF-8 в
         * формате XML в соответствии с xsd-схемой запроса, подписываются УЭП
         * абонента, полученная ЭП присоединяется к запросу.
         * Запрос с присоединенной ЭП передается на сервер КБКИ в теле POST-
         * запроса в формате PKCS#7.
         * Дата и время в запросах, квитанциях и ответах указываются по
         * московскому времени.
         */

        if (_config.SignFile &&
            !Path.GetExtension(file).Equals(_sig, StringComparison.OrdinalIgnoreCase))
        {
            string sign = file + _sig;
            await PKCS7.SignFileAsync(file, sign);
            file = sign;
        }

        ByteArrayContent content = new(File.ReadAllBytes(file));

        using HttpResponseMessage response = await _client.PostAsync(cmd, content);

        /* При установке соединения сервер КБКИ проверяет действительность и
         * принадлежность сертификата абонента, в случае выявления ошибок
         * возвращает пустой ответ с кодом состояния HTTP – 495 и разрывает
         * соединение.
         */

        if ((int)response.StatusCode == 495)
            throw new UnauthorizedAccessException(
                "495 - сервер не признает наш сертификат - доступ отказан.");

        /* В случае отсутствия ошибок для запросов, требующих подготовки
         * ответа, КБКИ формирует и возвращает ответ в течение 4 секунд.
         * В остальных случаях, а также если подготовка ответа превышает
         * 4 секунды, КБКИ формирует и возвращает квитанцию.
         * КБКИ формирует и возвращает квитанцию, которая в зависимости
         * от результатов проверки запроса может содержать информацию об
         * успешной загрузке запроса, об ошибке либо идентификатор ответа,
         * по которому ответ может быть получен после его готовности.
         * В случае запросов, требующих подготовки ответа, КБКИ готовит
         * ответ.
         */

        (int result, string xml) = await ApiHelper.WriteResultFileAsync(
            resultFile, response, _config.CleanSign);
        response.Dispose();

        /* Результат запроса – сведения о среднемесячных платежах Субъекта,
         * сформированные в соответствии со схемой qcb_answer.xsd, либо
         * квитанция, сформированная в соответствии со схемой qcb_result.xsd
         * Результат должен быть подготовлен и возвращен в течение 4 секунд.
         * Для снижения нагрузки в квитанцию, содержащую идентификатор
         * ответа, сервер может поместить атрибут «ВремяГотовности», указав
         * в качестве значения время (в миллисекундах), требующееся серверу
         * на подготовку ответа. При наличии атрибута «ВремяГотовности»
         * клиент должен обращаться за получением сведений о среднемесячных
         * платежах Субъекта не ранее, чем по истечении времени, указанного
         * в атрибуте.
         */

        switch (result)
        {
            case 200:
                Console.WriteLine(
                    "200 - результат запроса содержит информацию " +
                    "о результатах загрузки данных в базу данных КБКИ.");
                break;
            case 202:
                Console.WriteLine(
                    "202 - результат запроса содержит квитанцию " +
                    "с идентификатором ответа.");
                break;
            case 400:
                Console.WriteLine(
                    "400 - результат запроса содержит квитанцию " +
                    "с информацией об ошибке.");
                break;
            default:
                throw new InvalidOperationException(
                    $"{result} - недокументированный ответ.");
        }

        return (result, xml);
    }

    /// <summary>
    /// GET
    /// <list>
    /// <item>dlanswer – получение сведений о среднемесячных платежах
    /// Субъекта по идентификатору ответа.</item>
    /// <item>dlputanswer – получение информации о результатах загрузки
    /// данных, необходимых для формирования и предоставления пользователям
    /// кредитных историй сведений о среднемесячных платежах Субъекта,
    /// в базу данных КБКИ.</item>
    /// </list>
    /// </summary>
    /// <param name="cmd">Метод API ("dlanswer" или "dlputanswer").</param>
    /// <param name="id">Идентификатор Guid
    /// (вида A6563526-A3F3-4D4E-A923-E41E93F1D921)
    /// или файл результата отправки запроса, где его можно взять.</param>
    /// <param name="resultFile">Имя файла для получения информации.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig
    /// в исходном формате PKCS#7.</param>
    /// <returns>
    /// <list>
    /// <item>
    /// 200 – результат запроса содержит сведения о среднемесячных платежах
    /// Субъекта;</item>
    /// <item>202 – результат запроса содержит квитанцию с информацией
    /// об ошибке «Ответ не готов»;</item>
    /// <item>400 - результат запроса содержит квитанцию с информацией
    /// об ошибке, кроме ошибки «Ответ не готов».</item>
    /// </list>
    /// В случае получения ошибки «Ответ не готов» клиент должен
    /// повторить запрос не ранее, чем через 1 секунду.
    /// </returns>
    public static async Task<int> GetAnswerAsync(string cmd,
        string id, string resultFile)
    {
        if (File.Exists(id))
        {
            XmlDocument doc = new();
            doc.Load(id);
            id = doc.DocumentElement!.FirstChild!.InnerText;
        }

        if (id.Length != 36) //TODO check Guid
        {
            throw new ArgumentException(
                "404 - Неправильный id '{id}'.", id);
        }

        /* Для получения ответа (в том числе для повторного получения ответа)
         * абонент устанавливает новое соединение и GET запросом передает
         * полученный в квитанции (или в ранее полученном ответе)
         * идентификатор ответа.
         * В качестве значения параметра id передается значение идентификатора
         * ответа, содержащегося в квитанции, полученной при направлении
         * запроса.
         */

        using HttpResponseMessage response = await _client.GetAsync(cmd + "?id=" + id);

        /* КБКИ формирует ответ с использованием символов кодировки UTF-8 в
         * формате XML в соответствии с xsd-схемой ответа, подписывает УЭП,
         * присоединяет к ответу и возвращает абоненту в формате PKCS#7.
         * Ответ по идентификатору должен быть доступен для получения
         * абонентом в течение не менее 8 часов после его формирования.
         * В случае если ответ с указанным номером еще не готов, КБКИ
         * удерживает соединение до его готовности либо возвращает квитанцию
         * с кодом ошибки «Ответ не готов».
         * При возникновении ошибок возвращается квитанция с кодом и
         * описанием ошибки. Если во время ожидания ответа произойдет разрыв
         * соединения без возврата каких-либо сообщений, процедура установки
         * соединения повторяется.
         */

        (int result, _) = await ApiHelper.WriteResultFileAsync(
            resultFile, response, _config.CleanSign);
        response.Dispose();

        /* Результат запроса – сведения о среднемесячных платежах Субъекта
         * сформированные в соответствии со схемой qcb_answer.xsd либо
         * квитанция, сформированная в соответствии со схемой qcb_result.xsd
         * В случае неготовности ответа соединение удерживается сервером на
         * время его подготовки либо возвращается квитанция, содержащая код
         * ошибки «Ответ не готов».
         */

        switch (result)
        {
            case 200:
                Console.WriteLine(
                    "200 - результат запроса содержит сведения " +
                    "о среднемесячных платежах Субъекта.");
                break;
            case 202:
                Console.WriteLine(
                    "202 - результат запроса содержит квитанцию "
                    + "с информацией об ошибке «Ответ не готов».");
                break;
            case 400:
                Console.WriteLine(
                    "400 - результат запроса содержит квитанцию " +
                    "с информацией об ошибке, кроме ошибки «Ответ не готов».");
                break;
            default:
                throw new InvalidOperationException(
                    $"{result} - недокументированный ответ.");
        }

        return result;
    }
}
