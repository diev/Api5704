# Шаблоны для программы ReplForms

Templates for <https://github.com/diev/ReplForms>

Зеркала этой программы доступны на:
- <https://gitverse.ru/diev/ReplForms>
- <https://gitflic.ru/project/diev/replforms>

Replaces every `key[:value:remark:regexp]` in a template XML file with
DataGridView inserted and validated values.

Заменяет каждый `Параметр[:Значение:Примечание:RegExp]` в файле шаблона
(XML или другом) в наглядной сетке с опциональной проверкой введенных
полей на соответствие прилагаемям регулярным выражениям.

## Help / Помощь

Шаблон - это XML (или другой) файл,
где есть такие варианты специальных полей шаблона:

    `Параметр'
    `Параметр;Примечание'
    `Параметр;Значение;'
    `Параметр;Значение;Примечание'
    `Параметр;Значение;Примечание;RegExp'

Поле "Значение" (значение по умолчанию) может делать автозамену:
- `GUID`
- `YYYY-MM-DD`
