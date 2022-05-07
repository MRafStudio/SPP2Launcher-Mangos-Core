﻿Imports System.Threading

Public Class Launcher

    Private ReadOnly lockWorld As New Object

    Private ReadOnly lockRealmd As New Object

#Region " === ДРУЖЕСТВЕННЫЕ СВОЙСТВА === "

    ''' <summary>
    ''' Флаг объявляющий требование выхода из программы.
    ''' </summary>
    Friend ReadOnly Property NeedExitLauncher As Boolean

    ''' <summary>
    ''' Возвращает состояние стартового потока.
    ''' </summary>
    ''' <returns></returns>
    Friend Property StartThreadCompleted As Boolean
        Get
            Return IsNothing(_isStart) OrElse _isStart.IsAlive
        End Get
        Set(value As Boolean)
            _isStart = Nothing
        End Set
    End Property

    ''' <summary>
    ''' Флаг немедленной остановки серверов WoW.
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property NeedServerStop As Boolean

    ''' <summary>
    ''' Флаг автоматического запуска сервера WoW. 
    ''' </summary>
    Friend ReadOnly Property ServerWowAutostart As Boolean

    ''' <summary>
    ''' Флаг разрешения закрытия лаунчера.
    ''' </summary>
    Friend Property EnableClosing As Boolean

    ''' <summary>
    ''' Процесс REALMD
    ''' </summary>
    ''' <returns></returns>
    Friend Property RealmdProcess As Process

    ''' <summary>
    ''' Процесс WORLD
    ''' </summary>
    ''' <returns></returns>
    Friend Property WorldProcess As Process

    ''' <summary>
    ''' Флаг работы MySQL.
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property MySqlON As Boolean

    ''' <summary>
    ''' Флаг работы Realmd.
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property RealmdON As Boolean

    ''' <summary>
    ''' Флаг блокировки сервера MySQL.
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property MySqlLOCKED As Boolean

#End Region

#Region " === ПРИВАТНЫЕ ПОЛЯ === "

    Private _needServerStart As Boolean

    ''' <summary>
    ''' Хранит команду ручного запуска сервера.
    ''' </summary>
    Private _isLastCommandStart As Boolean

    ''' <summary>
    ''' Буфер команд отправленных с консоли
    ''' </summary>
    Private ReadOnly ConsoleCommandBuffer As New ConsoleBuffer

    ''' <summary>
    ''' Поток при запуске лаунчера.
    ''' </summary>
    Private _isStart As Threading.Thread

    ''' <summary>
    ''' Процесс MySQL
    ''' </summary>
    Private _mysqlProcess As Process

    ''' <summary>
    ''' Файл конфигурации сервера Realmd.
    ''' </summary>
    Private _iniRealmd As IniFiles

    ''' <summary>
    ''' Файл конфигурации сервера World.
    ''' </summary>
    Private _iniWorld As IniFiles

    ''' <summary>
    ''' Флаг работы World
    ''' </summary>
    Private _worldON As Boolean

    ''' <summary>
    ''' Флаг запрета запуска Apache
    ''' </summary>
    Private _apacheSTOP As Boolean

    ''' <summary>
    ''' Флаг работы Apache
    ''' </summary>
    Private _apacheON As Boolean

    ''' <summary>
    ''' Apache заблокирован.
    ''' </summary>
    Private _apacheLOCKED As Boolean

    ''' <summary>
    ''' Servers заблокирован.
    ''' </summary>
    Private _serversLOCKED As Boolean

#End Region

#Region " === КОНСТРУКТОР ИНИЦИАЛИЗАЦИИ === "

    Sub New()

        ' Восстановим размеры и положение окна в буфер
        Dim sp = FormStartPosition.Manual
        Dim sz = My.Settings.AppSize
        Dim loc = My.Settings.AppLocation

        InitializeComponent()

        ' Перенесём размеры и положение из буфера в форму
        Me.StartPosition = sp
        Me.Size = sz

        ' Устанавливаем локацию
        If loc.X < 0 Or loc.Y < 0 Then
            ' Исправляем ошибку, если сервер был прихлопнут в свёрнутом состоянии
            loc = New Point(0, 0)
            My.Settings.AppLocation = loc
            My.Settings.Save()
        End If
        Me.Location = loc

        ' Если всего один модуль, то прячем смену типа сервера
        If GV.Modules.Count = 1 Then TSMI_ServerSwitcher.Visible = False

        ' Настраиваем таймеры
        TimerStartMySQL = New Threading.Timer(AddressOf TimerTik_StartMySQL)
        TimerStartMySQL.Change(Threading.Timeout.Infinite, Threading.Timeout.Infinite)
        TimerStartWorld = New Threading.Timer(AddressOf TimerTik_StartWorld)
        TimerStartWorld.Change(Threading.Timeout.Infinite, Threading.Timeout.Infinite)
        TimerStartRealmd = New Threading.Timer(AddressOf TimerTik_StartRealmd)
        TimerStartRealmd.Change(Threading.Timeout.Infinite, Threading.Timeout.Infinite)

        ' Загружаем шрифт (попытка загрузить в память через GetFont пока не удалась - преследуют ошибки)
        LoadFont()

        ' Инициализируем BaseProcess
        BP = New ProcessController

        ' Первоначальная надпись в строке состояния ToolTip
        TSSL_Count.ToolTipText = String.Format(My.Resources.P011_AllChars, "N/A")

    End Sub

#End Region

#Region " === СТАНДАРТЫЕ ОПЕРАЦИИ === "

    ''' <summary>
    ''' ПРИ ЗАГРУЗКЕ ФОРМЫ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Launcher_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Инициализируем настройки приложения
        UpdateSettings()

        ' Включем поток ежесекундного тика
        If Not My.Settings.FirstStart Then
            Dim tikTok = New Threading.Thread(Sub() EverySecond()) With {
                .CurrentCulture = GV.CI,
                .CurrentUICulture = GV.CI,
                .IsBackground = True
            }
            tikTok.Start()
        End If

        ' Включаем поток вывода команды разработчиков
        _isStart = New Threading.Thread(Sub() PreStart()) With {
            .CurrentCulture = GV.CI,
            .CurrentUICulture = GV.CI,
            .IsBackground = True
        }
        _isStart.Start()

    End Sub

    ''' <summary>
    ''' ПРИ ЗАКРЫТИИ ФОРМЫ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Launcher_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If My.Settings.FirstStart Then
            ShutdownAll(True)
            Threading.Thread.Sleep(1000)
            GV.SPP2Launcher.NotifyIcon_SPP2.Visible = False
        Else
            If EnableClosing = False Then
                Try
                    Me.WindowState = FormWindowState.Minimized
                Catch
                End Try
                e.Cancel = True
            Else
                GV.Log.WriteInfo(My.Resources.P005_Exiting)
                Threading.Thread.Sleep(1000)
                GV.SPP2Launcher.NotifyIcon_SPP2.Visible = False
                If GV.NeedRestart Then Application.Restart()
            End If
        End If
    End Sub

#End Region

#Region " === ЗАПУСК КЛИЕНТА WOW === "

    ''' <summary>
    ''' Запуск клиента WoW
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_RunWow_Click(sender As Object, e As EventArgs) Handles TSMI_RunWow.Click
        StartWowClient()
    End Sub

    ''' <summary>
    ''' ЗАПУСКАЕТ WoW КЛИЕНТА
    ''' </summary>
    Private Sub StartWowClient()
        Try
            If My.Settings.WowClientPath = "" Then
                Using openFileDialog As New OpenFileDialog()
                    openFileDialog.InitialDirectory = "c:\"
                    openFileDialog.Filter = My.Resources.P003_SetWowClientPath
                    openFileDialog.FilterIndex = 1
                    openFileDialog.RestoreDirectory = True

                    If openFileDialog.ShowDialog() = DialogResult.OK Then
                        My.Settings.WowClientPath = openFileDialog.FileName
                        My.Settings.Save()
                    End If
                End Using
            End If
            If My.Settings.WowClientPath <> "" Then
                Process.Start(My.Settings.WowClientPath)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message,
                            My.Resources.E003_ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region " === ЯЗЫК ИНТЕРФЕЙСА === "

    ''' <summary>
    ''' АНГЛИЙСКИЙ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_English_Click(sender As Object, e As EventArgs) Handles TSMI_English.Click
        My.Settings.Locale = "en-GB"
        TryRestart()
    End Sub

    ''' <summary>
    ''' РУССКИЙ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_Russian_Click(sender As Object, e As EventArgs) Handles TSMI_Russian.Click
        My.Settings.Locale = "ru-RU"
        TryRestart()
    End Sub

    ''' <summary>
    ''' Попытка перезагрузки лайнчера.
    ''' </summary>
    Private Sub TryRestart()
        My.Settings.Save()
        If ServerWowAutostart Or Not IsNothing(_mysqlProcess) Then
            ' Перезагрузка отменяется
            MessageBox.Show(My.Resources.P006_NeedReboot,
                            My.Resources.P007_MessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            _EnableClosing = True
            Application.Restart()
        End If
    End Sub

#End Region

#Region " === ЛАУНЧЕР === "

    ''' <summary>
    ''' МЕНЮ - НАСТРОЙКИ ПРИЛОЖЕНИЯ.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_Launcher_Click(sender As Object, e As EventArgs) Handles TSMI_Launcher.Click
        Dim fLauncherSettings As New LauncherSettings
        fLauncherSettings.ShowDialog()
    End Sub

    ''' <summary>
    ''' МЕНЮ - СБРОС НАСТРОЕК ПРИЛОЖЕНИЯ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_Reset_Click(sender As Object, e As EventArgs) Handles TSMI_Reset.Click
        Dim result = MessageBox.Show(My.Resources.P014_ResetSettings,
                                     My.Resources.P016_WarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If result = DialogResult.Yes Then
            ' Устанавливаем флаги - нужна перезагрузка, это сброс настроек
            GV.NeedRestart = True
            GV.ResetSettings = True
            ' Останавливаем ВСЕ сервера
            ShutdownAll(True)
        End If
    End Sub

    ''' <summary>
    ''' КНОПКА - РАЗБЛОКИРОВАТЬ ВСЁ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button_UnlockAll_Click(sender As Object, e As EventArgs) Handles Button_UnlockAll.Click
        Dim dr = MessageBox.Show(My.Resources.P048_UnlockAll,
                                 My.Resources.P016_WarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If dr = DialogResult.No Then Exit Sub
        If _serversLOCKED Then
            ' Блокируем меню серверов
            LockedServersMenu()
            ' Запускаем поток выключающий все сервера
            Dim sw As New Threading.Thread(Sub() ShutdownAll(False)) With {
                .CurrentCulture = GV.CI,
                .CurrentUICulture = GV.CI,
                .IsBackground = True
            }
            sw.Start()
        Else
            ' Выключаем Apache
            If _apacheLOCKED Then ShutdownApache()
            If _mysqlLOCKED Then ShutdownMySQL(EProcess.mysqld, EAction.UpdateMainMenu)
        End If
    End Sub

    ''' <summary>
    ''' ПРИ ИЗМЕНЕНИИ РАЗМЕРА ОКНА.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Launcher_SizeChanged(sender As Object, e As EventArgs) Handles MyBase.SizeChanged
        If Me.Size.Width >= 640 And Me.Size.Height >= 420 Then
            My.Settings.AppSize = Me.Size
        End If
        My.Settings.Save()
        If Me.WindowState = FormWindowState.Minimized Then
            ' Эта штука переводит приложение в фоновый режим
            ' А ОНО НАДО?
            Me.Hide()
        End If
    End Sub

    ''' <summary>
    ''' ПРИ ИЗМЕНЕНИЕ ЛОКАЦИИ ПРИЛОЖЕНИЯ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Launcher_LocationChanged(sender As Object, e As EventArgs) Handles MyBase.LocationChanged
        If Location.X > 0 And Location.Y > 0 Then
            My.Settings.AppLocation = Location
            My.Settings.Save()
        End If
    End Sub

    ''' <summary>
    ''' Вывод сообщения в StatusStrip.
    ''' </summary>
    ''' <param name="text"></param>
    Friend Function OutMessageStatusStrip(text As String) As String
        Try
            If Not Me.InvokeRequired Then
                Me.Invoke(Sub()
                              OutMessageStatusStrip(text)
                          End Sub)
            Else
                TSSL_ALL.Text = text
            End If
            Return text
        Catch ex As Exception
            GV.Log.WriteException(ex)
            Return text
        End Try
    End Function

    ''' <summary>
    ''' Обновление информации в StatusStrip.
    ''' </summary>
    Friend Sub OutInfoStatusStrip()
        Dim online, total As String
        If _MySqlON Then
            online = MySqlDataBases.CHARACTERS.CHARACTERS.SELECT_ONLINE_CHARS()
            total = MySqlDataBases.CHARACTERS.CHARACTERS.SELECT_TOTAL_CHARS()
        Else
            online = "N/A"
            total = "N/A"
        End If
        If TSSL_Online.GetCurrentParent.InvokeRequired Then
            TSSL_Online.GetCurrentParent.Invoke(Sub()
                                                    TSSL_Count.Text = online
                                                    TSSL_Count.ToolTipText = String.Format(My.Resources.P011_AllChars, total)
                                                End Sub)
        Else
            TSSL_Count.Text = online
            TSSL_Count.ToolTipText = String.Format(My.Resources.P011_AllChars, total)
        End If
    End Sub

    ''' <summary>
    ''' Обновление параметров для вывода на экран.
    ''' </summary>
    Friend Sub UpdateSettings()

        ' Заголовок приложения
        Dim srv As String = ""
        Select Case My.Settings.LastLoadedServerType

            Case GV.EModule.Classic.ToString
                srv = "Classic"
                _iniRealmd = New IniFiles(My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\vanilla\realmd.conf")
                _iniWorld = New IniFiles(My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\vanilla\mangosd.conf")
                My.Settings.CurrentFileRealmd = My.Settings.DirSPP2 & "\" & SPP2CMANGOS & "\vanilla\Bin64\realmd.exe"
                My.Settings.CurrentFileWorld = My.Settings.DirSPP2 & "\" & SPP2CMANGOS & "\vanilla\Bin64\mangosd.exe"
                My.Settings.CurrentServerSettings = My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\vanilla\"
                _ServerWowAutostart = My.Settings.ServerClassicAutostart
                TSMI_OpenLauncher.Image = My.Resources.cmangos_classic_core
                GV.Log.WriteInfo(String.Format(My.Resources.P026_SettingsApplied, srv))

            Case GV.EModule.Tbc.ToString
                srv = "The Burning Crusade"
                _iniRealmd = New IniFiles(My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\tbc\realmd.conf")
                _iniWorld = New IniFiles(My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\tbc\mangosd.conf")
                My.Settings.CurrentFileRealmd = My.Settings.DirSPP2 & "\" & SPP2CMANGOS & "\tbc\Bin64\realmd.exe"
                My.Settings.CurrentFileWorld = My.Settings.DirSPP2 & "\" & SPP2CMANGOS & "\tbc\Bin64\mangosd.exe"
                My.Settings.CurrentServerSettings = My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\tbc\"
                _ServerWowAutostart = My.Settings.ServerTbcAutostart
                TSMI_OpenLauncher.Image = My.Resources.cmangos_tbc_core
                GV.Log.WriteInfo(String.Format(My.Resources.P026_SettingsApplied, srv))

            Case GV.EModule.Wotlk.ToString
                srv = "Wrath of the Lich King"
                _iniRealmd = New IniFiles(My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\wotlk\realmd.conf")
                _iniWorld = New IniFiles(My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\wotlk\mangosd.conf")
                My.Settings.CurrentFileRealmd = My.Settings.DirSPP2 & "\" & SPP2CMANGOS & "\wotlk\Bin64\realmd.exe"
                My.Settings.CurrentFileWorld = My.Settings.DirSPP2 & "\" & SPP2CMANGOS & "\wotlk\Bin64\mangosd.exe"
                My.Settings.CurrentServerSettings = My.Settings.DirSPP2 & "\" & SPP2SETTINGS & "\wotlk\"
                _ServerWowAutostart = My.Settings.ServerWotlkAutostart
                TSMI_OpenLauncher.Image = My.Resources.cmangos_wotlk_core
                GV.Log.WriteInfo(String.Format(My.Resources.P026_SettingsApplied, srv))

            Case Else
                Dim str = String.Format(My.Resources.E008_UnknownModule, My.Settings.LastLoadedServerType)
                GV.Log.WriteError(str)
                MessageBox.Show(str,
                                My.Resources.E003_ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Select
        My.Settings.Save()

        Text = My.Resources.P010_LauncherCaption & " : " & srv

        ' Инициализируем MySQL
        GV.SQL = New MySqlProvider()

        ' Настраиваем консоли
        Dim bc As Color = If(My.Settings.ConsoleTheme = "Black", Drawing.Color.Black, Drawing.Color.SeaShell)
        Dim fnt = GetCurrentFont()

        ' Realmd
        RichTextBox_ConsoleRealmd.ForeColor = My.Settings.RealmdConsoleForeColor
        RichTextBox_ConsoleRealmd.BackColor = bc
        RichTextBox_ConsoleRealmd.Font = fnt

        ' World
        RichTextBox_ConsoleWorld.ForeColor = My.Settings.WorldConsoleForeColor
        RichTextBox_ConsoleWorld.BackColor = bc
        RichTextBox_ConsoleWorld.Font = fnt

        ' MySQL
        RichTextBox_ConsoleMySQL.ForeColor = ECONSOLE
        RichTextBox_ConsoleMySQL.BackColor = bc
        RichTextBox_ConsoleMySQL.Font = fnt

        ' Настраиваем BP и запускаем процесс контроля
        BP.AddProcess(GV.EProcess.Realmd)
        BP.AddProcess(GV.EProcess.World)
        If Not IsNothing(PC) Then PC.Abort()
        PC = New Threading.Thread(Sub() Controller()) With {
            .CurrentCulture = GV.CI,
            .CurrentUICulture = GV.CI,
            .IsBackground = True
        }
        PC.CurrentCulture = GV.CI
        PC.CurrentUICulture = GV.CI
        PC.Start()

        ' Разбираемся с пунктом меню смены типа сервера
        If _ServerWowAutostart Then
            ' Прячем меню смены сервера
            TSMI_ServerSwitcher.Visible = False
        Else
            ' Отображаем меню смены сервера
            TSMI_ServerSwitcher.Visible = True
        End If

        ' Устанавливаем тему консоли.
        SetConsoleTheme()

        ' Выводим состояние пункта меню автозапуска сервера
        TSMI_WowAutoStart.Checked = ServerWowAutostart

        ' Если это первый запуск - предупреждаем
        If My.Settings.FirstStart Then
            MessageBox.Show(My.Resources.P012_FirstStart,
                            My.Resources.P023_InfoCaption, MessageBoxButtons.OK, MessageBoxIcon.Information)
            ' Выключаем все меню запуска/перезапуска серверов
            TSMI_MySqlStart.Visible = False
            TSMI_MySqlRestart.Visible = False
            TSMI_MySqlStop.Visible = False
            TSMI_MySQL1.Visible = False
            TSMI_ApacheStart.Visible = False
            TSMI_ApacheRestart.Visible = False
            TSMI_ApacheStop.Visible = False
            TSMI_Apache1.Visible = False
            TSMI_ServerStart.Visible = False
            TSMI_ServerStop.Visible = False
            TSMI_Server2.Visible = False
            TSMI_RunWow.Enabled = False
            TSMI_ServerSwitcher.Enabled = False
            TSMI_Tools.Visible = False
            TSMI_Bots.Visible = False
            TSMI_Reset.Enabled = False
        Else
            ' Настраиваем меню серверов.
            UpdateMainMenu(True)
        End If

    End Sub

    ''' <summary>
    ''' Блокировка меню всех серверов.
    ''' </summary>
    Friend Sub LockedServersMenu()
        If Me.InvokeRequired Then
            Me.Invoke(Sub()
                          LockedServersMenu()
                      End Sub)
        Else
            TSMI_ServerStart.Enabled = False
            TSMI_ServerStop.Enabled = False
            TSMI_MySqlStart.Enabled = False
            TSMI_MySqlRestart.Enabled = False
            TSMI_MySqlStop.Enabled = False
            TSMI_ApacheStart.Enabled = False
            TSMI_ApacheRestart.Enabled = False
            TSMI_ApacheStop.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Обновление параметров главного меню приложения.
    ''' </summary>
    Friend Sub UpdateMainMenu(firstStart As Boolean)
        If Me.InvokeRequired Then
            Me.Invoke(Sub()
                          UpdateMainMenu(firstStart)
                      End Sub)
        Else

            ' Обходим проверку блокировки серверов, если это выход из приложения
            If Not _NeedExitLauncher Then

                ' Проверяем процесс Apache которого не должно быть
                If firstStart AndAlso CheckProcess(EProcess.apache, False) Then _apacheLOCKED = True Else _apacheLOCKED = False

                ' Проверяем процесс MySQL которого не должно быть
                If CheckProcess(EProcess.mysqld, False) Then _MySqlLOCKED = True Else _MySqlLOCKED = False

                ' ПРоверяем процессы Realmd и World
                If CheckProcess(EProcess.realmd, False) Or CheckProcess(EProcess.world, False) Then _serversLOCKED = True Else _serversLOCKED = False

            End If

            ' Обустраиваем вкладки
            If _apacheLOCKED Or _MySqlLOCKED Or _serversLOCKED Then
                TabPage_MySQL.Text = "Errors"
                TabPage_Realmd.Visible = False
                TabControl1.SelectedTab = TabPage_MySQL
            Else
                TabPage_MySQL.Text = "MySQL & Apache"
                TabPage_Realmd.Visible = True
                TabControl1.SelectedTab = TabPage_World
            End If

            ' Обустраиваем меню MySQL
            If My.Settings.UseIntMySQL Then
                If _MySqlLOCKED Then
                    TSMI_MySqlStart.Enabled = False
                    TSMI_MySqlRestart.Enabled = False
                ElseIf ServerWowAutostart Then
                    TSMI_MySqlStart.Enabled = False
                    TSMI_MySqlRestart.Enabled = False
                    TSMI_MySqlStop.Enabled = False
                Else
                    TSMI_MySqlStart.Enabled = True
                    TSMI_MySqlRestart.Enabled = True
                    TSMI_MySqlStop.Enabled = True
                End If
            Else
                TSMI_MySqlStart.Enabled = False
                TSMI_MySqlRestart.Enabled = False
                TSMI_MySqlStop.Enabled = False
            End If

            ' Обустраиваем меню Apache
            If My.Settings.UseIntApache Then
                If _apacheLOCKED Then
                    TSMI_ApacheStart.Enabled = False
                    TSMI_ApacheRestart.Enabled = False
                Else
                    TSMI_ApacheStart.Enabled = True
                    TSMI_ApacheRestart.Enabled = True
                    TSMI_ApacheStop.Enabled = True
                End If
            Else
                TSMI_ApacheStart.Enabled = False
                TSMI_ApacheRestart.Enabled = False
                TSMI_ApacheStop.Enabled = False
            End If

            ' Обустраиваем меню Server
            If _MySqlLOCKED Or _serversLOCKED Then
                TSMI_ServerStart.Enabled = False
                TSMI_ServerStop.Enabled = False
            Else
                TSMI_ServerStart.Enabled = True
                TSMI_ServerStop.Enabled = True
            End If

            ' Если нет других mysqld.exe используется встроенный сервер MySQL и включен его автостарт и при этом это не первый запуск
            If Not _MySqlLOCKED And My.Settings.UseIntMySQL And My.Settings.MySqlAutostart And Not My.Settings.FirstStart Then
                ' Используем запуск с ожиданием завершения другого процесса, если таковой был
                WaitProcessEnd(EProcess.mysqld, EAction.Start, False)
            End If

            ' Если нет других mysqld.exe и включен автозапуск сервера
            If Not _MySqlLOCKED And ServerWowAutostart Then
                ' Выставляем флаг необходимости запуска всего комплекса
                _needServerStart = True
                GV.SPP2Launcher.UpdateRealmdConsole(My.Resources.P019_ControlEnabled, CONSOLE)
                GV.SPP2Launcher.UpdateWorldConsole(My.Resources.P019_ControlEnabled, CONSOLE)
            End If

            ' Отображение кнопки блокировки/разблокировки всех серверов
            If _MySqlLOCKED Or _apacheLOCKED Or _serversLOCKED Then
                RichTextBox_ConsoleMySQL.Clear()
                Button_UnlockAll.Visible = True
            Else
                Button_UnlockAll.Visible = False
            End If

            ' ТЕПЕРЬ ПО LOCKED ПОДСКАЗКАМ

            If _apacheLOCKED Then
                GV.Log.WriteWarning(UpdateMySQLConsole(String.Format(My.Resources.P046_ProcessDetected, "spp-httpd.exe"), ECONSOLE))
                GV.Log.WriteWarning(UpdateMySQLConsole(My.Resources.P047_Locked1, WCONSOLE))
                GV.Log.WriteWarning(UpdateMySQLConsole(My.Resources.P047_Locked2 & vbCrLf, WCONSOLE))
            End If

            If _MySqlLOCKED Then
                GV.Log.WriteWarning(UpdateMySQLConsole(String.Format(My.Resources.P046_ProcessDetected, "mysqld.exe"), ECONSOLE))
                GV.Log.WriteWarning(UpdateMySQLConsole(My.Resources.P047_Locked3, WCONSOLE))
                GV.Log.WriteWarning(UpdateMySQLConsole(My.Resources.P047_Locked4 & vbCrLf, WCONSOLE))
            End If

        End If
    End Sub

#End Region

#Region " === РЕЗЕРВНОЕ СОХРАНЕНИЕ === "

    ''' <summary>
    ''' Выполняет автосохранение.
    ''' </summary>
    Private Sub AutoSave()

    End Sub

#End Region

#Region " === СМЕНИТЬ ТИП СЕРВЕРА === "

    ''' <summary>
    ''' СМЕНА ТИПА СЕРВЕРА
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ServerSwitcher_Click(sender As Object, e As EventArgs) Handles TSMI_ServerSwitcher.Click
        ' Выводим сообщение о перезагрузке приложения
        MessageBox.Show(My.Resources.P006_NeedReboot,
                            My.Resources.P023_InfoCaption, MessageBoxButtons.OK, MessageBoxIcon.Information)
        ' Устанавливаем флаг - нужна перезагрузка
        GV.NeedRestart = True
        ' Останавливаем ВСЕ сервера
        ShutdownAll(True)
    End Sub

#End Region

#Region " ===  MySQL === "

    ''' <summary>
    ''' МЕНЮ - ЗАПУСК MySQL
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_MySqlStart_Click(sender As Object, e As EventArgs) Handles TSMI_MySqlStart.Click
        ' Активируем вкладку MySQL
        TabControl1.SelectedTab = TabPage_MySQL
        If Not _mysqlLOCKED Then TimerStartMySQL.Change(1000, 1000)
    End Sub

    ''' <summary>
    ''' МЕНЮ - ПЕРЕЗАПУСК MySQL
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_MySqlRestart_Click(sender As Object, e As EventArgs) Handles TSMI_MySqlRestart.Click
        ' Активируем вкладку MySQL
        TabControl1.SelectedTab = TabPage_MySQL
        If Not _mysqlLOCKED Then ShutdownMySQL(EProcess.mysqld, EAction.Start)
    End Sub

    ''' <summary>
    ''' МЕНЮ - ОСТАНОВКА MySQL
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_MySqlStop_Click(sender As Object, e As EventArgs) Handles TSMI_MySqlStop.Click
        ' Активируем вкладку MySQL
        TabControl1.SelectedTab = TabPage_MySQL
        ShutdownMySQL(EProcess.mysqld, EAction.UpdateMainMenu)
    End Sub

    ''' <summary>
    ''' МЕНЮ - НАСТРОЙКИ MySQL
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_MySqlSettings_Click(sender As Object, e As EventArgs) Handles TSMI_MySqlSettings.Click
        Dim fMySqlSettings As New MySqlSettings
        fMySqlSettings.ShowDialog()
    End Sub

    ''' <summary>
    ''' Запускает сервер MySQL
    ''' </summary>
    Friend Sub StartMySQL(obj As Object)
        If Not _MySqlLOCKED And My.Settings.UseIntMySQL And CheckProcess(EProcess.mysqld, False) And IsNothing(_mysqlProcess) Then
            _MySqlON = True
            GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P042_ServerStart, "MySQL"), CONSOLE))

            Dim exefile As String = My.Settings.DirSPP2 & "\" & SPP2MYSQL & "\bin\mysqld.exe"
            Dim settings As String = My.Settings.DirSPP2 & "\" & SPP2MYSQL & "\SPP-Database.ini"
            ' Создаём информацию о процессе
            Dim startInfo = New ProcessStartInfo("cmd.exe") With {
                .Arguments = "/C " & exefile & " --defaults-file=" & settings & " --standalone --console",
                .CreateNoWindow = True,
                .RedirectStandardInput = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .UseShellExecute = False,
                .WindowStyle = ProcessWindowStyle.Hidden,
                .WorkingDirectory = My.Settings.DirSPP2 & "\" & SPP2MYSQL
            }
            _mysqlProcess = New Process()
            Try
                _mysqlProcess.StartInfo = startInfo
                ' Запускаем
                If _mysqlProcess.Start() Then
                    GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P043_ServerStarted, "MySQL"), CONSOLE))
                    AddHandler _mysqlProcess.OutputDataReceived, AddressOf MySqlOutputDataReceived
                    AddHandler _mysqlProcess.ErrorDataReceived, AddressOf MySqlErrorDataReceived
                    AddHandler _mysqlProcess.Exited, AddressOf MySqlExited
                    _mysqlProcess.BeginOutputReadLine()
                    _mysqlProcess.BeginErrorReadLine()
                Else
                    _mysqlProcess = Nothing
                End If
            Catch ex As Exception
                ' MySQL выдал исключение
                _MySqlON = False
                GV.Log.WriteException(ex)
                UpdateMySQLConsole("[EConsole] " & String.Format(My.Resources.E019_StopException, "MySQL"), ECONSOLE)
            End Try
        Else
            GV.Log.WriteWarning(UpdateMySQLConsole(String.Format(My.Resources.P041_AlreadyStarted, "MySQL"), ECONSOLE))
        End If
    End Sub

    ''' <summary>
    ''' Выход из mysqld.exe
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MySqlExited(ByVal sender As Object, ByVal e As EventArgs)
        RemoveHandler _mysqlProcess.OutputDataReceived, AddressOf MySqlOutputDataReceived
        RemoveHandler _mysqlProcess.ErrorDataReceived, AddressOf MySqlErrorDataReceived
        RemoveHandler _mysqlProcess.Exited, AddressOf MySqlExited
    End Sub

    ''' <summary>
    ''' Останавливает сервер MySQL
    ''' </summary>
    Friend Sub ShutdownMySQL(p As EProcess, a As EAction)
        ' Проверяем наличие процесса
        If Not CheckProcess(EProcess.mysqld, False) Then Exit Sub
        GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P044_ServerStop, "MySQL"), CONSOLE))
        If My.Settings.UseIntMySQL Then
            Dim login, pass, port As String
            Select Case My.Settings.LastLoadedServerType
                Case GV.EModule.Classic.ToString
                    login = My.Settings.MySqlClassicIntUserName
                    pass = My.Settings.MySqlClassicIntPassword
                    port = My.Settings.MySqlClassicIntPort
                Case GV.EModule.Tbc.ToString
                    login = My.Settings.MySqlClassicIntUserName
                    pass = My.Settings.MySqlClassicIntPassword
                    port = My.Settings.MySqlClassicIntPort
                Case GV.EModule.Wotlk.ToString
                    login = My.Settings.MySqlClassicIntUserName
                    pass = My.Settings.MySqlClassicIntPassword
                    port = My.Settings.MySqlClassicIntPort
                Case Else
                    ' Неизвестный модуль
                    GV.Log.WriteWarning(String.Format(My.Resources.E008_UnknownModule, My.Settings.LastLoadedServerType))
                    Exit Sub
            End Select
            ' Создаём информацию для процесса
            Dim startInfo = New ProcessStartInfo With {
            .CreateNoWindow = True,
            .UseShellExecute = False,
            .FileName = My.Settings.DirSPP2 & "\" & SPP2MYSQL & "\bin\mysqladmin.exe",
            .Arguments = "-u " & login & " -p" & pass & " --port=" & port & " shutdown",
            .WindowStyle = ProcessWindowStyle.Hidden
            }
            ' Выполняем
            Try
                ' Блокируем меню серверов
                LockedServersMenu()
                Using exeProcess = Process.Start(startInfo)
                    exeProcess.WaitForExit()
                End Using
                ' Создаём поток для ожидания остановки MySQL
                Dim wpe = New Threading.Thread(Sub() WaitProcessEnd(p, a)) With {
                    .CurrentCulture = GV.CI,
                    .CurrentUICulture = GV.CI,
                    .IsBackground = True
                }
                wpe.Start()
            Catch ex As Exception
                ' Исключение при остановке MySQL
                UpdateMainMenu(False)
                GV.Log.WriteException(ex)
                GV.Log.WriteError(UpdateMySQLConsole(String.Format(My.Resources.E019_StopException, "MySQL"), ECONSOLE))
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Проверка соединения с сервером MySQL
    ''' Отсюда стартует автоматом Apache
    ''' Остюда стартует при _NeedServerStart и серверы WoW
    ''' </summary>
    Friend Sub CheckMySQL()
        Dim host, port As String
        If My.Settings.UseIntMySQL Then
            Select Case My.Settings.LastLoadedServerType
                Case GV.EModule.Classic.ToString
                    host = My.Settings.MySqlClassicIntHost
                    port = My.Settings.MySqlClassicIntPort
                Case GV.EModule.Tbc.ToString
                    host = My.Settings.MySqlClassicIntHost
                    port = My.Settings.MySqlClassicIntPort
                Case GV.EModule.Wotlk.ToString
                    host = My.Settings.MySqlClassicIntHost
                    port = My.Settings.MySqlClassicIntPort
                Case Else
                    ' Неизвестный модуль
                    GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                    Exit Sub
            End Select
        Else
            Select Case My.Settings.LastLoadedServerType
                Case GV.EModule.Classic.ToString
                    host = My.Settings.MySqlClassicExtHost
                    port = My.Settings.MySqlClassicExtPort
                Case GV.EModule.Tbc.ToString
                    host = My.Settings.MySqlClassicExtHost
                    port = My.Settings.MySqlClassicExtPort
                Case GV.EModule.Wotlk.ToString
                    host = My.Settings.MySqlClassicExtHost
                    port = My.Settings.MySqlClassicExtPort
                Case Else
                    ' Неизвестный модуль
                    GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                    Exit Sub
            End Select
        End If

        If host = "" Or port = "" Then Exit Sub
        Dim tcpClient = New Net.Sockets.TcpClient
        Dim ac = tcpClient.BeginConnect(host, CInt(port), Nothing, Nothing)
        Try
            If Not ac.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), False) Then
                tcpClient.Close()

                Try
                    If Me.Visible Then
                        TSSL_MySQL.GetCurrentParent().Invoke(Sub()
                                                                 TSSL_MySQL.Image = My.Resources.red_ball
                                                                 If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                     Me.Icon = My.Resources.cmangos_red
                                                                     Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                                                                 Else
                                                                     Me.Icon = My.Resources.wow
                                                                     Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                                 End If
                                                             End Sub)
                    Else
                        ' Меняем иконку в трее, коли свёрнуты
                        If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                            Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                        Else
                            Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                        End If
                    End If
                Catch ex As Exception
                    GV.Log.WriteException(ex)
                End Try

                _MySqlON = False

            Else

                tcpClient.EndConnect(ac)
                tcpClient.Close()

                Try
                    If Me.Visible Then
                        TSSL_MySQL.GetCurrentParent().Invoke(Sub()
                                                                 TSSL_MySQL.Image = My.Resources.green_ball
                                                                 If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                     If Not _RealmdON Or Not _worldON Then
                                                                         Me.Icon = My.Resources.cmangos_orange
                                                                         Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_orange
                                                                     End If
                                                                 Else
                                                                     Me.Icon = My.Resources.wow
                                                                     Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                                 End If
                                                             End Sub)
                    Else
                        ' Меняем иконку в трее, коли свёрнуты
                        If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                            If Not _RealmdON Or Not _worldON Then
                                Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_orange
                            End If
                        Else
                            Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                        End If
                    End If
                Catch ex As Exception
                    GV.Log.WriteException(ex)
                End Try

                _MySqlON = True

            End If

        Catch ex As Exception
            GV.Log.WriteException(ex)

            Try
                If Me.Visible Then
                    TSSL_MySQL.GetCurrentParent().Invoke(Sub()
                                                             TSSL_MySQL.Image = My.Resources.red_ball
                                                             If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                 Me.Icon = My.Resources.cmangos_red
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                                                             Else
                                                                 Me.Icon = My.Resources.wow
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                             End If
                                                         End Sub)
                Else
                    ' Меняем иконку в трее, коли свёрнуты
                    If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                        Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                    Else
                        Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                    End If
                End If
            Catch ex2 As Exception
                GV.Log.WriteException(ex2)
            End Try

            _MySqlON = False

        Finally

            ' Если нет и процесса, то грохаем и его аватар
            'If Not CheckProcess(EProcess.mysqld) Then
            'If Not IsNothing(_mysqlProcess) Then MySqlExited(Me, Nothing)
            '_mysqlProcess = Nothing
            'End If

            If _MySqlON Then

                ' Если установлен автозапуск сервера Apache
                If My.Settings.UseIntApache And My.Settings.ApacheAutostart And Not _apacheSTOP And Not _apacheON Then
                    StartApache()
                End If

                ' Проверить на флаг остановки
                If Not NeedServerStop Then
                    ' Поступил запрос на запуск серверов WoW
                    If _needServerStart Then
                        ' Запускаем World через 1 сек.
                        TimerStartWorld.Change(1000, 1000)
                        GV.Log.WriteInfo(String.Format(My.Resources.P021_TimerSetted, "world", "1000"))
                        ' А Realm через 3 сек.
                        TimerStartRealmd.Change(3000, 3000)
                        GV.Log.WriteInfo(String.Format(My.Resources.P021_TimerSetted, "realmd", "1000"))
                        ' Выключаем флаг ручного запуска сервера
                        _needServerStart = False
                    End If
                End If

            Else
                ' MySQL сдох - НЕЧЕГО ДЕЛАТЬ!
            End If

        End Try

        ac.AsyncWaitHandle.Close()

        ' Автоподсказки
        If _MySqlON AndAlso HintCollection.Count = 0 Then
            MySqlDataBases.MANGOS.COMMAND.SELECT_COMMAND(HintCollection)
            If TextBox_Command.InvokeRequired Then
                TextBox_Command.Invoke(Sub()
                                           TextBox_Command.AutoCompleteSource = AutoCompleteSource.CustomSource
                                           TextBox_Command.AutoCompleteCustomSource = HintCollection
                                           TextBox_Command.AutoCompleteMode = If(My.Settings.UseCommandAutoHints, AutoCompleteMode.SuggestAppend, AutoCompleteMode.None)
                                       End Sub)
            Else
                TextBox_Command.AutoCompleteSource = AutoCompleteSource.CustomSource
                TextBox_Command.AutoCompleteCustomSource = HintCollection
                TextBox_Command.AutoCompleteMode = If(My.Settings.UseCommandAutoHints, AutoCompleteMode.SuggestAppend, AutoCompleteMode.None)
            End If
        End If

    End Sub

#End Region

#Region " === СЕРВЕР APACHE === "

    ''' <summary>
    ''' МЕНЮ - ЗАПУСК Apache
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ApacheStart_Click(sender As Object, e As EventArgs) Handles TSMI_ApacheStart.Click
        If Not _apacheLOCKED Then
            If _MySqlON Then
                _apacheSTOP = False
                If My.Settings.ApacheAutostart Then
                    ' Ничего не делаем, CheckMySQL сам поднимет Apache
                Else
                    ' Запускаем Apache
                    StartApache()
                End If
            Else
                ' Без MySQL Apache не фиг делать - сайт не подымется
            End If
        End If
    End Sub

    ''' <summary>
    ''' МЕНЮ - ПЕРЕЗАПУСК Apache
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ApacheRestart_Click(sender As Object, e As EventArgs) Handles TSMI_ApacheRestart.Click
        If Not _apacheLOCKED Then
            _apacheSTOP = False
            ShutdownApache()
            If _MySqlON Then
                If My.Settings.ApacheAutostart Then
                    ' Ничего не делаем, MySQL сам поднимет Apache
                Else
                    ' Запускаем Apache
                    StartApache()
                End If
            Else
                ' Без MySQL Apache не фиг делать - сайт не подымется
            End If
        End If
    End Sub

    ''' <summary>
    ''' МЕНЮ - ОСТАНОВКА Apache
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ApacheStop_Click(sender As Object, e As EventArgs) Handles TSMI_ApacheStop.Click
        _apacheSTOP = True
        ShutdownApache()
        UpdateMainMenu(False)
    End Sub

    ''' <summary>
    ''' МЕНЮ - НАСТРОЙКА Apache
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ApacheSettings_Click(sender As Object, e As EventArgs) Handles TSMI_ApacheSettings.Click
        Dim fApacheSettings As New ApacheSettings
        fApacheSettings.ShowDialog()
    End Sub

    ''' <summary>
    ''' Запускает сервер Apache.
    ''' </summary>
    Friend Sub StartApache()
        If Not _apacheLOCKED Then
            If Not _apacheON Then
                _apacheON = True
                GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P042_ServerStart, "Apache"), CONSOLE))
                ' Разбираемся с настройками Apache
                If My.Settings.UseIntApache Then
                    Dim listen As String = ""
                    Select Case My.Settings.LastLoadedServerType
                        Case GV.EModule.Classic.ToString
                            listen = If(My.Settings.ApacheClassicIntHost = "ANY",
                                        "Listen " & My.Settings.ApacheClassicIntPort,
                                        "Listen " & My.Settings.ApacheClassicIntHost & ":" & My.Settings.ApacheClassicIntPort)
                        Case GV.EModule.Tbc.ToString
                            listen = If(My.Settings.ApacheTbcIntHost = "ANY",
                                        "Listen " & My.Settings.ApacheTbcIntPort,
                                        "Listen " & My.Settings.ApacheTbcIntHost & ":" & My.Settings.ApacheTbcIntPort)
                        Case GV.EModule.Wotlk.ToString
                            listen = If(My.Settings.ApacheWotlkIntHost = "ANY",
                                        "Listen " & My.Settings.ApacheWotlkIntPort,
                                        "Listen " & My.Settings.ApacheWotlkIntHost & ":" & My.Settings.ApacheWotlkIntPort)
                    End Select
                    If listen <> "" Then
                        ' Устанавливаем в httpd.conf текущие настройки
                        Dim lines() As String = System.IO.File.ReadAllLines(My.Settings.DirSPP2 & "\" & SPP2APACHE & "\conf\httpd.conf")
                        For count = 0 To lines.Length - 1
                            If lines(count).StartsWith("Listen ") Then
                                lines(count) = listen
                                System.IO.File.WriteAllLines(My.Settings.DirSPP2 & "\" & SPP2APACHE & "\conf\httpd.conf", lines)
                                Exit For
                            End If
                        Next
                    End If
                End If
                ' Создаём информацию о процессе
                Dim startInfo = New ProcessStartInfo() With {
                    .FileName = "CMD.exe",
                    .WorkingDirectory = My.Settings.DirSPP2 & "\" & SPP2APACHE & "\",
                    .Arguments = "/C " & My.Settings.DirSPP2 & "\" & SPP2APACHE & "\bin\spp-httpd.exe",
                    .CreateNoWindow = True,
                    .UseShellExecute = False,
                    .WindowStyle = ProcessWindowStyle.Normal
                }

                Dim p As New Process
                Try
                    p.StartInfo = startInfo
                    ' Запускаем
                    If p.Start() Then
                        GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P043_ServerStarted, "Apache"), CONSOLE))
                    Else
                        ' Не удалось запустить сервер
                        _apacheON = False
                        GV.Log.WriteError(UpdateMySQLConsole(String.Format(My.Resources.P049_NotStarted, "Apache"), ECONSOLE))
                    End If
                Catch ex As Exception
                    ' Apache выдал исключение
                    _apacheON = False
                    GV.Log.WriteException(ex)
                    GV.Log.WriteError(UpdateMySQLConsole(String.Format(My.Resources.E018_StartException, "Apache"), ECONSOLE))
                End Try
            Else
                GV.Log.WriteWarning(UpdateMySQLConsole(String.Format(My.Resources.P041_AlreadyStarted, "Apache"), WCONSOLE))
            End If
        End If
    End Sub

    ''' <summary>
    ''' Выключает встренный сервер Apache.
    ''' </summary>
    Friend Sub ShutdownApache()
        GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P044_ServerStop, "Apache"), CONSOLE))
        Try
            CheckProcess(EProcess.apache, True)
            GV.Log.WriteInfo(UpdateMySQLConsole(String.Format(My.Resources.P045_ServerStopped, "Apache"), CONSOLE))
            If Not _NeedExitLauncher Then UpdateMainMenu(False)
        Catch ex As Exception
            ' Исключение при остановке Apache
            GV.Log.WriteException(ex)
            GV.Log.WriteError(UpdateMySQLConsole(String.Format(My.Resources.E019_StopException, "Apache"), ECONSOLE))
        End Try
    End Sub

    ''' <summary>
    ''' Возвращает PID локального процесса Apache
    ''' </summary>
    ''' <returns></returns>
    Friend Function GetApachePid() As Integer
        Try
            If IO.File.Exists(My.Settings.DirSPP2 & "\" & SPP2APACHE & "\logs\httpd.pid") Then
                Dim line() = IO.File.ReadAllLines(My.Settings.DirSPP2 & "\" & SPP2APACHE & "\logs\httpd.pid", System.Text.Encoding.Default)
                Return CInt(line(0))
            Else
                Return 0
            End If
        Catch ex As Exception
            GV.Log.WriteException(ex)
            Return 0
        End Try
    End Function

    ''' <summary>
    ''' Проверка доступности Web сервера.
    ''' </summary>
    Public Sub CheckHttp()
        Dim host, port As String
        If My.Settings.UseIntApache Then
            Select Case My.Settings.LastLoadedServerType
                Case GV.EModule.Classic.ToString
                    host = My.Settings.ApacheClassicIntHost
                    port = My.Settings.ApacheClassicIntPort
                Case GV.EModule.Tbc.ToString
                    host = My.Settings.ApacheClassicIntHost
                    port = My.Settings.ApacheClassicIntPort
                Case GV.EModule.Wotlk.ToString
                    host = My.Settings.ApacheClassicIntHost
                    port = My.Settings.ApacheClassicIntPort
                Case Else
                    ' Неизвестный модуль
                    GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                    Exit Sub
            End Select
        Else
            Select Case My.Settings.LastLoadedServerType
                Case GV.EModule.Classic.ToString
                    host = My.Settings.ApacheClassicExtHost
                    port = My.Settings.ApacheClassicExtPort
                Case GV.EModule.Tbc.ToString
                    host = My.Settings.ApacheClassicExtHost
                    port = My.Settings.ApacheClassicExtPort
                Case GV.EModule.Wotlk.ToString
                    host = My.Settings.ApacheClassicExtHost
                    port = My.Settings.ApacheClassicExtPort
                Case Else
                    ' Неизвестный модуль
                    GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                    Exit Sub
            End Select
        End If
        If host = "" Or port = "" Then Exit Sub
        If host = "ANY" Then host = "127.0.0.1"
        Dim tcpClient = New Net.Sockets.TcpClient
        Dim ac = tcpClient.BeginConnect(host, CInt(port), Nothing, Nothing)
        Try
            If Not ac.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), False) Then
                tcpClient.Close()
                If Me.Visible Then
                    _apacheON = False
                    TSSL_Apache.GetCurrentParent().Invoke(Sub()
                                                              TSSL_Apache.Image = My.Resources.red_ball
                                                          End Sub)
                End If
            Else
                _apacheON = True
                tcpClient.EndConnect(ac)
                tcpClient.Close()
                If Me.Visible Then
                    TSSL_Apache.GetCurrentParent().Invoke(Sub()
                                                              TSSL_Apache.Image = My.Resources.green_ball
                                                          End Sub)
                End If
            End If
        Catch ex As Exception
            _apacheON = False
            If Me.Visible Then
                TSSL_Apache.GetCurrentParent().Invoke(Sub()
                                                          TSSL_Apache.Image = My.Resources.red_ball
                                                      End Sub)
            End If
            GV.Log.WriteException(ex)
        End Try
        ac.AsyncWaitHandle.Close()
    End Sub

#End Region

#Region " === СЕРВЕР WORLD === "

    ''' <summary>
    ''' Запускает сервер мира...
    ''' </summary>
    Friend Sub StartWorld(obj As Object)
        SyncLock lockWorld
            If Not _worldON Then
                If _MySqlON And Not _NeedExitLauncher And Not NeedServerStop And IsNothing(_WorldProcess) Then
                    GV.Log.WriteInfo(My.Resources.World001_WorldStart)

                    ' Исключаем повторный запуск World
                    _worldON = True

                    Dim login As String = ""
                    Dim world As String = ""
                    Dim characters As String = ""
                    Dim logs As String = ""
                    Dim playerbots As String = ""
                    If My.Settings.UseIntMySQL Then
                        Select Case My.Settings.LastLoadedServerType
                            Case GV.EModule.Classic.ToString
                                Dim base As String = Chr(34) & My.Settings.MySqlClassicIntHost & ";" &
                                    My.Settings.MySqlClassicIntPort & ";" &
                                    My.Settings.MySqlClassicIntUserName & ";" &
                                    My.Settings.MySqlClassicIntPassword & ";"
                                login &= base & My.Settings.MySqlClassicIntRealmd & Chr(34)
                                world &= base & My.Settings.MySqlClassicIntMangos & Chr(34)
                                characters &= base & My.Settings.MySqlClassicIntCharacters & Chr(34)
                                logs &= base & My.Settings.MySqlClassicIntLogs & Chr(34)
                                playerbots &= base & My.Settings.MySqlClassicIntPlayerbots & Chr(34)
                            Case GV.EModule.Tbc.ToString
                                Dim base As String = Chr(34) & My.Settings.MySqlClassicIntHost & ";" &
                                    My.Settings.MySqlTbcIntPort & ";" &
                                    My.Settings.MySqlTbcIntUserName & ";" &
                                    My.Settings.MySqlTbcIntPassword & ";"
                                login &= base & My.Settings.MySqlTbcIntRealmd & Chr(34)
                                world &= base & My.Settings.MySqlTbcIntMangos & Chr(34)
                                characters &= base & My.Settings.MySqlTbcIntCharacters & Chr(34)
                                logs &= base & My.Settings.MySqlTbcIntLogs & Chr(34)
                                playerbots &= base & My.Settings.MySqlTbcIntPlayerbots & Chr(34)
                            Case GV.EModule.Wotlk.ToString
                                Dim base As String = Chr(34) & My.Settings.MySqlWotlkIntHost & ";" &
                                    My.Settings.MySqlWotlkIntPort & ";" &
                                    My.Settings.MySqlWotlkIntUserName & ";" &
                                    My.Settings.MySqlWotlkIntPassword & ";"
                                login &= base & My.Settings.MySqlWotlkIntRealmd & Chr(34)
                                world &= base & My.Settings.MySqlWotlkIntMangos & Chr(34)
                                characters &= base & My.Settings.MySqlWotlkIntCharacters & Chr(34)
                                logs &= base & My.Settings.MySqlWotlkIntLogs & Chr(34)
                                playerbots &= base & My.Settings.MySqlWotlkIntPlayerbots & Chr(34)
                            Case Else
                                ' Неизвестный модуль
                                GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                                Exit Sub
                        End Select
                    Else
                        Select Case My.Settings.LastLoadedServerType
                            Case GV.EModule.Classic.ToString
                                Dim base As String = Chr(34) & My.Settings.MySqlClassicExtHost & ";" &
                                    My.Settings.MySqlClassicExtPort & ";" &
                                    My.Settings.MySqlClassicExtUserName & ";" &
                                    My.Settings.MySqlClassicExtPassword & ";"
                                login &= base & My.Settings.MySqlClassicExtRealmd & Chr(34)
                                world &= base & My.Settings.MySqlClassicExtMangos & Chr(34)
                                characters &= base & My.Settings.MySqlClassicExtCharacters & Chr(34)
                                logs &= base & My.Settings.MySqlClassicExtLogs & Chr(34)
                                playerbots &= base & My.Settings.MySqlClassicExtPlayerbots & Chr(34)
                            Case GV.EModule.Tbc.ToString
                                Dim base As String = Chr(34) & My.Settings.MySqlTbcExtHost & ";" &
                                    My.Settings.MySqlTbcExtPort & ";" &
                                    My.Settings.MySqlTbcExtUserName & ";" &
                                    My.Settings.MySqlTbcExtPassword & ";"
                                login &= base & My.Settings.MySqlTbcExtRealmd & Chr(34)
                                world &= base & My.Settings.MySqlTbcExtMangos & Chr(34)
                                characters &= base & My.Settings.MySqlTbcExtCharacters & Chr(34)
                                logs &= base & My.Settings.MySqlTbcExtLogs & Chr(34)
                                playerbots &= base & My.Settings.MySqlTbcExtPlayerbots & Chr(34)
                            Case GV.EModule.Wotlk.ToString
                                Dim base As String = Chr(34) & My.Settings.MySqlWotlkExtHost & ";" &
                                    My.Settings.MySqlWotlkExtPort & ";" &
                                    My.Settings.MySqlWotlkExtUserName & ";" &
                                    My.Settings.MySqlWotlkExtPassword & ";"
                                login &= base & My.Settings.MySqlWotlkExtRealmd & Chr(34)
                                world &= base & My.Settings.MySqlWotlkExtMangos & Chr(34)
                                characters &= base & My.Settings.MySqlWotlkExtCharacters & Chr(34)
                                logs &= base & My.Settings.MySqlWotlkExtLogs & Chr(34)
                                playerbots &= base & My.Settings.MySqlWotlkExtPlayerbots & Chr(34)
                            Case Else
                                ' Неизвестный модуль
                                GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                                Exit Sub
                        End Select
                    End If

                    ' Правим файл конфигурации World
                    _iniWorld.Write("MangosdConf", "LoginDatabaseInfo", login)
                    _iniWorld.Write("MangosdConf", "WorldDatabaseInfo", world)
                    _iniWorld.Write("MangosdConf", "CharacterDatabaseInfo", characters)
                    _iniWorld.Write("MangosdConf", "LogsDatabaseInfo", logs)
                    _iniWorld.Write("MangosdConf", "PlayerbotDatabaseInfo", playerbots)

                    ' Создаём информацию о процессе
                    Dim startInfo = New ProcessStartInfo(My.Settings.CurrentFileWorld) With {
                        .CreateNoWindow = True,
                        .RedirectStandardInput = True,
                        .RedirectStandardOutput = True,
                        .RedirectStandardError = True,
                        .UseShellExecute = False,
                        .WindowStyle = ProcessWindowStyle.Normal,
                        .WorkingDirectory = My.Settings.CurrentServerSettings
                    }

                    _WorldProcess = New Process()
                    Try
                        _WorldProcess.StartInfo = startInfo
                        BP.WasLaunched(GV.EProcess.World)
                        ' Запускаем
                        If _WorldProcess.Start() Then
                            GV.Log.WriteInfo(My.Resources.World002_WorldStarted)
                            AddHandler _WorldProcess.OutputDataReceived, AddressOf WorldOutputDataReceived
                            AddHandler _WorldProcess.ErrorDataReceived, AddressOf WorldErrorDataReceived
                            AddHandler _WorldProcess.Exited, AddressOf WorldExited
                            _WorldProcess.BeginOutputReadLine()
                            _WorldProcess.BeginErrorReadLine()
                        Else
                            _WorldProcess = Nothing
                        End If
                    Catch ex As Exception
                        ' World выдал исключение
                        _worldON = False
                        Try
                            _WorldProcess.Dispose()
                        Catch
                        End Try
                        GV.Log.WriteException(ex)
                        MessageBox.Show(My.Resources.E014_WorldException & vbLf & ex.Message,
                                        My.Resources.E003_ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                Else
                    TimerStartWorld.Change(2000, 2000)
                    GV.Log.WriteWarning(My.Resources.P035_ThereIsProblems)
                End If
            Else
                GV.Log.WriteInfo(String.Format(My.Resources.P041_AlreadyStarted, "World"))
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Выход из world.exe
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Friend Sub WorldExited(ByVal sender As Object, ByVal e As EventArgs)
        GV.Log.WriteInfo(My.Resources.World003_WorldStopped)
        RemoveHandler _WorldProcess.OutputDataReceived, AddressOf WorldOutputDataReceived
        RemoveHandler _WorldProcess.ErrorDataReceived, AddressOf WorldErrorDataReceived
        RemoveHandler _WorldProcess.Exited, AddressOf RealmdExited
    End Sub

    ''' <summary>
    ''' Останавливает сервер World.
    ''' </summary>
    ''' <param name="otherServers">Выключить так же и все прочие сервера.</param>
    Friend Sub ShutdownWorld(otherServers As Boolean)
        Dim processes = GetAllProcesses()
        Dim pc = processes.FindAll(Function(p) p.ProcessName = "mangosd")
        Dim id = 0

        ' Если серверы заблокированы но надо выйти из приложения - не гасим серверы WoW!
        If Not (_serversLOCKED And _NeedExitLauncher) Then
            If pc.Count > 0 Then
                For Each process In pc
                    Try
                        If process.MainModule.FileName = My.Settings.CurrentFileWorld Then
                            id = process.Id
                            UpdateWorldConsole(vbCrLf & My.Resources.P020_NeedServerStop & vbCrLf, CONSOLE)
                            ' Выполняем автосохранение БД
                            AutoSave()
                            If _worldON Then
                                ' Сервер нас слышит, поэтому отправляем saveall и server shutdown  согласно требованиям разработчиков
                                SendCommandToWorld(".saveall")
                                SendCommandToWorld("server shutdown 0")
                            Else
                                ' Сервер не готов нас слушать, поэтому удаляем хандлеры и киллим процесс world
                                If Not IsNothing(_WorldProcess) Then WorldExited(Me, Nothing)
                                Try
                                    process.Kill()
                                    _worldON = False
                                    _WorldProcess = Nothing

                                    ' Выключаем Realmd
                                    ShutdownRealmd()

                                Catch
                                End Try
                            End If
                        End If
                    Catch
                        ' Нет доступа.
                    End Try
                Next
            Else
                _worldON = False
                ' Удалям хандлеры, если таковые были
                If Not IsNothing(_WorldProcess) Then WorldExited(Me, Nothing)
                _WorldProcess = Nothing

                ' Выключаем Realmd
                ShutdownRealmd()
            End If
        End If

        ' Завершаем остановку сервера World в потоке
        Dim sw As New Threading.Thread(Sub() StoppingWorld(id, otherServers)) With {
            .CurrentUICulture = GV.CI,
            .CurrentCulture = GV.CI,
            .IsBackground = True
        }
        sw.Start()
    End Sub

    ''' <summary>
    ''' Проверяет доступность сервера World.
    ''' </summary>
    Friend Sub CheckWorld()
        Dim host = _iniWorld.ReadString("MangosdConf", "BindIP", "127.0.0.1")
        If host = "0.0.0.0" Then host = "127.0.0.1"
        Dim port = _iniWorld.ReadString("MangosdConf", "WorldServerPort", "8085")
        Dim tcpClient = New Net.Sockets.TcpClient
        Dim ac = tcpClient.BeginConnect(host, CInt(port), Nothing, Nothing)

        Try
            If Not ac.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), False) Then
                tcpClient.Close()
                _worldON = False
                If Me.Visible Then
                    TSSL_World.GetCurrentParent().Invoke(Sub()
                                                             TSSL_World.Image = My.Resources.red_ball
                                                             If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                 Me.Icon = My.Resources.cmangos_red
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                                                             Else
                                                                 Me.Icon = My.Resources.wow
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                             End If
                                                         End Sub)
                Else
                    Try
                        ' Меняем иконку в трее, коли свёрнуты
                        If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                            Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                        Else
                            Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                        End If
                    Catch
                    End Try
                End If
            Else
                tcpClient.EndConnect(ac)
                tcpClient.Close()
                _worldON = True
                If Me.Visible Then
                    TSSL_World.GetCurrentParent().Invoke(Sub()
                                                             TSSL_World.Image = My.Resources.green_ball
                                                             If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                 Select Case My.Settings.LastLoadedServerType
                                                                     Case GV.EModule.Classic.ToString
                                                                         Me.Icon = My.Resources.cmangos_classic
                                                                         Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_classic
                                                                     Case GV.EModule.Tbc.ToString
                                                                         Me.Icon = My.Resources.cmangos_tbc
                                                                         Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_tbc
                                                                     Case GV.EModule.Wotlk.ToString
                                                                         Me.Icon = My.Resources.cmangos_wotlk
                                                                         Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_wotlk
                                                                 End Select
                                                             Else
                                                                 Me.Icon = My.Resources.wow
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                             End If
                                                         End Sub)

                Else
                    Try
                        ' Меняем иконку в трее, коли свёрнуты
                        If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                            Select Case My.Settings.LastLoadedServerType
                                Case GV.EModule.Classic.ToString
                                    Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_classic
                                Case GV.EModule.Tbc.ToString
                                    Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_tbc
                                Case GV.EModule.Wotlk.ToString
                                    Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_wotlk
                            End Select
                        Else
                            Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                        End If
                    Catch
                    End Try
                End If
            End If
        Catch ex As Exception
            If Me.Visible Then
                _worldON = False
                TSSL_Realm.GetCurrentParent().Invoke(Sub()
                                                         TSSL_World.Image = My.Resources.red_ball
                                                         If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                             Me.Icon = My.Resources.cmangos_orange
                                                             Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_orange
                                                         Else
                                                             Me.Icon = My.Resources.wow
                                                             Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                         End If
                                                     End Sub)
            Else
                Try
                    ' Меняем иконку в трее, коли свёрнуты
                    If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                        Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                    Else
                        Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                    End If
                Catch
                End Try
            End If
            GV.Log.WriteException(ex)
        End Try
        ac.AsyncWaitHandle.Close()
    End Sub

#End Region

#Region " === СЕРВЕР REALMD === "

    ''' <summary>
    ''' Запускает сервер авторизации.
    ''' </summary>
    Friend Sub StartRealmd(ob As Object)
        SyncLock lockRealmd
            If Not _RealmdON Then
                If _MySqlON And Not NeedServerStop And IsNothing(_RealmdProcess) Then
                    GV.Log.WriteInfo(My.Resources.Real001_RealmdStart)

                    ' Исключаем повторный запуск Realmd
                    _RealmdON = True

                    Dim value As String = ""
                    If My.Settings.UseIntMySQL Then
                        Select Case My.Settings.LastLoadedServerType
                            Case GV.EModule.Classic.ToString
                                value = Chr(34) & My.Settings.MySqlClassicIntHost & ";" &
                                        My.Settings.MySqlClassicIntPort & ";" &
                                        My.Settings.MySqlClassicIntUserName & ";" &
                                        My.Settings.MySqlClassicIntPassword & ";" &
                                        My.Settings.MySqlClassicIntRealmd & Chr(34)
                            Case GV.EModule.Tbc.ToString
                                value = Chr(34) & My.Settings.MySqlTbcIntHost & ";" &
                                        My.Settings.MySqlTbcIntPort & ";" &
                                        My.Settings.MySqlTbcIntUserName & ";" &
                                        My.Settings.MySqlTbcIntPassword & ";" &
                                        My.Settings.MySqlTbcIntRealmd & Chr(34)
                            Case GV.EModule.Wotlk.ToString
                                value = Chr(34) & My.Settings.MySqlWotlkIntHost & ";" &
                                        My.Settings.MySqlWotlkIntPort & ";" &
                                        My.Settings.MySqlWotlkIntUserName & ";" &
                                        My.Settings.MySqlWotlkIntPassword & ";" &
                                        My.Settings.MySqlWotlkIntRealmd & Chr(34)
                            Case Else
                                ' Неизвестный модуль
                                GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                                Exit Sub
                        End Select
                    Else
                        Select Case My.Settings.LastLoadedServerType
                            Case GV.EModule.Classic.ToString
                                value = Chr(34) & My.Settings.MySqlClassicExtHost & ";" &
                                        My.Settings.MySqlClassicExtPort & ";" &
                                        My.Settings.MySqlClassicExtUserName & ";" &
                                        My.Settings.MySqlClassicExtPassword & ";" &
                                        My.Settings.MySqlClassicExtRealmd & Chr(34)
                            Case GV.EModule.Tbc.ToString
                                value = Chr(34) & My.Settings.MySqlTbcExtHost & ";" &
                                        My.Settings.MySqlTbcExtPort & ";" &
                                        My.Settings.MySqlTbcExtUserName & ";" &
                                        My.Settings.MySqlTbcExtPassword & ";" &
                                        My.Settings.MySqlTbcExtRealmd & Chr(34)
                            Case GV.EModule.Wotlk.ToString
                                value = Chr(34) & My.Settings.MySqlWotlkExtHost & ";" &
                                        My.Settings.MySqlWotlkExtPort & ";" &
                                        My.Settings.MySqlWotlkExtUserName & ";" &
                                        My.Settings.MySqlWotlkExtPassword & ";" &
                                        My.Settings.MySqlWotlkExtRealmd & Chr(34)
                            Case Else
                                ' Неизвестный модуль
                                GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
                                Exit Sub
                        End Select
                    End If

                    ' Правим файл конфигурации Realmd
                    _iniRealmd.Write("RealmdConf", "LoginDatabaseInfo", value)

                    ' Создаём информацию о процессе
                    Dim startInfo = New ProcessStartInfo(My.Settings.CurrentFileRealmd) With {
                            .CreateNoWindow = True,
                            .RedirectStandardInput = True,
                            .RedirectStandardOutput = True,
                            .RedirectStandardError = True,
                            .UseShellExecute = False,
                            .WindowStyle = ProcessWindowStyle.Normal,
                            .WorkingDirectory = My.Settings.CurrentServerSettings
                        }

                    _RealmdProcess = New Process()

                    Try
                        _RealmdProcess.StartInfo = startInfo
                        BP.WasLaunched(GV.EProcess.Realmd)
                        ' Запускаем
                        If _RealmdProcess.Start() Then
                            GV.Log.WriteInfo(My.Resources.Real002_RealmdStarted)
                            AddHandler _RealmdProcess.OutputDataReceived, AddressOf RealmdOutputDataReceived
                            AddHandler _RealmdProcess.ErrorDataReceived, AddressOf RealmdErrorDataReceived
                            AddHandler _RealmdProcess.Exited, AddressOf RealmdExited
                            _RealmdProcess.BeginOutputReadLine()
                            _RealmdProcess.BeginErrorReadLine()
                        Else
                            _RealmdProcess = Nothing
                        End If
                    Catch ex As Exception
                        ' Realmd выдал исключение
                        _RealmdON = False
                        Try
                            _RealmdProcess.Dispose()
                        Catch
                        End Try
                        GV.Log.WriteException(ex)
                        MessageBox.Show(My.Resources.E012_RealmdException & vbLf & ex.Message,
                                            My.Resources.E003_ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            Else
                GV.Log.WriteInfo(String.Format(My.Resources.P041_AlreadyStarted, "Realmd"))
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Выход из realmd.exe
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub RealmdExited(ByVal sender As Object, ByVal e As EventArgs)
        GV.Log.WriteInfo(My.Resources.Real003_RealmdStopped)
        RemoveHandler _RealmdProcess.OutputDataReceived, AddressOf RealmdOutputDataReceived
        RemoveHandler _RealmdProcess.ErrorDataReceived, AddressOf RealmdErrorDataReceived
        RemoveHandler _RealmdProcess.Exited, AddressOf RealmdExited
    End Sub

    ''' <summary>
    ''' Останавливает сервер Realmd.
    ''' </summary>
    Friend Sub ShutdownRealmd()
        Dim processes = GetAllProcesses()
        Dim pc = processes.FindAll(Function(p) p.ProcessName = "realmd")
        If pc.Count > 0 Then
            For Each process In pc
                Try
                    If process.MainModule.FileName = My.Settings.CurrentFileRealmd Then
                        Try
                            process.Kill()
                            Thread.Sleep(100)
                            GV.Log.WriteInfo(My.Resources.P020_NeedServerStop)
                            UpdateRealmdConsole(My.Resources.P020_NeedServerStop, CONSOLE)
                        Catch
                        End Try
                    End If
                Catch
                    ' Нет доступа.
                End Try
            Next
        Else
        End If
        _RealmdON = False
        ' Удаляем хандлеры, если таковые были
        If Not IsNothing(_RealmdProcess) Then RealmdExited(Me, Nothing)
        _RealmdProcess = Nothing
    End Sub

    ''' <summary>
    ''' Проверка доступности сервера Realmd.
    ''' </summary>
    Friend Sub CheckRealmd()
        Try
            ' Сначала проверяем наличие процесса
            Dim host = _iniRealmd.ReadString("RealmdConf", "BindIP", "127.0.0.1")
            If host = "0.0.0.0" Then host = "127.0.0.1"
            Dim port = _iniRealmd.ReadString("RealmdConf", "RealmServerPort", "3724")
            Dim tcpClient = New Net.Sockets.TcpClient
            Dim ac = tcpClient.BeginConnect(host, CInt(port), Nothing, Nothing)

            Try
                If Not ac.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), False) Then
                    tcpClient.Close()
                    _RealmdON = False
                    If Me.Visible Then
                        TSSL_Realm.GetCurrentParent().Invoke(Sub()
                                                                 TSSL_Realm.Image = My.Resources.red_ball
                                                                 If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                     Me.Icon = My.Resources.cmangos_red
                                                                     Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                                                                 Else
                                                                     Me.Icon = My.Resources.wow
                                                                     Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                                 End If
                                                             End Sub)
                    Else
                        Try
                            ' Меняем иконку в трее, коли свёрнуты
                            If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                            Else
                                Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                            End If
                        Catch
                        End Try
                    End If
                Else
                    tcpClient.EndConnect(ac)
                    tcpClient.Close()
                    _RealmdON = True
                    If Me.Visible Then
                        TSSL_Realm.GetCurrentParent().Invoke(Sub()
                                                                 TSSL_Realm.Image = My.Resources.green_ball
                                                                 If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                     If Not _worldON Then
                                                                         Me.Icon = My.Resources.cmangos_realmd_started
                                                                         Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_realmd_started
                                                                     End If
                                                                 Else
                                                                     Me.Icon = My.Resources.wow
                                                                     Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                                 End If
                                                             End Sub)
                    Else
                        If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                            If Not _worldON Then
                                Try
                                    ' Меняем иконку в трее, коли свёрнуты
                                    Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_realmd_started
                                Catch
                                End Try
                            End If
                        Else
                            Try
                                ' Меняем иконку в трее, коли свёрнуты
                                Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                            Catch
                            End Try
                        End If
                    End If
                End If
            Catch ex As Exception
                If Me.Visible Then
                    _RealmdON = False
                    TSSL_Realm.GetCurrentParent().Invoke(Sub()
                                                             TSSL_Realm.Image = My.Resources.red_ball
                                                             If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                                                                 Me.Icon = My.Resources.cmangos_orange
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_orange
                                                             Else
                                                                 Me.Icon = My.Resources.wow
                                                                 Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                                                             End If
                                                         End Sub)
                Else
                    Try
                        If Not NeedServerStop AndAlso Me.ServerWowAutostart Or _isLastCommandStart Then
                            ' Меняем иконку в трее, коли свёрнуты
                            Me.NotifyIcon_SPP2.Icon = My.Resources.cmangos_red
                        Else
                            Me.NotifyIcon_SPP2.Icon = My.Resources.wow
                        End If
                    Catch
                    End Try
                End If
                GV.Log.WriteException(ex)
            End Try
            ac.AsyncWaitHandle.Close()
        Catch ex As Exception
            GV.Log.WriteException(ex)
            MessageBox.Show(ex.Message,
                            My.Resources.E003_ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region " === ЯРЛЫК И МЕНЮ В ТРЕЕ === "

    ''' <summary>
    ''' МЕНЮ - ОТКРЫТЬ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_OpenLauncher_Click(sender As Object, e As EventArgs) Handles TSMI_OpenLauncher.Click
        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub

    ''' <summary>
    ''' МЕНЮ - ЗАКРЫТЬ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_CloseLauncher_Click(sender As Object, e As EventArgs) Handles TSMI_CloseLauncher.Click
        _NeedExitLauncher = True
        ShutdownAll(True)
    End Sub

    ''' <summary>
    ''' ДВОЙНОЙ КЛИК МЫШКОЙ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon_SPP2.MouseDoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
    End Sub

#End Region

#Region " === МЕНЮ СЕРВЕР === "

    ''' <summary>
    ''' МЕНЮ - ЗАПУСК СЕРВЕРА
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ServerStart_Click(sender As Object, e As EventArgs) Handles TSMI_ServerStart.Click
        _isLastCommandStart = True
        _NeedServerStop = False
        RichTextBox_ConsoleRealmd.Clear()
        RichTextBox_ConsoleWorld.Clear()
        If ServerWowAutostart Then
            GV.SPP2Launcher.UpdateRealmdConsole(My.Resources.P019_ControlEnabled, CONSOLE)
            GV.SPP2Launcher.UpdateWorldConsole(My.Resources.P019_ControlEnabled, CONSOLE)
        End If
        ServerStart()
    End Sub

    ''' <summary>
    ''' МЕНЮ - ОСТАНОВКА СЕРВЕРА
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_ServerStop_Click(sender As Object, e As EventArgs) Handles TSMI_ServerStop.Click
        _isLastCommandStart = False
        _NeedServerStop = True
        _needServerStart = False
        _NeedExitLauncher = False
        ShutdownWorld(False)
    End Sub

    ''' <summary>
    ''' Изменяет автозапуск сервера WoW.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_WowAutoStart_Click(sender As Object, e As EventArgs) Handles TSMI_WowAutoStart.Click
        Select Case My.Settings.LastLoadedServerType
            Case GV.EModule.Classic.ToString
                My.Settings.ServerClassicAutostart = Not ServerWowAutostart
            Case GV.EModule.Tbc.ToString
                My.Settings.ServerTbcAutostart = Not ServerWowAutostart
            Case GV.EModule.Wotlk.ToString
                My.Settings.ServerWotlkAutostart = Not ServerWowAutostart
            Case Else
                ' Неизвестный модуль
                GV.Log.WriteInfo(My.Resources.E008_UnknownModule)
        End Select
        _ServerWowAutostart = Not ServerWowAutostart
        TSMI_WowAutoStart.Checked = ServerWowAutostart
        My.Settings.Save()
    End Sub

    ''' <summary>
    ''' Запускает сервер WOW.
    ''' </summary>
    Friend Sub ServerStart()
        _NeedExitLauncher = False
        ' Сначала запускаем MySQL
        StartMySQL(Nothing)
        _needServerStart = True
    End Sub

    ''' <summary>
    ''' Глобальная остановка всего и вся.
    ''' </summary>
    ''' <param name="shutdown">Завершить работу лаунчера после остановки серверов.</param>
    Friend Sub ShutdownAll(shutdown As Boolean)
        GV.Log.WriteInfo(My.Resources.P040_CommandShutdown)
        _NeedExitLauncher = shutdown
        ' Запускаем остановку всех серверов 
        ShutdownWorld(True)
    End Sub

#End Region

#Region " === КОНСОЛЬ === "

    ''' <summary>
    ''' ПРИ НАЖАТИИ КЛАВИШ В КОМАНДНОЙ СТРОКЕ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TextBox_Command_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox_Command.KeyDown
        If Not Me.TextBox_Command.IsHandleCreated Then Return

        Select Case e.KeyData

            Case Keys.Enter
                e.Handled = True
                e.SuppressKeyPress = True
                If TabControl1.SelectedTab.Name = "TabPage_MySQL" Then
                    If Not IsNothing(_mysqlProcess) Then _mysqlProcess.StandardInput.WriteLine(Me.TextBox_Command.Text)
                ElseIf TabControl1.SelectedTab.Name = "TabPage_Realmd" Then
                    If Not IsNothing(_RealmdProcess) Then _RealmdProcess.StandardInput.WriteLine(Me.TextBox_Command.Text)
                ElseIf TabControl1.SelectedTab.Name = "TabPage_World" Then
                    If Me.TextBox_Command.Text <> "" Then
                        SendCommandToWorld(Me.TextBox_Command.Text)
                    End If
                End If
                Me.TextBox_Command.Text = ""

            Case Keys.Up
                If My.Settings.UseConsoleBuffer Then
                    Dim text = ConsoleCommandBuffer.GetUp
                    If Not IsNothing(text) OrElse text <> "" Then
                        TextBox_Command.Text = text
                        TextBox_Command.Select(text.Length, text.Length)
                    End If
                End If

            Case Keys.Down
                If My.Settings.UseConsoleBuffer Then
                    Dim text = ConsoleCommandBuffer.GetDown
                    If Not IsNothing(text) OrElse text <> "" Then
                        TextBox_Command.Text = text
                        TextBox_Command.Select(text.Length, text.Length)
                    End If
                End If

            Case Keys.Escape
                Me.TextBox_Command.Text = ""

        End Select
    End Sub

    ''' <summary>
    ''' Отправляет команду в сервер World.
    ''' </summary>
    ''' <param name="text"></param>
    Private Sub SendCommandToWorld(text As String)

        ' Если буфер разрешён, добавляем текст в буфер
        If My.Settings.UseConsoleBuffer Then ConsoleCommandBuffer.Add(text)

        Dim t = String.Format(My.Resources.World005_YourSendCommand, text)
        GV.Log.WriteInfo(t)
        OutWorldConsole(t, CONSOLE)
        If Not IsNothing(_WorldProcess) Then
            _WorldProcess.StandardInput.WriteLine(text)
            If Not _worldON Then
                OutWorldConsole(My.Resources.P037_WorldNotStarted, CONSOLE)
            End If
        Else
            OutWorldConsole(My.Resources.P037_WorldNotStarted, CONSOLE)
        End If
    End Sub

    ''' <summary>
    ''' Вывод сообщения в консоль сервера World.
    ''' </summary>
    ''' <param name="text">Текст для вывода.</param>
    Friend Function UpdateWorldConsole(text As String, color As Drawing.Color) As String
        If Not IsNothing(text) AndAlso Me.Visible = True Then
            If Me.RichTextBox_ConsoleWorld.InvokeRequired Then
                Me.RichTextBox_ConsoleWorld.Invoke(Sub()
                                                       OutWorldConsole(text, color)
                                                   End Sub)
            Else
                OutWorldConsole(text, color)
            End If
        End If
        Return text
    End Function

    Dim oldMessage As String = ""
    Dim holdline As Integer

    ''' <summary>
    ''' Прямой вывод в консоль World.
    ''' </summary>
    ''' <param name="text"></param>
    Private Sub OutWorldConsole(text As String, color As Drawing.Color)
        Select Case RichTextBox_ConsoleWorld.Lines.Count
            Case 0
                RichTextBox_ConsoleWorld.SelectionColor = color
                RichTextBox_ConsoleWorld.AppendText(text)
                RichTextBox_ConsoleWorld.ScrollToCaret()
            Case 500
                ' Не более 500 строк в окне
                Dim str = RichTextBox_ConsoleWorld.Lines(0)
                RichTextBox_ConsoleWorld.Select(0, str.Length + 1)
                RichTextBox_ConsoleWorld.ReadOnly = False
                RichTextBox_ConsoleWorld.SelectedText = String.Empty
                RichTextBox_ConsoleWorld.ReadOnly = True
                RichTextBox_ConsoleWorld.AppendText(vbCrLf)
                RichTextBox_ConsoleWorld.SelectionColor = color
                RichTextBox_ConsoleWorld.AppendText(text)
                RichTextBox_ConsoleWorld.ScrollToCaret()
            Case Else
                RichTextBox_ConsoleWorld.AppendText(vbCrLf)
                RichTextBox_ConsoleWorld.SelectionColor = color
                RichTextBox_ConsoleWorld.AppendText(text)
                RichTextBox_ConsoleWorld.ScrollToCaret()
        End Select
        oldMessage = text
        If oldMessage = "=" Then
            holdline = RichTextBox_ConsoleWorld.Lines.Count
        End If
        RichTextBox_ConsoleWorld.Select(RichTextBox_ConsoleWorld.GetFirstCharIndexOfCurrentLine(), 0)
        If text.Contains("mangos>Halting process") Or text.Contains(My.Resources.E016_WorldCrashed) Then
            ' Сервер World остановился. Удаляем хандлеры
            If Not IsNothing(_WorldProcess) Then WorldExited(Me, Nothing)
            WorldProcess = Nothing
            _worldON = False
        End If
    End Sub

    ''' <summary>
    ''' Вывод сообщения в консоль сервера MySQL с учётом Invoke.
    ''' </summary>
    ''' <param name="text"></param>
    Friend Function UpdateMySQLConsole(ByVal text As String, ByVal ink As Drawing.Color) As String
        Dim msg As String = ""
        If Not IsNothing(text) And Me.Visible = True Then
            If ink.A = 0 And ink.R = 0 And ink.G = 0 And ink.B = 0 Then
                If text.Contains("Got an error reading communication packets") Then Return text
                If text.Contains("[Warning]") Then
                    msg = Mid(text, 31)
                    ink = Color.Red
                ElseIf text.Contains("[ERROR]") Then
                    msg = Mid(text, 31)
                    ink = Color.Red
                ElseIf text.Contains("[Note]") Then
                    msg = Mid(text, 31)
                    ink = Color.DarkOrange
                ElseIf text.Contains("[Console]") Then
                    msg = Mid(text, 11)
                    ink = CONSOLE
                ElseIf text.Contains("[EConsole]") Then
                    msg = Mid(text, 12)
                    ink = ECONSOLE
                ElseIf text.Contains("[WConsole]") Then
                    msg = Mid(text, 12)
                    ink = WCONSOLE
                Else
                    msg = text
                    ink = Color.YellowGreen
                End If
            Else
                msg = text
            End If
            If Me.RichTextBox_ConsoleMySQL.InvokeRequired Then
                Me.RichTextBox_ConsoleMySQL.Invoke(Sub()
                                                       OutMySqlConsole(msg, ink)
                                                   End Sub)
            Else
                OutMySqlConsole(msg, ink)
            End If
        End If
        Return text
    End Function

    ''' <summary>
    ''' Вывод в консоль MySQL только из главного потока!
    ''' </summary>
    ''' <param name="text">Текст для вывода.</param>
    ''' <param name="ink">Цвет текста.</param>
    Friend Function OutMySqlConsole(text As String, ink As Drawing.Color) As String
        If Not IsNothing(text) AndAlso Me.Visible = True Then
            Select Case RichTextBox_ConsoleMySQL.Lines.Count
                Case 0
                    RichTextBox_ConsoleMySQL.ReadOnly = False
                    RichTextBox_ConsoleMySQL.SelectedText = String.Empty
                    RichTextBox_ConsoleMySQL.SelectionColor = ink
                    RichTextBox_ConsoleMySQL.ReadOnly = True

                    RichTextBox_ConsoleMySQL.AppendText(text)
                    RichTextBox_ConsoleMySQL.ScrollToCaret()
                Case 500
                    ' Не более 500 строк в окне
                    Dim str = RichTextBox_ConsoleMySQL.Lines(0)
                    RichTextBox_ConsoleMySQL.Select(0, str.Length + 1)
                    RichTextBox_ConsoleMySQL.ReadOnly = False
                    RichTextBox_ConsoleMySQL.SelectedText = String.Empty
                    RichTextBox_ConsoleMySQL.ReadOnly = True
                    RichTextBox_ConsoleMySQL.AppendText(vbCrLf)
                    RichTextBox_ConsoleMySQL.SelectionColor = ink
                    RichTextBox_ConsoleMySQL.AppendText(text)
                    RichTextBox_ConsoleMySQL.ScrollToCaret()
                Case Else
                    RichTextBox_ConsoleMySQL.AppendText(vbCrLf)
                    RichTextBox_ConsoleMySQL.SelectionColor = ink
                    RichTextBox_ConsoleMySQL.AppendText(text)
                    RichTextBox_ConsoleMySQL.ScrollToCaret()
            End Select

        End If
        Return text
    End Function

    ''' <summary>
    ''' Вывод сообщения в консоль сервера Realmd.
    ''' </summary>
    ''' <param name="text">Текст для вывода.</param>
    Friend Sub UpdateRealmdConsole(text As String, color As Drawing.Color)
        If Not IsNothing(text) AndAlso Me.Visible = True Then
            If Me.RichTextBox_ConsoleRealmd.InvokeRequired Then
                Me.RichTextBox_ConsoleRealmd.Invoke(Sub()
                                                        OutRealmdConsole(text, color)
                                                    End Sub)
            Else
                OutRealmdConsole(text, color)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Прямой вывод в консоль Realmd
    ''' </summary>
    ''' <param name="text"></param>
    Private Sub OutRealmdConsole(text As String, color As Drawing.Color)
        If Not IsNothing(text) AndAlso Me.Visible = True Then
            Select Case RichTextBox_ConsoleRealmd.Lines.Count
                Case 0
                    RichTextBox_ConsoleRealmd.SelectionColor = color
                    RichTextBox_ConsoleRealmd.AppendText(text)
                    RichTextBox_ConsoleRealmd.ScrollToCaret()
                Case 500
                    ' Не более 500 строк в окне
                    Dim str = RichTextBox_ConsoleRealmd.Lines(0)
                    RichTextBox_ConsoleRealmd.Select(0, str.Length + 1)
                    RichTextBox_ConsoleRealmd.ReadOnly = False
                    RichTextBox_ConsoleRealmd.SelectedText = String.Empty
                    RichTextBox_ConsoleRealmd.ReadOnly = True
                    RichTextBox_ConsoleRealmd.AppendText(vbCrLf)
                    RichTextBox_ConsoleRealmd.SelectionColor = color
                    RichTextBox_ConsoleRealmd.AppendText(text)
                    RichTextBox_ConsoleRealmd.ScrollToCaret()
                Case Else
                    RichTextBox_ConsoleRealmd.AppendText(vbCrLf)
                    RichTextBox_ConsoleRealmd.SelectionColor = color
                    RichTextBox_ConsoleRealmd.AppendText(text)
                    RichTextBox_ConsoleRealmd.ScrollToCaret()
            End Select
            RichTextBox_ConsoleRealmd.Select(RichTextBox_ConsoleRealmd.GetFirstCharIndexOfCurrentLine(), 0)
            If text.Contains(My.Resources.E015_RealmdCrashed) Then
                ' Сервер Realmd рухнул. Удаляем хандлеры
                If Not IsNothing(_RealmdProcess) Then RealmdExited(Me, Nothing)
                RealmdProcess = Nothing
                _RealmdON = False
            End If
        End If
    End Sub

    ''' <summary>
    ''' КОНТЕКСТНОЕ МЕНЮ - КОПИРОВАТЬ
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TSMI_Copy_Click(sender As Object, e As EventArgs) Handles TSMI_Copy.Click
        Select Case TabControl1.SelectedTab.Name
            Case "TabPage_MySQL"
                If RichTextBox_ConsoleMySQL.SelectionLength > 0 Then
                    My.Computer.Clipboard.SetText(RichTextBox_ConsoleMySQL.SelectedText)
                Else
                    My.Computer.Clipboard.Clear()
                End If
            Case "TabPage_Realmd"
                If RichTextBox_ConsoleRealmd.SelectionLength > 0 Then
                    My.Computer.Clipboard.SetText(RichTextBox_ConsoleRealmd.SelectedText)
                Else
                    My.Computer.Clipboard.Clear()
                End If
            Case "TabPage_World"
                If RichTextBox_ConsoleWorld.SelectionLength > 0 Then
                    My.Computer.Clipboard.SetText(RichTextBox_ConsoleWorld.SelectedText)
                Else
                    My.Computer.Clipboard.Clear()
                End If
        End Select
    End Sub

    ''' <summary>
    ''' Меняет задний фон консоли.
    ''' </summary>
    Friend Sub SetConsoleTheme()
        If My.Settings.ConsoleTheme = "Black Theme" Then
            RichTextBox_ConsoleMySQL.BackColor = Color.Black
            RichTextBox_ConsoleRealmd.BackColor = Color.Black
            RichTextBox_ConsoleWorld.BackColor = Color.Black
        Else
            RichTextBox_ConsoleMySQL.BackColor = Color.White
            RichTextBox_ConsoleRealmd.BackColor = Color.White
            RichTextBox_ConsoleWorld.BackColor = Color.White
        End If
    End Sub

    ''' <summary>
    ''' Изменяет шрифт консоли.
    ''' </summary>
    Friend Sub ChangeFont()
        My.Settings.Save()

        ' Получает шрифт с текущими настройками.
        Dim fnt = GetCurrentFont()

        ' MySQL
        RichTextBox_ConsoleMySQL.SuspendLayout()
        For x = 0 To RichTextBox_ConsoleMySQL.Lines().Count
            Dim istart = RichTextBox_ConsoleMySQL.GetFirstCharIndexFromLine(x)
            Dim iend = RichTextBox_ConsoleMySQL.GetFirstCharIndexFromLine(x + 1) - 1
            If istart >= 0 Then
                RichTextBox_ConsoleMySQL.Select(istart, iend - istart)
                RichTextBox_ConsoleMySQL.SelectionFont = fnt
            End If
        Next
        RichTextBox_ConsoleMySQL.ResumeLayout()

        ' Realmd
        RichTextBox_ConsoleRealmd.SuspendLayout()
        For x = 0 To RichTextBox_ConsoleRealmd.Lines().Count
            Dim istart = RichTextBox_ConsoleRealmd.GetFirstCharIndexFromLine(x)
            Dim iend = RichTextBox_ConsoleRealmd.GetFirstCharIndexFromLine(x + 1) - 1
            If istart >= 0 Then
                RichTextBox_ConsoleRealmd.Select(istart, iend - istart)
                RichTextBox_ConsoleRealmd.SelectionFont = fnt
            End If
        Next
        RichTextBox_ConsoleRealmd.ResumeLayout()

        ' World
        RichTextBox_ConsoleWorld.SuspendLayout()
        Dim x3 = 0
        For Each line In RichTextBox_ConsoleWorld.Lines()
            Dim istart = RichTextBox_ConsoleWorld.GetFirstCharIndexFromLine(x3)
            Dim iend = istart + line.Length
            'Dim iend = RichTextBox_ConsoleWorld.GetFirstCharIndexFromLine(x + 1) - 1
            If istart >= 0 Then
                RichTextBox_ConsoleWorld.Select(istart, iend - istart)
                RichTextBox_ConsoleWorld.SelectionFont = fnt
            End If
            x3 += 1
        Next
        RichTextBox_ConsoleWorld.ResumeLayout()

        RichTextBox_ConsoleMySQL.Font = fnt
        RichTextBox_ConsoleRealmd.Font = fnt
        RichTextBox_ConsoleWorld.Font = fnt

    End Sub

#End Region

End Class
