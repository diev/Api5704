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
/// Класс для работы с форматом PKCS#7.
/// </summary>
internal static class PKCS7
{
    /// <summary>
    /// Извлечь из PKCS#7 с ЭП чистый исходный текст.
    /// Криптопровайдер и проверка ЭП здесь не используются - только извлечение блока данных из формата ASN.1
    /// </summary>
    /// <param name="data">Массив байтов с сообщением в формате PKCS#7.</param>
    /// <returns>Массив байтов с исходным сообщением без ЭП.</returns>
    public static async Task<byte[]> CleanSign(byte[] data)
    {
        //using System.Security.Cryptography.Pkcs;

        //var signedCms = new SignedCms();
        //signedCms.Decode(data);

        //return signedCms.ContentInfo.Content;

        return await ASN1.CleanSignAsync(data);
    }

    /// <summary>
    /// Подписать файл с помощью.
    /// </summary>
    /// <param name="file">Имя исходного файла.</param>
    /// <param name="resultFile">Имя подписанного файла.</param>
    /// <exception cref="FileNotFoundException"></exception>
    public static async Task SignFileAsync(string file, string resultFile)
    {
        var config = Program.Config;

        // "C:\Program Files\Crypto Pro\CSP\csptest.exe"
        // -sfsign -sign -in %1 -out %2 -my %3 [-password %4] -add -addsigtime

        string cmdline = config.CspTestSignFile
            .Replace("%1", file)
            .Replace("%2", resultFile)
            .Replace("%3", config.MyThumbprint);

        await Exec.StartAsync(config.CspTest, cmdline);

        if (!File.Exists(resultFile))
        {
            throw new FileNotFoundException("Signed file not created. Token not found?", resultFile);
        }
    }
}
