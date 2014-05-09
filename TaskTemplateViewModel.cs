using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using GraphLabs.Common;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;
using GraphLabs.Utils;

namespace GraphLabs.Tasks.Template
{
    /// <summary> ViewModel для TaskTemplate </summary>
    public partial class TaskTemplateViewModel : TaskViewModelBase<TaskTemplate>
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

        /// <summary> Допустимые версии генератора, с помощью которого сгенерирован вариант </summary>
        private readonly Version[] _allowedGeneratorVersions = {  new Version(1, 0) };
        
        /// <summary> Допустимые версии генератора </summary>
        protected override Version[] AllowedGeneratorVersions
        {
            get { return _allowedGeneratorVersions; }
        }


        #region Public свойства вьюмодели

        /// <summary> Идёт загрузка данных? </summary>
        public static readonly DependencyProperty IsLoadingDataProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsLoadingData), 
            typeof(bool), 
            typeof(TaskTemplateViewModel), 
            new PropertyMetadata(false));

        /// <summary> Разрешено перемещение вершин? </summary>
        public static readonly DependencyProperty IsMouseVerticesMovingEnabledProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsMouseVerticesMovingEnabled),
            typeof(bool),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(false));

        /// <summary> Команды панели инструментов</summary>
        public static readonly DependencyProperty ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));

        /// <summary> Выданный в задании граф </summary>
        public static readonly DependencyProperty GivenGraphProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.GivenGraph),
            typeof(IGraph),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(IGraph)));


        /// <summary> Идёт загрузка данных? </summary>
        public bool IsLoadingData
        {
            get { return (bool)GetValue(IsLoadingDataProperty); }
            private set { SetValue(IsLoadingDataProperty, value); }
        }

        /// <summary> Разрешено перемещение вершин? </summary>
        public bool IsMouseVerticesMovingEnabled
        {
            get { return (bool)GetValue(IsMouseVerticesMovingEnabledProperty); }
            set { SetValue(IsMouseVerticesMovingEnabledProperty, value); }
        }

        /// <summary> Команды панели инструментов </summary>
        public ObservableCollection<ToolBarCommandBase> ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>)GetValue(ToolBarCommandsProperty); }
            set { SetValue(ToolBarCommandsProperty, value); }
        }

        /// <summary> Выданный в задании граф </summary>
        public IGraph GivenGraph
        {
            get { return (IGraph)GetValue(GivenGraphProperty); }
            set { SetValue(GivenGraphProperty, value); }
        }

        #endregion


        /// <summary> Инициализация </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            UserActionsManager.PropertyChanged += (sender, args) => HandlePropertyChanged(args);
            VariantProvider.PropertyChanged += (sender, args) => HandlePropertyChanged(args);
            InitToolBarCommands();
            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            View.VertexClicked += (sender, args) => OnVertexClick(args.Vertex);
            View.Loaded += (sender, args) => StartVariantDownload();
        }

        /// <summary> Начать загрузку варианта </summary>
        public void StartVariantDownload()
        {
            VariantProvider.DownloadVariantAsync();
        }

        /// <summary> Клик по вершине </summary>
        public void OnVertexClick(IVertex vertex)
        {
            UserActionsManager.RegisterInfo(string.Format("Клик по вершине [{0}]", vertex.Name));
        }

        private void HandlePropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ExpressionUtility.NameForMember((IUiBlockerAsyncProcessor p) => p.IsBusy))
                RecalculateIsLoadingData();
        }

        private void RecalculateIsLoadingData()
        {
            IsLoadingData = VariantProvider.IsBusy || UserActionsManager.IsBusy;
        }

        /// <summary> Задание загружено </summary>
        /// <param name="e"></param>
        protected override void OnTaskLoadingComlete(VariantDownloadedEventArgs e)
        {
            GivenGraph = GraphSerializer.Deserialize(e.Data);

            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
        }
    }
}
