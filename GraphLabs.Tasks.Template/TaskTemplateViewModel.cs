using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using GraphLabs.Common;
using GraphLabs.Common.Utils;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;
using GraphLabs.Graphs.UIComponents.Visualization;
using Edge = GraphLabs.Graphs.Edge;
using Vertex = GraphLabs.Graphs.Vertex;

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

            /// <summary> Пометить вершину как вершину для добавления </summary>
            MarkVertex,

            /// <summary> Добавить к поддереву помеченной вершины новые вершины </summary>
            AddVertex,

            /// <summary> Закончить задание </summary>
            FinishTask,
        }

        /// <summary> Текущее состояние </summary>
        private State _state;

        /// <summary> Допустимые версии генератора, с помощью которого сгенерирован вариант </summary>
        private readonly Version[] _allowedGeneratorVersions = {new Version(1, 0)};

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

        public static readonly DependencyProperty TreeProperty =
            DependencyProperty.Register(
                ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Tree),
                typeof(IGraph),
                typeof(TaskTemplateViewModel),
                new PropertyMetadata(default(IGraph)));

        /// <summary> Идёт загрузка данных? </summary>
        public bool IsLoadingData
        {
            get { return (bool) GetValue(IsLoadingDataProperty); }
            private set { SetValue(IsLoadingDataProperty, value); }
        }

        /// <summary> Разрешено перемещение вершин? </summary>
        public bool IsMouseVerticesMovingEnabled
        {
            get { return (bool) GetValue(IsMouseVerticesMovingEnabledProperty); }
            set { SetValue(IsMouseVerticesMovingEnabledProperty, value); }
        }

        /// <summary> Команды панели инструментов </summary>
        public ObservableCollection<ToolBarCommandBase> ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>) GetValue(ToolBarCommandsProperty); }
            set { SetValue(ToolBarCommandsProperty, value); }
        }

        /// <summary> Выданный в задании граф </summary>
        public IGraph GivenGraph
        {
            get { return (IGraph) GetValue(GivenGraphProperty); }
            set { SetValue(GivenGraphProperty, value); }
        }

        /// <summary>
        /// Дерево для построения
        /// </summary>
        public IGraph Tree
        {
            get { return (IGraph) GetValue(TreeProperty); }
            set { SetValue(TreeProperty, value); }
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
            View.VertexClicked += (sender, args) => OnVertexClick(args.Control);
            View.VertexClickedTree += (sender, args) => Visualizer_Tree_OnVertexClick(args.Control);
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
            UserActionsManager.RegisterInfo(string.Format("Клик по вершине [{0}] исходного графа", vertex.Name));
            Vertex v = new Vertex(vertex.Name);
            Tree.AddVertex(v);
            Tree.AddEdge(new UndirectedEdge(new Vertex(Tree.Vertices[0].Name), v));
            View.Visualizer.Vertices[Int32.Parse(vertex.Name)].Radius = 0.0;
        }

        /// <summary>
        /// Клик по вершине дерева
        /// </summary>
        /// <param name="vertex"></param>
        public void Visualizer_Tree_OnVertexClick(IVertex vertex)
        {
            UserActionsManager.RegisterInfo(string.Format("Клик по вершине [{0}] дерева", vertex.Name));
        }

        private void HandlePropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ExpressionUtility.NameForMember((IUiBlockerAsyncProcessor p) => p.IsBusy))
            {
                // Нас могли дёрнуть из другого потока, поэтому доступ к UI - через Dispatcher.
                Dispatcher.BeginInvoke(RecalculateIsLoadingData);
            }
        }

        private void RecalculateIsLoadingData()
        {
            IsLoadingData = VariantProvider.IsBusy || UserActionsManager.IsBusy;
        }

        /// <summary> Задание загружено </summary>
        /// <param name="e"></param>
        protected override void OnTaskLoadingComlete(VariantDownloadedEventArgs e)
        {
            // Мы вызваны из другого потока. Поэтому работаем с UI-элементами через Dispatcher.
            Dispatcher.BeginInvoke(() => { GivenGraph = VariantSerializer.Deserialize(e.Data)[0]; });
            Dispatcher.BeginInvoke((() => { Tree = new UndirectedGraph(); Tree.AddVertex(new Vertex("root")); }));
            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
        }
    }
}