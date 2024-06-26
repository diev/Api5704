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
    /// <param name="requests">Папка с готовыми запросами.</param>
    /// <param name="results">Папка с готовыми результатами.</param>
    /// <param name="answers">Папка с готовыми ответами.</param>
    public static async Task PostRequestFolderAsync(string dir, string requests, string results, string answers)
    {
        Directory.CreateDirectory(requests);
        Directory.CreateDirectory(results);
        Directory.CreateDirectory(answers);

        foreach (var file in Directory.GetFiles(dir, "*.xml"))
        {
            byte[] data = File.ReadAllBytes(file);

            if (data[0] == 0x30)
            {
                data = PKCS7.CleanSign(data);
            }

            XmlDocument doc = new();
            doc.LoadXml(Encoding.UTF8.GetString(data));
            string id = doc.DocumentElement!.GetAttribute("ИдентификаторЗапроса");

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string name = $"{date}.{id}";

            string request = Path.Combine(results, name + ".request.xml");
            string result = Path.Combine(results, name + ".result.xml");
            string answer = Path.Combine(answers, name + ".answer.xml");

            File.Copy(file, request, true);

            await Api.PostRequestAsync(Api.auto, request, result, answer);

            if (File.Exists(answer))
            {
                File.Delete(file);
            }

            Thread.Sleep(1000);
        }
    }
}
