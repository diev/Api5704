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
    /// <summary>
    /// Пакетная обработка папок с запросами (Extra).
    /// </summary>
    /// <param name="dir">Папка с исходными запросами.</param>
    /// <param name="requests">Папка с отправленными запросами.</param>
    /// <param name="results">Папка с полученными квитанциями.</param>
    /// <param name="answers">Папка с полученными сведениями.</param>
    public static async Task PostRequestFolderAsync(string dir, string requests, string results, string answers)
    {
        if (!requests.Equals(string.Empty)) Directory.CreateDirectory(requests);
        if (!results.Equals(string.Empty)) Directory.CreateDirectory(results);
        if (!answers.Equals(string.Empty)) Directory.CreateDirectory(answers);

        int count = 0;

        foreach (var file in Directory.GetFiles(dir, "*.xml"))
        {
            byte[] data = File.ReadAllBytes(file);

            if (data[0] == 0x30)
            {
                //data = PKCS7.CleanSign(data);
                data = await ASN1.CleanSignAsync(data);
            }

            XmlDocument doc = new();
            doc.LoadXml(Encoding.UTF8.GetString(data));
            string id = doc.DocumentElement!.GetAttribute("ИдентификаторЗапроса");

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string name = $"{Path.GetFileName(file)}.{date}.{id}";

            string request = Path.Combine(results, name + ".request.xml");
            string result = Path.Combine(results, name + ".result.xml");
            string answer = Path.Combine(answers, name + ".answer.xml");

            File.Copy(file, request, true);

            await Api.PostRequestAsync(Api.auto, request, result, answer);

            if (File.Exists(answer))
            {
                File.Delete(file);
                count++;
            }

            Thread.Sleep(1000);
        }

        Console.WriteLine($"Сведений получено: {count}.");

        Environment.Exit(0);
    }
}
