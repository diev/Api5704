# Api5704
[![Build status](https://ci.appveyor.com/api/projects/status/0rnoo3ra0mwyc327?svg=true)](https://ci.appveyor.com/project/diev/api5704)
[![.NET8 Desktop](https://github.com/diev/Api5704/actions/workflows/dotnet8-desktop.yml/badge.svg)](https://github.com/diev/Api5704/actions/workflows/dotnet8-desktop.yml)
[![.NET9 Desktop](https://github.com/diev/Api5704/actions/workflows/dotnet9-desktop.yml/badge.svg)](https://github.com/diev/Api5704/actions/workflows/dotnet9-desktop.yml)
[![GitHub Release](https://img.shields.io/github/release/diev/Api5704.svg)](https://github.com/diev/Api5704/releases/latest)

Передача сведений о среднемесячных платежах (ССП) и сведений о самозапрете
(снятии самозапрета) по Указанию Банка России от 11.01.2021 N 5704-У
«О порядке и форме предоставления сведений о среднемесячных платежах
субъекта кредитной истории, о порядке и форме запроса и предоставления
квалифицированным бюро кредитных историй сведений, необходимых для
подготовки сведений о среднемесячных платежах субъекта кредитной истории,
а также о порядке предоставления данных, необходимых для формирования и
предоставления пользователям кредитных историй сведений о среднемесячных
платежах субъекта кредитной истории».

## Порядок взаимодействия с использованием API

«Порядок взаимодействия пользователей кредитных историй, бюро кредитных
историй, в том числе квалифицированных бюро кредитных историй, с
квалифицированными бюро кредитных историй с использованием программного
интерфейса приложения (API) в целях предоставления сведений о
среднемесячных платежах» публикуется в соответствии с пунктом 1.2
Указания Банка России N 5704-У:

<https://cbr.ru/ckki/transfer_inform/>

По этой ссылке важно отслеживать вступление в силу новых версий форматов
и не путать версии документов и версии xsd.

* Версия 2.0 (с 01.03.2025)
* Версия 1.3 (с 10.07.2024 по 30.04.2025)

В обновленной версии 2.0 по сравнению с версией 1.3 произведены следующие
основные изменения:

1. изменено основание разработки Порядка (вступление в силу Указания Банка
России от 27.06.2024 N 6775-У «О порядке взаимодействия квалифицированных
бюро кредитных историй (в том числе между собой) с пользователями кредитных
историй, иными юридическими лицами и индивидуальными предпринимателями,
не являющимися пользователями кредитных историй, в целях предоставления
квалифицированными бюро кредитных историй сведений о среднемесячных платежах,
сведений о запрете (снятии запрета) на заключение договоров потребительского
займа (кредита), порядке и форме предоставления указанных сведений, порядке
и форме запроса и предоставления квалифицированным бюро кредитных историй
необходимых сведений для подготовки сведений о среднемесячных платежах,
сведений о запрете (снятии запрета) на заключение договоров потребительского
займа (кредита), а также о порядке предоставления бюро кредитных историй
в квалифицированные бюро кредитных историй данных, необходимых для
формирования и предоставления пользователям кредитных историй сведений
о среднемесячных платежах субъекта кредитной истории»);
2. изменено название Порядка — «Порядок взаимодействия пользователей
кредитных историй, бюро кредитных историй, в том числе квалифицированных
бюро кредитных историй, с квалифицированными бюро кредитных историй
с использованием программного интерфейса приложения (API) в целях
предоставления сведений о среднемесячных платежах субъектов кредитных
историй и (или) сведений о запрете (снятии запрета) на заключение
договоров потребительского займа (кредита)»;
3. скорректирован запрос сведений о среднемесячных платежах в части
дополнения возможностью запроса сведений о запрете (снятии запрета);
4. добавлена возможность пакетного запроса сведений (до 10 субъектов
в одном запросе).

Адрес базового URL API тестовой системы:
<https://reports.demo.nbki.ru/qbch/>

Адрес базового URL API тестовой системы, планируемой к выпуску:
<https://reports.test-alfa.nbki.ru/qbch/>

Адрес базового URL API версии 1.3 промышленной системы:
<https://ssp.nbki.ru/qbch/>

Адрес базового URL API версии 2.0 промышленной системы:
<https://ssp.nbki.ru/qbch/v2_0/>

Для подключения требуется зарегистрированный через ЛК сертификат
и зачастую дополнительная привязка через Службу поддержки.

## Подготовка запросов

Для небольшого использования по подготовке и учету сделанных запросов
прилагается файл Microsoft Excel с макросами `Api5704.xslm` в папке
[XSLM](XSLM).

Как альтернативу Excel можно рассмотреть другой проект по составлению
шаблонных XML-запросов - <https://github.com/diev/ReplForms>.
Для этой программы положен файл шаблона в папку [Templates](Templates).

Ныне запросы ССП могут быть только в режиме одного окна,
что определяется значением параметра `ТипЗапроса`:

* ТипЗапроса="`2`" – запрашивает одно окно КБКИ.

СНИЛС требуется указывать по формату `\d\d\d-\d\d\d-\d\d\d \d\d`.

ХэшКод требуется переводить в нижний регистр `[\da-f]{64}`.

## Config

При первом запуске будет создан файл настроек `Api5704.config.json`
(в папке с программой) с параметрами по умолчанию.
Откорректируйте его перед новым запуском:

* `MyThumbprint` = отпечаток вашего сертификата (прописанного в КБКИ
для подключения к их серверу), который должен быть в Хранилище
сертификатов и у вас должен быть ключ (и PIN, если назначен);
* `VerboseClient` = отображать содержимое вашего сертификата
(для наглядности);
* `ServerAddress` = url сервера для подключения;
* `ServerThumbprint` = отпечаток сертификата сервера (опционально);
* `ValidateTls` = проверять действительность сертификатов TLS
(по срокам действия, цепочки и т.п.);
* `ValidateThumbprint` = проверять отпечаток сервера (отключите,
если не знаете `ServerThumbprint`);
* `VerboseServer` = отображать содержимое сертификата сервера
(для наглядности);
* `UseProxy` = использовать прокси;
* `ProxyAddress` = url прокси-сервера (опционально);
* `SignFile` = подписывать запросы в программе (если СКЗИ для
подписывания и доступ в Интернет для обмена совмещены на одном АРМ);
* `CleanSign` = удалять подписи ответов в программе (на диске
будут сохранены оба файла - `.xml` и `.xml.sig`);
* `MaxRetries` = число попыток с предписанным интервалом в 1 сек.,
чтобы получить сведения по запросу;
* `DirSource` = папка и маска файлов с исходными запросами для пакетной
обработки (должна существовать, чтобы при запуске без параметров, файлы
брались оттуда). Пример: `OUT\*.xml` (не забывайте об удвоении `\\` в
файлах формата JSON);
* `DirRequest` = папка+файл для отправленных запросов, где
папка+файл - это путь к создаваемому файлу, где могут быть сделаны
автоподстановки (в любом порядке и количестве - и в имени папки, и в
имени файла):
  * `{name}` = имя исходного файла без расширения;
  * `{date}` = текущая дата в формате `ГГГГ-ММ-ДД`;
  * `{guid}` = ИдентификаторЗапроса из исходного XML;
* `DirResult` = папка+файл для полученных квитанций;
* `DirAnswer` = папка+файл для полученных сведений;
* `CspTest` = путь к программе КриптоПро `csptest.exe` (опционально);
* `CspTestSignFile` = команда с параметрами для подписи запросов в
программе, где:
  * `%1` = исходный файл XML;
  * `%2` = подписанный файл XML.sig для отправки;
  * `%3` = будет подставлено значение `MyThumbprint` для выбора
сертификата в Хранилище для подписи.

Пример рабочего конфига:

```json
{
  "MyThumbprint": "2756273e9e3c99ee435ffeaa79505b10214321c8",
  "VerboseClient": true,
  "ServerAddress": "https://ssp.nbki.ru/qbch/",
  "ServerThumbprint": "18042E6D06AE9F05B639DF511A8583FEDE72784D",
  "ValidateTls": true,
  "ValidateThumbprint": false,
  "VerboseServer": true,
  "UseProxy": false,
  "ProxyAddress": "http://192.168.2.1:3128",
  "SignFile": true,
  "CleanSign": true,
  "MaxRetries": 10,
  "DirSource": "OUT\\*.xml",
  "DirRequests": "Requests\\{name}.{date}.{guid}.request.xml",
  "DirResults": "Results\\{name}.{date}.{guid}.result.xml",
  "DirAnswers": "Answers\\{name}.{date}.{guid}.answer.xml",
  "CspTest": "C:\\Program Files\\Crypto Pro\\CSP\\csptest.exe",
  "CspTestSignFile": "-sfsign -sign -in %1 -out %2 -my %3 -add -addsigtime"
}
```

## Usage

Предоставление сведений о среднемесячных платежах субъектов
кредитных историй:

    Api5704 запрос параметры

Регистр командной строки неважен. Ниже запросы (команды) для
удобства указаны в верхнем регистре, а файлы - в нижнем.

Обычно программе нужен только первый файл (или guid), а последующие
в параметрах она создает с указанными именами и полученной информацией.

**dlput** – передача от КБКИ данных, необходимых для формирования
и предоставления пользователям кредитных историй сведений о
среднемесячных платежах Субъекта.

    Api5704 DLPUT qcb_put.xml result.xml

**dlrequest** – запрос сведений о среднемесячных платежах Субъекта.
Параметры: `request.xml[.sig] result.xml`
(`result.xml` будет создан с результатом операции).

    Api5704 DLREQUEST request.xml result.xml

**dlanswer** – получение сведений о среднемесячных платежах Субъекта
по идентификатору ответа.

    Api5704 DLANSWER n6c80c1c8-f620-491c-994a-6886706d85dc answer.xml
    Api5704 DLANSWER result.xml answer.xml

**dlputanswer** – получение информации о результатах загрузки данных,
необходимых для формирования и предоставления пользователям кредитных
историй сведений о среднемесячных платежах Субъекта, в базу данных КБКИ.
Параметры: `id answer.xml` (вместо `id` можно подставить `result.xml`
с ним из предыдущей операции, `answer.xml` будет создан с ответом).

    Api5704 DLPUTANSWER 945cb186-0d50-45ff-8823-797942987638 answer.xml
    Api5704 DLPUTANSWER result.xml answer.xml

**certadd** – добавление нового сертификата абонента.

    Api5704 CERTADD A6563526-A3F3-4D4E-A923-E41E93F1D921 cert.cer cert.cer.sig result.xml

**certrevoke** – отзыв сертификата абонента.
Параметры: `id cert.cer sign.sig result.xml`
(`result.xml` будет создан с результатом операции).

    Api5704 CERTREVOKE A6563526-A3F3-4D4E-A923-E41E93F1D921 cert.cer cert.cer.sig result.xml

## Пример получения ССП в конфигурации с наложением ЭП программой

Получить новый GUID (пусть в данном примере
`6d20a9fd-7bce-4480-bf56-a66932876bf7`).
Подготовить файл запроса `request.xml`, где будет этот
`ИдентификаторЗапроса="6d20a9fd-7bce-4480-bf56-a66932876bf7"`.
Отправить файл командой `dlrequest`:

    Api5704 DLREQUEST request.xml result.xml

Посмотреть полученный (в случае успеха передачи) файл `result.xml`.
Там будет строка вида (одной строкой) с ответом на наш запрос:

    <ИдентификаторОтвета
    ИдентификаторЗапроса="6d20a9fd-7bce-4480-bf56-a66932876bf7">
    b17c7a39-359e-4e7c-941d-668e2e957a7c
    </ИдентификаторОтвета>

Вот этот идентификатор ответа надо через некоторое время
отправить командой для получения ответного файла с ССП:

    Api5704 DLANSWER b17c7a39-359e-4e7c-941d-668e2e957a7c answer.xml

Другой вариант проще - запустить запрос с файлом из предыдущего этапа -
программа возьмет ИдентификаторОтвета из него сама:

    Api5704 DLANSWER result.xml answer.xml

Или еще проще - использовать расширение API (см.ниже).

Полученный файл `answer.xml` содержит искомую информацию с ССП.

## Расширение API дополнительными командами

Отправка запроса (`dlrequest`), получение квитанции и сведений
(`dlanswer`), создание текстового отчета (`report`) за один запуск -
команда `auto`:

    Api5704 AUTO request.xml result.xml answer.xml report.txt

Пакетная обработка запросов (`auto`) из папки за один запуск -
команда `dir` (это и действие по умолчанию, если параметров не указано
вовсе, но есть папка `DirSource` в конфиге, а также там указаны папки
`DirRequest`, `DirResult`, `DirAnswer`):

    Api5704 DIR source request result answer report
    Api5704

где:

- `source` - папка с исходными запросами `*.xml` (имена файлов любые -
рекомендуется использовать ФИО);
- `request` - папка, куда будут сложены копии исходных файлов,
переименованные по маске `ФИО.yyyy-MM-dd.guid.request.xml`:
  - `ФИО` - исходное имя файла (например, ФИО),
  - `yyyy-MM-dd` - текущая дата,
  - `guid` - ИдентификаторЗапроса из XML;
- `result` - папка, куда будут сложены полученные квитанции,
переименованные по аналогичной маске `ФИО.yyyy-MM-dd.guid.result.xml`;
- `answer` - папка, куда будут сложены полученные сведения,
переименованные по аналогичной маске `ФИО.yyyy-MM-dd.guid.answer.xml`.
- `report` - папка, куда будет положен сводный отчет по полученным сведениям,
переименованный по аналогичной маске `ФИО.yyyy-MM-dd.guid.answer.xml.txt`.

После получения файла в папке `answer`, соответствующий ему исходный
файл будет считаться обработанным и удален из папки `source`, при этом
он всегда может быть позже найден в папке `request` с датой и guid.

Также к полученному файлу `answer.xml` будет создан текстовый сводный
отчет в файле `answer.txt` рядом с ним.

Отдельно его можно получить командой:

    Api5704 REPORT answer.xml report.txt

## Вычисление ХэшКода согласий

Для вычисления ХэшКода служит утилита из состава КриптоПро:

    "C:\Program Files (x86)\Crypto Pro\CSP\cpverify.exe" -mk -alg GR3411_2012_256 file.pdf
    A36D628486A17D934BE027C9CAF79B27D7CD9E4E49469D97312B40AD6228D26F

Для удобства в Api5704.xlsm добавлены макросы:

- `CalcHash` - указать файл PDF в диалоге, рассчитать (требуется
установленный КриптоПро) и скопипастить из окна ввода.
- `ReadHash` - указать готовый файл TXT в диалоге и скопипастить из
окна ввода.

Также добавлен `hash.cmd`, который надо закинуть в папку с PDF, и он
посчитает и запишет к каждому PDF рядом файл `.txt`, с кодом уже в
нижнем регистре.

## Проверка ХэшКода на сервисе Госуслуг

<https://www.gosuslugi.ru/pgu/eds>

## Requirements

* .NET 8
* .NET 9
* CryptoPro CSP
* Microsoft Excel

## Versioning

Номер версии программы указывается по нарастающему принципу и строится
от максимальной протестированной версии .NET на момент разработки и даты
редакции:

* Актуальная версия .NET (9);
* Год текущей разработки (2024);
* Месяц без первого нуля и день редакции (624 - 24.06.2024);
* Номер билда, если указан - просто нарастающее число для внутренних отличий.

Продукт развивается для собственных нужд, а не по коробочной
стратегии, и поэтому *Breaking Changes* могут случаться чаще,
чем это принято в *SemVer*. Поэтому проще по датам актуализации кода.

При обновлении программы рекомендуется сохранить предыдущий конфиг,
удалить его из папки с программой, чтобы она создала новый, перенести
необходимые старые значения в новый конфиг перед новым запуском
программы.

## License

Licensed under the [Apache License, Version 2.0](LICENSE).
Вы можете использовать эти материалы под свою ответственность.

[![Telegram](https://img.shields.io/badge/t.me-dievdo-blue?logo=telegram)](https://t.me/dievdo)
