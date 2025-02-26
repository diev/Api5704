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

using System.Diagnostics;

namespace Api6775;

internal static class Exec
{
    /// <summary>
    /// Запустить программу с параметрами и дождаться ее завершения.
    /// </summary>
    /// <param name="exe">Запускаемая программа.</param>
    /// <param name="cmdline">Параметры для запускаемой программы.</param>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    public static async Task StartAsync(string exe, string cmdline)
    {
        if (!File.Exists(exe))
        {
            throw new FileNotFoundException("File to exec not found.", exe);
        }

        ProcessStartInfo startInfo = new()
        {
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal, // NO .Hidden with CryptoPro!!!
            UseShellExecute = true, // NO false with CryptoPro!!!
            FileName = exe,
            Arguments = cmdline
        };

        try
        {
            using Process? process = Process.Start(startInfo);

            if (process is null)
            {
                throw new Exception("Fail to get starting process.");
            }
            else
            {
                await process.WaitForExitAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Fail to start [\"{exe}\" {cmdline}]", ex);
        }
    }
}
