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

        private void OnVertexClicked(VertexClickEventArgs e)
        {
            var handler = VertexClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnVertexClick(object sender, VertexClickEventArgs e)
        {
            OnVertexClicked(e);
        }
    }
}
