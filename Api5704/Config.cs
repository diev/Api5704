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

namespace Api5704;

/// <summary>
/// Файл настроек Api5704.config.json
/// </summary>
public class Config
{
    /// <summary>
    /// Отпечаток сертификата клиента, зарегистрированного на сервере в ЛК и
    /// имеющего допуск к серверу.
    /// </summary>
    public string MyThumbprint { get; set; } = "2756273e9e3c99ee435ffeaa79505b10214321c8";
    
    /// <summary>
    /// Показывать дамп сертификата клиента при подключении.
    /// </summary>
    public bool VerboseClient { get; set; } = true;

    /// <summary>
    /// Адрес базового URL API тестовой системы:
    /// https://reports.demo.nbki.ru/qbch/
    /// Адрес базового URL API тестовой системы, планируемой к выпуску:
    /// https://reports.test-alfa.nbki.ru/qbch/
    /// Адрес базового URL API промышленной системы:
    /// https://ssp.nbki.ru/qbch/
    /// </summary>
    public string ServerAddress { get; set; } = "https://ssp.nbki.ru/qbch/";

    /// <summary>
    /// Проверять отпечаток сервера
    /// (имеет смысл при указанном ServerThumbprint).
    /// </summary>
    public bool ValidateThumbprint { get; set; } = false;

    /// <summary>
    /// Отпечаток сертификата сервера ServerAddress
    /// (имеет смысл при включении ValidateThumbprint).
    /// </summary>
    public string ServerThumbprint { get; set; } = string.Empty;
    
    /// <summary>
    /// Проверять валидность сертификатов для подключения.
    /// </summary>
    public bool ValidateTls { get; set; } = true;
    
    /// <summary>
    /// Показывать дамп сертификата сервера при подключении.
    /// </summary>
    public bool VerboseServer { get; set; } = true;
    
    /// <summary>
    /// Использовать прокси для подключения.
    /// </summary>
    public bool UseProxy { get; set; } = false;
    
    /// <summary>
    /// Адрес и порт сервера прокси
    /// (имеет смысл при включении UseProxy).
    /// </summary>
    public string ProxyAddress { get; set; } = "http://192.168.2.1:3128";
    
    /// <summary>
    /// Подписывать файлы при отправке или же они уже подписаны
    /// (требует установленного КриптоПро).
    /// </summary>
    public bool SignFile { get; set; } = true;
    
    /// <summary>
    /// Извлекать полученные файлы из контейнера с подписью КриптоПро
    /// (не требует установленного КриптоПро).
    /// </summary>
    public bool CleanSign { get; set; } = true;
    
    /// <summary>
    /// Число попыток повтора при получении сведений.
    /// </summary>
    public int MaxRetries { get; set; } = 10;
    
    /// <summary>
    /// Папка и маска исходных файлов для пакетной отправки запросов.
    /// Ее наличие при отсутствии параметров запускает пакетный режим.
    /// Файлы из нее удаляются при получении соответствующих им файлов
    /// со сведениями.
    /// </summary>
    public string DirSource { get; set; } = @"OUT\*.xml";
    
    /// <summary>
    /// Архивная папка и шаблон имени для файлов отправленных запросов.
    /// </summary>
    public string DirRequests { get; set; } = @"Requests\{name}.{date}.{guid}.request.xml";

    /// <summary>
    /// Архивная папка и шаблон имени для полученных файлов сведений
    /// на отправленные запросы.
    /// </summary>
    public string DirAnswers { get; set; } = @"Answers\{name}.{date}.{guid}.answer.xml";

    /// <summary>
    /// Архивная папка и шаблон имени для полученных файлов квитанций
    /// на отправленные запросы.
    /// </summary>
    public string DirResults { get; set; } = @"Results\{name}.{date}.{guid}.result.xml";

    /// <summary>
    /// Архивная папка и шаблон имени для файлов сводного отчета
    /// из полученных файлов квитанций (окончательный результат).
    /// </summary>
    public string DirReports { get; set; } = @"Results\{name}.{date}.{guid}.result.txt";

    /// <summary>
    /// Путь к утилите командной строки КриптоПро.
    /// </summary>
    public string CspTest { get; set; } =
        @"C:\Program Files\Crypto Pro\CSP\csptest.exe";
    
    /// <summary>
    /// Командная строка подписывания файла для утилиты КриптоПро.
    /// </summary>
    public string CspTestSignFile { get; set; } =
        "-sfsign -sign -in %1 -out %2 -my %3 -add -addsigtime";
}
