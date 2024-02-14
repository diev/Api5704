Attribute VB_Name = "RequestModule"
Option Explicit

Const INN As String = "7831001422"
Const OGRN As String = "1027800000095"
Const BANK As String = "Акционерное общество ""Банк"""

Public Sub CreateDlRequest()
    Dim XDoc As Object, xmlVersion As Object, root As Object
    Dim abonent As Object, person As Object, elem As Object
    Dim request As Object, src As Object, doc As Object
    Dim i As Integer
    
    i = 2
    '--Запрос--
    
    'Идентификатор
    Dim iz As String: iz = Cells(ActiveCell.Row, i).Text
    If Len(iz) = 0 Then
        iz = CreateGuidStr()
        Cells(ActiveCell.Row, i) = iz
    End If
    i = i + 1

    'Тип
    Dim tz As String: tz = Cells(ActiveCell.Row, i).Text
    If Len(tz) = 0 Then
        'tz = "2"
        'Cells(ActiveCell.Row, i) = tz
    End If
    i = i + 1
    
    'Дата
    Dim dz As String: dz = Cells(ActiveCell.Row, i).Text
    If Len(dz) = 0 Then
        dz = Format(Now, "yyyy-MM-dd")
        Cells(ActiveCell.Row, i) = Now
    Else
        dz = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd")
    End If
    i = i + 1
    
    'Цель
    Dim cz As String: cz = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Описание иной цели (99)
    Dim ocz As String: ocz = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Валюта
    Dim vz As String: vz = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Сумма
    Dim sz As String: sz = Replace(Cells(ActiveCell.Row, i).Text, " ", ""): i = i + 1
    
    '--Субъект--
    
    'Фамилия
    Dim fioF As String: fioF = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Имя
    Dim fioI As String: fioI = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Отчество
    Dim fioO As String: fioO = Cells(ActiveCell.Row, i).Text: i = i + 1
    
    'Фамилия2
    Dim fio2F As String: fio2F = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Имя2
    Dim fio2I As String: fio2I = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Отчество2
    Dim fio2O As String: fio2O = Cells(ActiveCell.Row, i).Text: i = i + 1
    
    'Дата рождения
    Dim dr As String: dr = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    'Место рождения
    Dim mr As String: mr = Cells(ActiveCell.Row, i).Text: i = i + 1
    
    'Документ
    Dim id As String: id = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Наименование иного документа (14)
    Dim td As String: td = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Серия
    Dim sd As String: sd = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Номер
    Dim nd As String: nd = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Дата выдачи
    Dim vd As String: vd = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    'Орган
    Dim od As String: od = Cells(ActiveCell.Row, i).Text: i = i + 1
    'КП
    Dim kd As String: kd = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Срок действия
    Dim ed As String: ed = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    
    'Документ2
    Dim id2 As String: id2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Наименование иного документа (14)
    Dim td2 As String: td2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Серия2
    Dim sd2 As String: sd2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Номер2
    Dim nd2 As String: nd2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Дата выдачи2
    Dim vd2 As String: vd2 = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    'Орган2
    Dim od2 As String: od2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'КП2
    Dim kd2 As String: kd2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Срок действия2
    Dim ed2 As String: ed2 = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    
    'ИНН
    Dim inn2 As String: inn2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    'СНИЛС
    Dim snils2 As String: snils2 = Cells(ActiveCell.Row, i).Text: i = i + 1
    
    '--Согласие--
    
    'Выдано
    Dim ds As String: ds = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    'Срок
    Dim ss As String: ss = Cells(ActiveCell.Row, i).Text: i = i + 1
    
    'Основание 'TODO?? (у нас нет переданных Согласий?)
    'Dim os As String: os = Cells(ActiveCell.Row, i).Text: i = i + 1
    
    'Список Целей
    Dim cs As String: cs = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Описание иной цели (99)
    Dim ocs As String: ocs = Cells(ActiveCell.Row, i).Text: i = i + 1
    'Договор если срок 3
    Dim dog As String: dog = Format(Cells(ActiveCell.Row, i), "yyyy-MM-dd"): i = i + 1
    'ХэшКод
    Dim hash As String: hash = Cells(ActiveCell.Row, i).Text: i = i + 1 'TODO?? Как его вычислять? Нигде не указано!
    
    '--XML--
    
    Set XDoc = CreateObject("MSXML2.DOMDocument")
    Set xmlVersion = XDoc.createProcessingInstruction("xml", "version=""1.0"" encoding=""UTF-8""")
    XDoc.appendChild xmlVersion

    'ЗапросСведенийОПлатежах
    Set root = XDoc.createElement("ЗапросСведенийОПлатежах")
    XDoc.appendChild root
    root.setAttribute "Версия", "1.1" 'const
    root.setAttribute "ИдентификаторЗапроса", iz
    '1 – абонент запрашивает сведения только у запрашиваемого КБКИ;
    '2 – абонент запрашивает сведения всех КБКИ путем обращения в одно КБКИ (режим «одно окно»).
    root.setAttribute "ТипЗапроса", tz
    
    'Абонент
    Set abonent = XDoc.createElement("Абонент")
    
        'ЮридическоеЛицо
        Set person = XDoc.createElement("ЮридическоеЛицо")
    
            'ИНН
            Set elem = XDoc.createElement("ИНН")
            elem.Text = INN 'const
            person.appendChild elem
    
            'ОГРН
            Set elem = XDoc.createElement("ОГРН")
            elem.Text = OGRN 'const
            person.appendChild elem
    
        '/ЮридическоеЛицо
        abonent.appendChild person
        
    '/Абонент
    root.appendChild abonent
    
    'Запрос
    Set request = XDoc.createElement("Запрос")
    
        'Источник
        Set src = XDoc.createElement("Источник")
        
            'ЮридическоеЛицо
            Set person = XDoc.createElement("ЮридическоеЛицо")
    
                'ИНН
                Set elem = XDoc.createElement("ИНН")
                elem.Text = INN 'const
                person.appendChild elem
    
                'ОГРН
                Set elem = XDoc.createElement("ОГРН")
                elem.Text = OGRN 'const
                person.appendChild elem
    
                'ПолноеНаименование
                Set elem = XDoc.createElement("ПолноеНаименование")
                elem.Text = BANK 'const
                person.appendChild elem
    
            '/ЮридическоеЛицо
            src.appendChild person
            
        '/Источник
        request.appendChild src
    
        'Субъект
        Set abonent = XDoc.createElement("Субъект")
            'ФИО
            Set person = XDoc.createElement("ФИО")
    
                'Фамилия
                Set elem = XDoc.createElement("Фамилия")
                elem.Text = fioF
                person.appendChild elem
    
                'Имя
                Set elem = XDoc.createElement("Имя")
                elem.Text = fioI
                person.appendChild elem
    
                If Len(fioO) > 0 Then 'optional
                    'Отчество
                    Set elem = XDoc.createElement("Отчество")
                    elem.Text = fioO
                    person.appendChild elem
                End If
    
            '/ФИО
            abonent.appendChild person
    
            If Len(fio2F) + Len(fio2I) + Len(fio2O) > 0 Then 'optional
                'ФИО предыдущие
                Set person = XDoc.createElement("ФИО")
                
                    'Фамилия
                    Set elem = XDoc.createElement("Фамилия")
                    elem.Text = fio2F
                    person.appendChild elem
        
                    'Имя
                    Set elem = XDoc.createElement("Имя")
                    elem.Text = fio2I
                    person.appendChild elem
        
                    If Len(fio2O) > 0 Then 'optional
                        'Отчество
                        Set elem = XDoc.createElement("Отчество")
                        elem.Text = fio2O
                        person.appendChild elem
                    End If
        
                '/ФИО предыдущие
                abonent.appendChild person
            End If
    
            'ДатаРождения
            Set elem = XDoc.createElement("ДатаРождения")
            elem.Text = dr
            abonent.appendChild elem
    
            If Len(mr) > 0 Then 'optional
                'МестоРождения
                Set elem = XDoc.createElement("МестоРождения")
                elem.Text = mr
                abonent.appendChild elem
            End If
    
            'ДокументЛичности
            Set doc = XDoc.createElement("ДокументЛичности")
            
                If Len(sd) > 0 Then 'optional
                    'Серия
                    Set elem = XDoc.createElement("Серия")
                    elem.Text = sd
                    doc.appendChild elem
                End If
                
                'Номер
                Set elem = XDoc.createElement("Номер")
                elem.Text = nd
                doc.appendChild elem
                
                'ДатаВыдачи
                Set elem = XDoc.createElement("ДатаВыдачи")
                elem.Text = vd
                doc.appendChild elem
                
                'НаименованиеОргана
                Set elem = XDoc.createElement("НаименованиеОргана")
                elem.Text = od
                doc.appendChild elem
    
                If Len(kd) > 0 Then 'optional
                    'КодПодразделения
                    Set elem = XDoc.createElement("КодПодразделения")
                    elem.Text = kd
                    doc.appendChild elem
                End If
            
                If Len(ed) > 0 Then 'optional
                    'СрокДействия
                    Set elem = XDoc.createElement("СрокДействия")
                    elem.Text = ed
                    doc.appendChild elem
                End If
            
            '/ДокументЛичности
            abonent.appendChild doc
            
            'ВидДУЛ:
            '14 Иной документ, выдаваемый уполномоченным органом (??)
            '21 Паспорт гражданина Российской Федерации
            '22.1 Паспорт гражданина Российской Федерации, удостоверяющий его личность
            'за пределами территории Российской Федерации
            '22.2 Дипломатический паспорт, удостоверяющий личность гражданина
            'Российской Федерации за пределами территории Российской Федерации
            '22.3 Служебный паспорт, удостоверяющий личность гражданина Российской
            'Федерации за пределами территории Российской Федерации
            '23 Удостоверение личности моряка
            '24 Удостоверение личности военнослужащего
            '25 Военный билет военнослужащего
            '26 Временное удостоверение личности гражданина Российской Федерации,
            'выдаваемое на период оформления паспорта гражданина Российской
            'Федерации
            '27 Свидетельство о рождении гражданина Российской Федерации
            '28 Иной ДУЛ гражданина Российской Федерации в соответствии с
            'законодательством Российской Федерации
            '31 Паспорт иностранного гражданина либо иной документ, установленный
            'федеральным законом или признаваемый в соответствии с международным
            'договором Российской Федерации в качестве документа, удостоверяющего
            'личность иностранного гражданина
            '32 Документ, выданный иностранным государством и признаваемый в
            'соответствии с международным договором Российской Федерации в качестве
            'документа, удостоверяющего личность лица без гражданства
            '35 Иной документ, признаваемый документом, удостоверяющим личность лица
            'без гражданства в соответствии с законодательством Российской Федерации и
            'международным договором Российской Федерации
            '37 Удостоверение беженца
            '38 Удостоверение вынужденного переселенца
            '999 Иной документ
            doc.setAttribute "ВидДУЛ", id
            If id = "14" Then doc.setAttribute "НаименованиеДУЛ", td 'TODO?? 999
    
            If Len(id2) + Len(sd2) + Len(nd2) + Len(vd2) + Len(od2) > 0 Then 'optional
                'ДокументЛичности предыдущие
                Set doc = XDoc.createElement("ДокументЛичности")
                
                    If Len(sd2) > 0 Then 'optional
                        'Серия
                        Set elem = XDoc.createElement("Серия")
                        elem.Text = sd2
                        doc.appendChild elem
                    End If
                    
                    'Номер
                    Set elem = XDoc.createElement("Номер")
                    elem.Text = nd2
                    doc.appendChild elem
                    
                    'ДатаВыдачи
                    Set elem = XDoc.createElement("ДатаВыдачи")
                    elem.Text = vd2
                    doc.appendChild elem
                    
                    'НаименованиеОргана
                    Set elem = XDoc.createElement("НаименованиеОргана")
                    elem.Text = od2
                    doc.appendChild elem
    
                    If Len(kd2) > 0 Then 'optional
                        'КодПодразделения
                        Set elem = XDoc.createElement("КодПодразделения")
                        elem.Text = kd2
                        doc.appendChild elem
                    End If
        
                    If Len(ed2) > 0 Then 'optional
                        'СрокДействия
                        Set elem = XDoc.createElement("СрокДействия")
                        elem.Text = ed2
                        doc.appendChild elem
                    End If
                
                '/ДокументЛичности предыдущие
                abonent.appendChild doc
                doc.setAttribute "ВидДУЛ", id2
                If id2 = "14" Then doc.setAttribute "НаименованиеДУЛ", td2 'TODO?? 999
            End If
    
            If Len(inn2) > 0 Then 'optional
                'ИНН
                Set elem = XDoc.createElement("ИНН")
                elem.Text = inn2
                abonent.appendChild elem
            End If
    
            If Len(snils2) > 0 Then 'optional
                'СНИЛС
                Set elem = XDoc.createElement("СНИЛС")
                elem.Text = snils2
                abonent.appendChild elem
            End If
    
        '/Субъект
        request.appendChild abonent
    
        'Согласие
        Set doc = XDoc.createElement("Согласие")
        
            'Выдано
            Set src = XDoc.createElement("Выдано")
            
                'ЮридическоеЛицо
                Set person = XDoc.createElement("ЮридическоеЛицо")
                
                    'ИНН
                    Set elem = XDoc.createElement("ИНН")
                    elem.Text = INN 'const
                    person.appendChild elem
                    
                    'ОГРН
                    Set elem = XDoc.createElement("ОГРН")
                    elem.Text = OGRN 'const
                    person.appendChild elem
                    
                    'ПолноеНаименование
                    Set elem = XDoc.createElement("ПолноеНаименование")
                    elem.Text = BANK 'const
                    person.appendChild elem
                
                '/ЮридическоеЛицо
                src.appendChild person
                
            '/Выдано
            doc.appendChild src
        
            'Цели согласия:
            'Коды целей выдачи согласия Субъектом пользователю кредитной истории:
            'Заключение договора с потребителем:
            '1 Потребительский заем (кредит) на приобретение автомобиля
            '2 Потребительский микрозаем
            '3 Потребительский заем (кредит) нецелевой
            '4 Потребительский заем (кредит) с расходным лимитом (кредитная линия,
            'овердрафт)
            '5 Иной потребительский заем (кредит)
            '6 Поручительство гражданина-потребителя
            '7 Ипотека, предоставленная гражданином-потребителем
            '8 Иной залог, предоставленный гражданином-потребителем
            '9 Иной потребительский договор
            'Совершение сделки, за исключением договора с потребителем:
            '10 Заем (кредит) на развитие бизнеса
            '11 Заем (кредит) на пополнение оборотных средств
            '12 Заем (кредит) на покупку оборудования
            '13 Заем (кредит) на строительство
            '14 Заем (кредит) на приобретение ценных бумаг
            '15 Иной заем (кредит)
            '16 Лизинг
            '17 Независимая гарантия
            '18 Поручительство
            '19 Страхование
            '20 Ипотека
            '21 Иной залог
            '22 Иной договор
            'Иные цели выдачи согласия Субъектом:
            '23 Кредитный мониторинг в рамках действующего договора
            '24 Прием на работу
            '25 Маркетинговые исследования
            '26 Научные исследования
            '27 Контроль данных
            '99 Иное (требуется заполнить поле Описание)
            Dim A() As String: A = Split(cs, ",")

            For i = LBound(A) To UBound(A)
                Set elem = XDoc.createElement("Цель")
                doc.appendChild elem
                elem.setAttribute "КодЦели", A(i)
                
                If A(i) = "99" Then elem.setAttribute "Описание", ocs
            Next
        
            'Договор
            If ss = "3" Then 'СрокДействия = 3
                Set elem = XDoc.createElement("Договор")
                doc.appendChild elem
                elem.setAttribute "Дата", dog
            End If
        
            'ХэшКод для согласия Субъекта, предусмотренный
            'Указанием Банка России от 11 мая 2021 года № 5791-У «О требованиях к составу и формату
            'запроса о предоставлении кредитного отчета, правилах поиска бюро кредитных историй
            'информации о субъекте кредитной истории и форме подтверждения наличия согласия
            'субъекта кредитной истории», зарегистрированным Министерством юстиции Российской
            'Федерации 15 июня 2021 года № 63883.
            Set elem = XDoc.createElement("ХэшКод")
            elem.Text = hash
            doc.appendChild elem
        
        '/Согласие
        request.appendChild doc
        doc.setAttribute "ДатаВыдачи", ds
        
        'СрокДействия:
        '1 Согласие действительно в течение шести месяцев со дня его оформления
        '2 Согласие действительно в течение года со дня его оформления
        '3 В течение срока действия согласия с субъектом кредитной истории были
        'заключены договор займа (кредита), договор лизинга, договор залога, договор
        'поручительства, выдана независимая гарантия
        doc.setAttribute "СрокДействия", ss
        
        'Основание передачи в случае отличия сведений о пользователе
        'кредитной истории, запрашивающем сведения о среднемесячных платежах Субъекта,
        'от сведений в блоке «Выдано» кодом основания передачи согласия:
        '1 Согласие Субъекта передано правопреемнику по заключенному договору
        'займа (кредита) или иному договору, информация об обязательствах по
        'которым передается в БКИ
        '2 Согласие Субъекта передано кредитной организации, осуществляющей
        'обслуживание денежных требований по договору займа (кредита),
        'уступленных специализированному финансовому обществу или ипотечному
        'агенту
        'doc.setAttribute "ОснованиеПередачи", os 'TODO??
        
        'Подтверждение ознакомления абонента с ответственностью за незаконные
        'действия по получению и (или) распространению информации, составляющей кредитную
        'историю, незаконное получение кредитного отчета, предусмотренной статьями 5.53 и 14.29
        'Кодекса Российской Федерации об административных правонарушениях
        doc.setAttribute "ОбОтветственностиПредупрежден", "1" 'const
    
        'Цель запроса
        Set elem = XDoc.createElement("Цель")
        request.appendChild elem
        elem.setAttribute "КодЦели", cz
        If cz = "99" Then elem.setAttribute "Описание", ocz
        
        If Len(sz) > 0 Then
            'СуммаОбязательства
            Set elem = XDoc.createElement("СуммаОбязательства")
            elem.Text = sz
            request.appendChild elem
            elem.setAttribute "Валюта", vz
        End If
        
    '/Запрос
    root.appendChild request
    request.setAttribute "Дата", dz
    
    '/ЗапросСведенийОПлатежах
    XDoc.Save "C:\TEMP\Request." & dz & "." & iz & ".xml"
End Sub
