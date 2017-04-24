using System;
using GraphLabs.CommonUI;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    /// <summary> Поиск компонент сильной связности, построение конденсата </summary>
    public partial class TaskTemplate : TaskViewBase
    {
        /// <summary> Ctor. </summary>
        public TaskTemplate()
        {
            InitializeComponent();
        }

        /// <summary> Клик по вершине </summary>
        public event EventHandler<VertexClickEventArgs> VertexClicked;

        /// <summary>
        /// Клик по вершине дерева
        /// </summary>
        public event EventHandler<VertexClickEventArgs> VertexClickedTree;


        private void OnVertexClick(object sender, VertexClickEventArgs e)
        {
            var handler = VertexClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void Visualizer_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Visualizer_Tree_OnVertexClick(object sender, VertexClickEventArgs e)
        {
            var handler = VertexClickedTree;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
