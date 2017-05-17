using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Castle.Core.Internal;
using GraphLabs.Common;
using GraphLabs.Common.Utils;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;

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
            AddVertex
        }

        /// <summary> Текущее состояние </summary>
        private State _state;

        private IVertex _markedVertex;

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

        /// <summary> дерево </summary>
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
            if (_state != State.AddVertex) return; //interesting only if addVertex to tree
            //try to add
            try
            {
                var col = _markedVertex.Name.Split(':');
                Vertex v = new Vertex((col.Length == 1 ? col[0] : col[1]) + ":" + vertex.Name);
                Tree.AddVertex(v);
                Tree.AddEdge(new UndirectedEdge(new Vertex(_markedVertex.Name), v));
                RedrawTree();   //cheat
            }
            catch (Exception)
            {
                UserActionsManager.RegisterMistake("Ошибка -) !!!!!!!!!!", 10);
            }
            //animation of adjcent
            SetGraphStdColors();
            SetGraphAjcentVertexColors(vertex);

        }

        

        /// <summary>
        /// Клик по вершине дерева
        /// </summary>
        /// <param name="vertex"></param>
        public void Visualizer_Tree_OnVertexClick(IVertex vertex)
        {
            UserActionsManager.RegisterInfo(string.Format("Клик по вершине [{0}] дерева", vertex.Name));
            if (_state != State.MarkVertex) return; //interesting only if markVertex
            if (vertex.Name.Equals(_markedVertex.Name)) return; //click 2 times at same vertex

            //visualization
            SetGraphStdColors();
            MarkTreeVertexColors(vertex);
            PerspectiveVertexColors(vertex);
            _markedVertex = vertex;
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
            Dispatcher.BeginInvoke((() =>
            {
                Tree = new UndirectedGraph();
                _markedVertex = new Vertex("root");
                Tree.AddVertex(_markedVertex);
                View.Visualizer_Tree.Vertices[0].BorderBrush = new SolidColorBrush(Colors.Blue);
            }));
            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
        }

        #region Раскраска вершин

        private void RedrawTree()       //fixme it's cheet
        {
            var vertices = View.Visualizer_Tree.Vertices;
            if (!vertices.Any())
                return;

            var curH = 100;
            var STEP = 100;

            var que = new Queue<GraphLabs.Graphs.UIComponents.Visualization.Vertex>();
            que.Enqueue(vertices[0]);

            while (!que.IsNullOrEmpty())
            {
                var StepX = View.Visualizer_Tree.ActualWidth / (que.Count + 1);
                var i = 1;
                foreach (var vert in que)
                {
                    vert.ModelX = StepX * i++;
                    vert.ModelY = curH;
                    vert.ScaleFactor = 1;
                }
                curH += STEP;

                var nextQue = new Queue<GraphLabs.Graphs.UIComponents.Visualization.Vertex>();
                while (!que.IsNullOrEmpty())
                {
                    var ver = que.Dequeue();
                    var quenextPart = getAdjcentVertexes(ver);
                    while (!quenextPart.IsNullOrEmpty())
                        nextQue.Enqueue(quenextPart.Dequeue());
                }
                que = nextQue;
            }
            vertices[0].ModelX = View.Visualizer_Tree.ActualWidth / 2;
            vertices[0].ModelY = 100;
        }

        private Queue<GraphLabs.Graphs.UIComponents.Visualization.Vertex> getAdjcentVertexes(GraphLabs.Graphs.UIComponents.Visualization.Vertex v)
        {
            var q = new Queue<GraphLabs.Graphs.UIComponents.Visualization.Vertex > ();
            var edges = View.Visualizer_Tree.Edges;
            foreach (var edge in edges)
            {
                if (edge.Vertex1.Equals(v))
                    q.Enqueue(edge.Vertex2);
            }
            return q;
        }

        private void SetGraphStdColors()
        {
            foreach (var vertex1 in View.Visualizer.Vertices)
                vertex1.BorderBrush = new SolidColorBrush(Colors.Black);
        }

        private void SetGraphAjcentVertexColors(IVertex vertex)
        {
            foreach (var vertex1 in View.Visualizer.Vertices)
            {
                if (vertex1.Name.Equals(vertex.Name)) vertex1.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 200, 0));
                if (GivenGraph.Edges.Contains(new UndirectedEdge(new Vertex(vertex.Name), new Vertex(vertex1.Name))))
                    vertex1.BorderBrush = new SolidColorBrush(Colors.Magenta);
            }
        }

        private void MarkTreeVertexColors(IVertex vertex)
        {
            foreach (var vertex1 in View.Visualizer_Tree.Vertices)
            {
                if (vertex1.Name.Equals(_markedVertex.Name))
                    vertex1.BorderBrush = new SolidColorBrush(Colors.Black);
                if (vertex1.Name.Equals(vertex.Name))
                    vertex1.BorderBrush = new SolidColorBrush(Colors.Blue);
            }
        }

        private void PerspectiveVertexColors(IVertex vertex)
        {
            bool markall = false;
            var path = new LinkedList<string>();
            //path.AddLast(vertex.Name);
            string vName = vertex.Name;
            var col = vertex.Name.Split(':');
            var work = true;
            while (work)
            {
                if (col.Length == 1) break;
                path.AddLast(col[1]);

                foreach (var edge in View.Visualizer_Tree.Edges)
                {

                    if (edge.Vertex1.Name.Equals(vName) || edge.Vertex2.Name.Equals(vName))
                    {
                        var v = edge.Vertex1.Name.Equals(vName) ? edge.Vertex2 : edge.Vertex1;
                        var colv = v.Name.Split(':');
                        if (colv.Length == 1)
                        {
                            work = false; break;
                        }
                        if (colv[1].Equals(col[0]))
                        {
                            vName = v.Name;
                            col = colv; break;
                        }
                    }
                }
            }
 
            //all undisplayed vertexes
            var col2 = new LinkedList<string>(path);
            foreach (var vertex1 in View.Visualizer.Vertices)
            {
                foreach (var verName in path)
                {
                    if (GivenGraph.Edges.Contains(new UndirectedEdge(new Vertex(verName), new Vertex(vertex1.Name))))
                        col2.AddLast(vertex1.Name);
                }
            }

            //display other vertexes
            foreach (var vertex1 in View.Visualizer.Vertices)
                foreach (var verName in col2)
                    if (vertex1.Name == verName)
                        vertex1.BorderBrush = new SolidColorBrush(Colors.Red);
        }

       

        #endregion
    }
}