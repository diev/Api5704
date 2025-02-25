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

using static Api5704.Api;
using static Api5704.ApiExtra;

namespace Api5704;

internal class Program
{
    public static Config Config { get; set; } = ConfigManager.Read();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!"); // :)
        int result = 0;

        if (args.Length == 0)
        {
            if (!string.IsNullOrEmpty(Config.DirSource))
            {
                string source = Config.DirSource;
                string dir = Path.GetDirectoryName(source)!;

                if (Directory.Exists(dir))
                {
                    Console.WriteLine(
                        @$"Параметры не указаны, но есть папка ""{dir}"".");
                    // source requests results answers reports
                    result = await ApiExtra.PostRequestFolderAsync(source,
                        Config.DirRequests, Config.DirResults,
                        Config.DirAnswers, Config.DirReports);

                    Environment.Exit(result);
                }
            }

            Usage();
        }

        string cmd = args[0].ToLower();

        try
        {
            switch (cmd)
            {
                // API
                case certadd:
                case certrevoke:
                    if (args.Length != 5) Usage();
                    // id cert.cer sign.sig result.xml
                    result = await PostCertAsync(cmd,
                        args[1], args[2], args[3], args[4]);
                    break;

                case dlput:
                case dlrequest:
                    if (args.Length != 3) Usage();
                    // request.xml result.xml
                    var (Result, Xml) = await PostRequestAsync(cmd,
                        args[1], args[2]);
                    result = Result;
                    break;

                case dlanswer:
                case dlputanswer:
                    if (args.Length != 3) Usage();
                    // id answer.xml
                    // result.xml answer.xml
                    result = await GetAnswerAsync(cmd,
                        args[1], args[2]);
                    break;

                // API Extra
                case auto:
                    if (args.Length != 5) Usage();
                    // request.xml result.xml answer.xml report.txt
                    result = await AutoRequestAsync(cmd,
                        args[1], args[2], args[3], args[4]);
                    // answer xml -> txt
                    result = await MakeReportAsync(
                        args[3], args[4]);
                    break;

                case dir:
                    if (args.Length != 6) Usage();
                    // source requests results answers reports
                    result = await PostRequestFolderAsync(
                        args[1], args[2], args[3], args[4], args[5]);
                    break;

                case report:
                    if (args.Length != 3) Usage();
                    // answer xml -> txt
                    result = await MakeReportAsync(
                        args[1], args[2]);
                    break;

                // Unknown
                default:
                    Usage();
                    break;
            }
        }
        catch (Exception e)
        {
            result = 1;
            Console.WriteLine("--- Error! ---");
            Console.WriteLine(e.Message);

            while (e.InnerException != null)
            {
                e = e.InnerException;
                Console.WriteLine(e.Message);
            }
        }

        Environment.Exit(result);
    }

    private static void Usage()
    {
        string usage = @"
Предоставление сведений о среднемесячных платежах (ССП)
субъектов кредитных историй:

    Api5704 запрос параметры

Запросы API:

dlput       – передача от БКИ данных, необходимых для формирования и
            предоставления пользователям кредитных историй сведений о
            среднемесячных платежах Субъекта.
dlrequest   – запрос сведений о среднемесячных платежах Субъекта.
 
    Параметры: request.xml result.xml

dlanswer    – получение сведений о среднемесячных платежах Субъекта по
            идентификатору ответа.
dlputanswer – получение информации о результатах загрузки данных,
            необходимых для формирования и предоставления пользователям
            кредитных историй сведений о среднемесячных платежах Субъекта,
            в базу данных КБКИ.

    Параметры: id answer.xml
               result.xml answer.xml

certadd     – добавление нового сертификата абонента.
certrevoke  – отзыв сертификата абонента.

    Параметры: id cert.cer sign.sig result.xml

Запросы расширенные:

auto        - запрос (dlrequest) и получение (dlanswer) за один запуск,
            создание текстового сводного отчета по полученным сведениям
            XML (report).

    Параметры: request.xml result.xml answer.xml report.txt

dir         - пакетная обработка запросов (auto) из папки.
            Это действие по умолчанию, если параметров не указано,
            но есть папка DirSource в конфиге.
            Подробнее см. в README.

    Параметры: source request result answer report

report      - создание текстового сводного отчета по полученным
            сведениям XML.

    Параметры: answer.xml report.txt";

        Console.WriteLine(usage);

        Environment.Exit(1);
    }
}
