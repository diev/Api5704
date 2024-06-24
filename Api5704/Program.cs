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

using System.Text.Json;

using static Api5704.Api;

namespace Api5704;

internal class Program
{
    public static Config Config { get; private set; } = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!"); // :)

        if (!TryGetConfig())
        {
            Environment.Exit(2);
        }

        if (args.Length == 0) Usage();
        string cmd = args[0].ToLower();

        try
        {
            switch (cmd)
            {
                case certadd:
                case certrevoke:
                    if (args.Length != 5) Usage();
                    await PostCertAsync(cmd, args[1], args[2], args[3], args[4]);
                    break;

                case dlput:
                case dlrequest:
                    if (args.Length != 3) Usage();
                    await PostRequestAsync(cmd, args[1], args[2]);
                    break;

                case dlanswer:
                case dlputanswer:
                    if (args.Length != 3) Usage();
                    await GetAnswerAsync(cmd, args[1], args[2]);
                    break;

                default:
                    Usage();
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("--- Error! ---");
            Console.WriteLine(e.Message);

            while (e.InnerException != null)
            {
                e = e.InnerException;
                Console.WriteLine(e.Message);
            }
        }

        Environment.Exit(2);
    }

    private static void Usage()
    {
        string usage = @"
Предоставление сведений о среднемесячных платежах субъектов кредитных историй:
    Api5704 запрос параметры

Запросы:

dlput – передача от БКИ данных, необходимых для формирования и предоставления
    пользователям кредитных историй сведений о среднемесячных платежах Субъекта.
dlrequest – запрос сведений о среднемесячных платежах Субъекта.
 
    Параметры: request.xml result.xml

dlanswer – получение сведений о среднемесячных платежах Субъекта по
    идентификатору ответа.
dlputanswer – получение информации о результатах загрузки данных, необходимых
    для формирования и предоставления пользователям кредитных историй сведений
    о среднемесячных платежах Субъекта, в базу данных КБКИ.

    Параметры: id answer.xml
               result.xml answer.xml

certadd – добавление нового сертификата абонента.
certrevoke – отзыв сертификата абонента.

    Параметры: id cert.cer sign.sig result.xml";

        Console.WriteLine(usage);

        Environment.Exit(1);
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new()
        {
            WriteIndented = true
        };
    }

    private static bool TryGetConfig()
    {
        string appsettings = Path.ChangeExtension(Environment.ProcessPath!, ".config.json");

        if (File.Exists(appsettings))
        {
            using var stream = File.OpenRead(appsettings);
            Config = JsonSerializer.Deserialize<Config>(stream)!;
            return true;
        }
        else
        {
            using var stream = File.OpenWrite(appsettings);
            JsonSerializer.Serialize(stream, Config, GetJsonOptions());

            Console.WriteLine($"Создан новый файл настроек '{appsettings}' - откорректируйте его.");
            return false;
        }
    }
}
