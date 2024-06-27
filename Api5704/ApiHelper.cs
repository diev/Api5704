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

namespace Api5704;

public class ApiHelper
{
    public static (string, string) GetDirMask(string path)
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
    /// Разобрать ответ сервера, записать файл с текстом полученной квитанции и вернуть код результата запроса
    /// и содержимое квитанции.
    /// </summary>
    /// <param name="file">Имя файла для ответной квитанции.
    /// Если в конфиге CleanSign: true, то будут очищенный файл и файл.sig в исходном формате PKCS#7.</param>
    /// <param name="response">Комплексный ответ сервера.</param>
    /// <returns>Код результата запроса и содержимое квитанции, возвращенные сервером.</returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static async Task<(int, string)> WriteResultFileAsync(string file, HttpResponseMessage response, bool clean)
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
}
