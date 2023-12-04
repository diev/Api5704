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

using System.Text.Json;

namespace Api5704;

/// <summary>
/// Класс для работы с секцией "configProperties": {...} в файле конфигурации .runtimeconfig.json
/// </summary>
internal static class ConfigReader
{
    //TODO: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration

    private const string _noParam = " - no parameter in config!";
    private const string _invalidParam = " - invalid parameter in config!";

    /// <summary>
    /// Получение значения строки из "name": "value" (значение должно быть!)
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Значение строки.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetString(string name)
    {
        return AppContext.GetData(name) as string
            ?? throw new ArgumentNullException(name + _noParam);
    }

    /// <summary>
    /// Получение значения строки из "name": "value"
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Значение строки или NULL.</returns>
    public static string? GetNullString(string name)
    {
        return AppContext.GetData(name) as string;
    }

    /// <summary>
    /// Получение логического значения из "name": true (значение должно быть!)
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Логическое значение.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static bool GetBool(string name)
    {
        var value = AppContext.GetData(name)
            ?? throw new ArgumentNullException(name + _noParam);

        if (value is bool b)
            return b;

        if (bool.TryParse((string)value, out bool result))
            return result;

        throw new ArgumentException(name + _invalidParam);
    }

    /// <summary>
    /// Получение значения строки из "name": true или NULL.
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Логическое значение.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool? GetNullBool(string name)
    {
        var value = AppContext.GetData(name);

        if (value is null)
            return null;

        if (value is bool b)
            return b;

        if (bool.TryParse((string)value, out bool result))
            return result;

        throw new ArgumentException(name + _invalidParam);
    }

    /// <summary>
    /// Получение целочисленного значения из "name": 1 (значение должно быть!)
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Целочисленноге значение.<./returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static int GetInt(string name)
    {
        var value = AppContext.GetData(name)
            ?? throw new ArgumentNullException(name + _noParam);

        if (value is int i)
            return i;

        if (int.TryParse((string)value, out int result))
            return result;

        throw new ArgumentException(name + _invalidParam);
    }

    /// <summary>
    /// Получение целочисленного значения из "name": 1 или NULL.
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Целочисленноге значение или NULL.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static int? GetNullInt(string name)
    {
        var value = AppContext.GetData(name);

        if (value is null)
            return null;

        if (value is int i)
            return i;

        if (int.TryParse((string)value, out int result))
            return result;

        throw new ArgumentException(name + _invalidParam);
    }

    /// <summary>
    /// Получение массива строк из "name": ["value", ...]
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Массив строк.</returns>
    public static string[] GetStrings(string name)
    {
        var value = AppContext.GetData(name);

        if (value is null)
            return [];

        if (value is string s)
        {
            var values = JsonSerializer.Deserialize<string[]>(s);

            if (values is null)
                return [];

            return values;
        }

        return (string[])value;
    }

    /// <summary>
    /// Получение массива чисел из "name": [1, ...]
    /// </summary>
    /// <param name="name">Параметр.</param>
    /// <returns>Массив чисел.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static int[] GetInts(string name)
    {
        var value = AppContext.GetData(name);

        if (value is null)
            return [];

        if (value is string s)
        {
            var values = JsonSerializer.Deserialize<string[]>(s);

            if (values is null)
                return [];

            int[] arr = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                if (int.TryParse(values[i], out int result))
                    arr[i] = result;
                else
                    throw new ArgumentException(name + _invalidParam);
            }

            return arr;
        }

        return (int[])value;
    }
}
