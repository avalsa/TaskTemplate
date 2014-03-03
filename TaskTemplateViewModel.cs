using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using GraphLabs.Common;
using GraphLabs.Common.UserActionsRegistrator;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;
using GraphLabs.Utils;
using GraphLabs.Utils.Services;

namespace GraphLabs.Tasks.Template
{
    /// <summary> ViewModel для TaskTemplate </summary>
    public partial class TaskTemplateViewModel : DependencyObject
    {
        /// <summary> Текущее состояние </summary>
        private enum State
        {
            /// <summary> Пусто </summary>
            Nothing,
            /// <summary> Перемещение вершин </summary>
            MoveVertex,
        }

        /// <summary> Текущее состояние </summary>
        private State _state;

        /// <summary> Поставщик варианта </summary>
        private readonly VariantProvider _variantProvider;

        /// <summary> ID текущего задания </summary>
        private readonly long _taskId;

        /// <summary> Guid текущей сессии </summary>
        private readonly Guid _sessionGuid;

        /// <summary> Допустимые версии генератора, с помощью которого сгенерирован вариант </summary>
        private readonly Version[] _allowedGeneratorVersions = new[] {  new Version(1, 0) };


        #region Public свойства вьюмодели

        /// <summary> Регистратор действий студента </summary>
        public static readonly DependencyProperty UserActionsManagerProperty =
            DependencyProperty.Register("UserActionsManager", typeof(UserActionsManager), typeof(TaskTemplateViewModel), new PropertyMetadata(default(UserActionsManager)));

        /// <summary> Регистратор действий студента </summary>
        public UserActionsManager UserActionsManager
        {
            get { return (UserActionsManager)GetValue(UserActionsManagerProperty); }
            set { SetValue(UserActionsManagerProperty, value); }
        }

        /// <summary> Идёт загрузка данных? </summary>
        public static readonly DependencyProperty IsLoadingDataProperty = DependencyProperty.Register(
            "IsLoadingData", 
            typeof(bool), 
            typeof(TaskTemplateViewModel), 
            new PropertyMetadata(false));

        /// <summary> Идёт загрузка данных? </summary>
        public bool IsLoadingData
        {
            get { return (bool)GetValue(IsLoadingDataProperty); }
            private set { SetValue(IsLoadingDataProperty, value); }
        }

        /// <summary> Разрешено перемещение вершин? </summary>
        public static readonly DependencyProperty IsMouseVerticesMovingEnabledProperty = DependencyProperty.Register(
            "IsMouseVerticesMovingEnabled", 
            typeof(bool), 
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(false));

        /// <summary> Разрешено перемещение вершин? </summary>
        public bool IsMouseVerticesMovingEnabled
        {
            get { return (bool)GetValue(IsMouseVerticesMovingEnabledProperty); }
            set { SetValue(IsMouseVerticesMovingEnabledProperty, value); }
        }


        /// <summary> Команды панели инструментов</summary>
        public static readonly DependencyProperty ToolBarCommandsProperty = DependencyProperty.Register(
            "ToolBarCommands", 
            typeof(ObservableCollection<ToolBarCommandBase>), 
            typeof(TaskTemplateViewModel), 
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));

        /// <summary> Команды панели инструментов </summary>
        public ObservableCollection<ToolBarCommandBase> ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>)GetValue(ToolBarCommandsProperty); }
            set { SetValue(ToolBarCommandsProperty, value); }
        }

        /// <summary> Выданный в задании граф </summary>
        public static readonly DependencyProperty GivenGraphProperty =
            DependencyProperty.Register("GivenGraph", typeof(IGraph), typeof(TaskTemplateViewModel), new PropertyMetadata(default(IGraph)));

        /// <summary> Выданный в задании граф </summary>
        public IGraph GivenGraph
        {
            get { return (IGraph)GetValue(GivenGraphProperty); }
            set { SetValue(GivenGraphProperty, value); }
        }

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public static readonly DependencyProperty OnLoadedCmdProperty =
            DependencyProperty.Register("OnLoadedCmd", typeof(ICommand), typeof(TaskTemplateViewModel), new PropertyMetadata(default(ICommand)));

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public ICommand OnLoadedCmd
        {
            get { return (ICommand)GetValue(OnLoadedCmdProperty); }
            set { SetValue(OnLoadedCmdProperty, value); }
        }

        /// <summary> Клик по вершине </summary>
        public static readonly DependencyProperty VertexClickCmdProperty =
            DependencyProperty.Register("VertexClickCmd", typeof(ICommand), typeof(TaskTemplateViewModel), new PropertyMetadata(default(ICommand)));

        /// <summary> Клик по вершине </summary>
        public ICommand VertexClickCmd
        {
            get { return (ICommand)GetValue(VertexClickCmdProperty); }
            set { SetValue(VertexClickCmdProperty, value); }
        }

        #endregion


        /// <summary> Ctor. </summary>
        public TaskTemplateViewModel(long taskId, Guid sessionGuid, 
            DisposableWcfClientWrapper<ITasksDataServiceClient> dataServiceClient,
            DisposableWcfClientWrapper<IUserActionsRegistratorClient> actionsRegistratorClient,
            IDateTimeService dateTimeService)
        {
            _taskId = taskId;
            _sessionGuid = sessionGuid;

            _variantProvider = new VariantProvider(_taskId, _sessionGuid, _allowedGeneratorVersions, dataServiceClient);
            _variantProvider.VariantDownloaded += TaskLoadingComplete;
            _variantProvider.PropertyChanged += (sender, args) => HandlePropertyChanged(args);

            UserActionsManager = new UserActionsManager(_taskId, _sessionGuid, actionsRegistratorClient, dateTimeService);
            UserActionsManager.PropertyChanged += (sender, args) => HandlePropertyChanged(args);
            UserActionsManager.SendReportOnEveryAction = true; // mock очень не хочется настраивать нормально, в боевом режиме можно выключить.

            InitToolBarCommands();

            OnLoadedCmd = new DelegateCommand(o => _variantProvider.DownloadVariantAsync(), o => true);
            VertexClickCmd = new DelegateCommand(
                o => UserActionsManager.RegisterInfo(string.Format("Клик по вершине [{0}]", ((IVertex)o).Name)), 
                o => true);
        }

        private void HandlePropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ExpressionUtility.NameForMember((IUiBlockerAsyncProcessor p) => p.IsBusy))
                RecalculateIsLoadingData();
        }

        private void RecalculateIsLoadingData()
        {
            IsLoadingData = _variantProvider.IsBusy || UserActionsManager.IsBusy;
        }

        private void TaskLoadingComplete(object sender, VariantDownloadedEventArgs e)
        {
            GivenGraph = GraphSerializer.Deserialize(e.Data);

            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
        }
    }
}
