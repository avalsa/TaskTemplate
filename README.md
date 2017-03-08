# Шаблон модуля-задания GraphLabs.

**TaskTemplate** - каркас для новых модулей-заданий на Silverlight.

## Запуск и отладка

Проект можно запускать в двух режимах: в браузере и вне браузера. Принципы запуска и отладки в двух различных режимах существенно отличаются.


**Работа в браузере**

*Запуск*

  1. В первую очередь необходимо в "Solution configurations" (на панели инструментов между кнопкой запуска и стрелочками "Undo" и "Redo") установить "DebugWithSite"
  2. Далее нужно открыть свойства проекта "GraphLabs.Tasks.Template" и в разделе "Debug" выбрать "Dynamically generate a test page". Свойства проекта "Propertis" можно найти в контекстном меню, которое открывается при нажатии правой кнопки мыши по выбранному проекту.
  3. *Внимание!* Перед первым запуском проекта в этом режиме сначала неободимо запустить проект "DebugVariantDataGenerator". Это можно сделать, выбрав в контекстом меню этого проекта по правому клику мыши "Debug"->"Start new instance"
  4. Для запуска проекта в меню Visual Studio в разделе "Debug" выбрать "Start debugging". В зависимости от выбранного по умолчанию браузера может возникнуть ошибка - см. FAQ.
  
*Отладка*

 1. Настройка графа осуществляется в файле "Program.cs", который находится в проекте "DebugVariantDataGenerator". В "Program.cs" можно задать количество вершин и связи между ними.  
 2. После каждого изменения в исходном коде "Program.cs" перед запуском проекта "GraphLabs.Tasks.Template" необходимо запускать проект "DebugVariantDataGenerator".


**Работа вне браузера**

*Запуск*

  1. Аналогично - в "Solution configurations" установить режим "Debug"
  2. В "Properties" в том же разделе "Debug" выбрать "Out-of-browser application". 
  3. После запуска проекта модуль будет отображен в отдельном окне.
  
*Отладка*

  Задать граф можно в файле "MockedWcfServicesConfigurator.cs".
  

## FAQ

* **Q**: При сборке проекта падает ошибка о том, что "выполнение сценариев отключено в этой системе" ("execution of scripts is disabled on this system").
* *A*: Необходимо разрешить выполнение Powershell-скриптов. Для этого, запустите консоль Powershell (x86) с правами администратора, и выполните "Set-ExecutionPolicy Unrestricted"


* **Q**: При сборке проекта выдаёт ошибку "Unable to attach to application '<имя браузера>.exe' (PID: XXXX) using 'DESKTOP-OB7UNSH'. The 32-bit version of the Visual Studio Remote Debugger (MSVSMON.EXE) cannot be used to debug 64-bit processes or 64-bit dumps. Please use the 64-bit version instead."
* *A*: Необходимо изменить в настройках системы браузер по умолчанию. Важно, чтобы это был 32-битный браузер, например Internet Explorer.
