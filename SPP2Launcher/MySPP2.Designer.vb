﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Этот код создан программой.
'     Исполняемая версия:4.0.30319.42000
'
'     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
'     повторной генерации кода.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'Этот класс создан автоматически классом StronglyTypedResourceBuilder
    'с помощью такого средства, как ResGen или Visual Studio.
    'Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    'с параметром /str или перестройте свой проект VS.
    '''<summary>
    '''  Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Class MySPP2
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("DevCake.WoW.SPP2Launcher.MySPP2", GetType(MySPP2).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Перезаписывает свойство CurrentUICulture текущего потока для всех
        '''  обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Остановка сервера Apache..
        '''</summary>
        Friend Shared ReadOnly Property Apache001_Shutdown() As String
            Get
                Return ResourceManager.GetString("Apache001_Shutdown", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Запуск сервера Apache..
        '''</summary>
        Friend Shared ReadOnly Property Apache002_Start() As String
            Get
                Return ResourceManager.GetString("Apache002_Start", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Сервер Apache успешно запущен..
        '''</summary>
        Friend Shared ReadOnly Property Apache003_Started() As String
            Get
                Return ResourceManager.GetString("Apache003_Started", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Сервер Apache успешно остановлен..
        '''</summary>
        Friend Shared ReadOnly Property Apache004_Stopped() As String
            Get
                Return ResourceManager.GetString("Apache004_Stopped", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Не удалось запустить сервер Apache..
        '''</summary>
        Friend Shared ReadOnly Property Apache005_NotStarted() As String
            Get
                Return ResourceManager.GetString("Apache005_NotStarted", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Каталог {0} не найден!.
        '''</summary>
        Friend Shared ReadOnly Property E001_DirNotFound() As String
            Get
                Return ResourceManager.GetString("E001_DirNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Single Player Project v2 уже запущен!.
        '''</summary>
        Friend Shared ReadOnly Property E002_AlreadyRunning() As String
            Get
                Return ResourceManager.GetString("E002_AlreadyRunning", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Ошибка!.
        '''</summary>
        Friend Shared ReadOnly Property E003_ErrorCaption() As String
            Get
                Return ResourceManager.GetString("E003_ErrorCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Не найдено ни одного модуля сервера WoW!.
        '''</summary>
        Friend Shared ReadOnly Property E004_ModulesNotFound() As String
            Get
                Return ResourceManager.GetString("E004_ModulesNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Не найден каталог расположения сервера MySQL!.
        '''</summary>
        Friend Shared ReadOnly Property E005_MySqlNotFound() As String
            Get
                Return ResourceManager.GetString("E005_MySqlNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Не найден каталог расположения сервера Apache!.
        '''</summary>
        Friend Shared ReadOnly Property E006_ApacheNotFound() As String
            Get
                Return ResourceManager.GetString("E006_ApacheNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Не найдены исполняемые файлы CMaNGOS.
        '''</summary>
        Friend Shared ReadOnly Property E007_MangoNotFound() As String
            Get
                Return ResourceManager.GetString("E007_MangoNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Неизвестный модуль расширения {0}.
        '''</summary>
        Friend Shared ReadOnly Property E008_UnknownModule() As String
            Get
                Return ResourceManager.GetString("E008_UnknownModule", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на MySQL выдал исключение:.
        '''</summary>
        Friend Shared ReadOnly Property E009_MySqlException() As String
            Get
                Return ResourceManager.GetString("E009_MySqlException", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Apache выдал исключение:.
        '''</summary>
        Friend Shared ReadOnly Property E010_ApacheException() As String
            Get
                Return ResourceManager.GetString("E010_ApacheException", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Инициализация базовых настроек приложения выполнена..
        '''</summary>
        Friend Shared ReadOnly Property P001_BaseInit() As String
            Get
                Return ResourceManager.GetString("P001_BaseInit", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Добавлен серверный модуль {0}.
        '''</summary>
        Friend Shared ReadOnly Property P002_ModuleAdded() As String
            Get
                Return ResourceManager.GetString("P002_ModuleAdded", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Файлы exe (*.exe)|*.exe|Все файлы (*.*)|*.*.
        '''</summary>
        Friend Shared ReadOnly Property P003_SetWowClientPath() As String
            Get
                Return ResourceManager.GetString("P003_SetWowClientPath", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Выберите сервер.
        '''</summary>
        Friend Shared ReadOnly Property P004_SelectServer() As String
            Get
                Return ResourceManager.GetString("P004_SelectServer", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Выход из приложения..
        '''</summary>
        Friend Shared ReadOnly Property P005_Exiting() As String
            Get
                Return ResourceManager.GetString("P005_Exiting", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Изменения произойдут после перезагрузки..
        '''</summary>
        Friend Shared ReadOnly Property P006_NeedReboot() As String
            Get
                Return ResourceManager.GetString("P006_NeedReboot", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Сообщение.
        '''</summary>
        Friend Shared ReadOnly Property P007_MessageCaption() As String
            Get
                Return ResourceManager.GetString("P007_MessageCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на установлен.
        '''</summary>
        Friend Shared ReadOnly Property P008_Installed() As String
            Get
                Return ResourceManager.GetString("P008_Installed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на не установлен.
        '''</summary>
        Friend Shared ReadOnly Property P009_NotInstalled() As String
            Get
                Return ResourceManager.GetString("P009_NotInstalled", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Single Player Project v2.
        '''</summary>
        Friend Shared ReadOnly Property P010_LauncherCaption() As String
            Get
                Return ResourceManager.GetString("P010_LauncherCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Настройки лаунчера.
        '''</summary>
        Friend Shared ReadOnly Property P011_LauncherSettingsCaption() As String
            Get
                Return ResourceManager.GetString("P011_LauncherSettingsCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Параметры.
        '''</summary>
        Friend Shared ReadOnly Property P012_Parameters() As String
            Get
                Return ResourceManager.GetString("P012_Parameters", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Включить сайт.
        '''</summary>
        Friend Shared ReadOnly Property P013_SiteAutostart() As String
            Get
                Return ResourceManager.GetString("P013_SiteAutostart", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Автоматический запуск сервера.
        '''</summary>
        Friend Shared ReadOnly Property P014_ServerAutostart() As String
            Get
                Return ResourceManager.GetString("P014_ServerAutostart", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Запускать MySQL вне зависимости от сервера.
        '''</summary>
        Friend Shared ReadOnly Property P015_MySqlAutostart() As String
            Get
                Return ResourceManager.GetString("P015_MySqlAutostart", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Настройки MySQL.
        '''</summary>
        Friend Shared ReadOnly Property P016_MySqlSettingsCaption() As String
            Get
                Return ResourceManager.GetString("P016_MySqlSettingsCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Использовать встроенный сервер.
        '''</summary>
        Friend Shared ReadOnly Property P017_UseIntServer() As String
            Get
                Return ResourceManager.GetString("P017_UseIntServer", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Базы данных.
        '''</summary>
        Friend Shared ReadOnly Property P018_Databases() As String
            Get
                Return ResourceManager.GetString("P018_Databases", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Подключение.
        '''</summary>
        Friend Shared ReadOnly Property P020_Connection() As String
            Get
                Return ResourceManager.GetString("P020_Connection", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Каталог проекта SPP2:.
        '''</summary>
        Friend Shared ReadOnly Property P021_ProjectDirectory() As String
            Get
                Return ResourceManager.GetString("P021_ProjectDirectory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Настройки Apache.
        '''</summary>
        Friend Shared ReadOnly Property P022_ApacheSettingsCaption() As String
            Get
                Return ResourceManager.GetString("P022_ApacheSettingsCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Информация.
        '''</summary>
        Friend Shared ReadOnly Property P023_InfoCaption() As String
            Get
                Return ResourceManager.GetString("P023_InfoCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Параметры сервера.
        '''</summary>
        Friend Shared ReadOnly Property P024_ServerParametersCaption() As String
            Get
                Return ResourceManager.GetString("P024_ServerParametersCaption", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Повторная загрузка лаунчера после селектора сервера..
        '''</summary>
        Friend Shared ReadOnly Property P025_ReShowLauncher() As String
            Get
                Return ResourceManager.GetString("P025_ReShowLauncher", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Применены настройки сервера {0}.
        '''</summary>
        Friend Shared ReadOnly Property P026_SettingsApplied() As String
            Get
                Return ResourceManager.GetString("P026_SettingsApplied", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Выбран сервер: {0}.
        '''</summary>
        Friend Shared ReadOnly Property P027_ServerSelected() As String
            Get
                Return ResourceManager.GetString("P027_ServerSelected", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Перезагрузка приложения..
        '''</summary>
        Friend Shared ReadOnly Property P028_ApplicationRestart() As String
            Get
                Return ResourceManager.GetString("P028_ApplicationRestart", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Запуск основного приложения..
        '''</summary>
        Friend Shared ReadOnly Property P029_LaunchMain() As String
            Get
                Return ResourceManager.GetString("P029_LaunchMain", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Остановка сервера MySQL..
        '''</summary>
        Friend Shared ReadOnly Property SQL001_Shutdown() As String
            Get
                Return ResourceManager.GetString("SQL001_Shutdown", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Запуск сервера MySQL..
        '''</summary>
        Friend Shared ReadOnly Property SQL002_Start() As String
            Get
                Return ResourceManager.GetString("SQL002_Start", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Сервер MySQL успешно запущен..
        '''</summary>
        Friend Shared ReadOnly Property SQL003_Started() As String
            Get
                Return ResourceManager.GetString("SQL003_Started", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Ищет локализованную строку, похожую на Сервер MySQL успешно остановлен..
        '''</summary>
        Friend Shared ReadOnly Property SQL004_Stopped() As String
            Get
                Return ResourceManager.GetString("SQL004_Stopped", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
