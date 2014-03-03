using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    /// <summary> Поиск компонент сильной связности, построение конденсата </summary>
    public partial class TaskTemplate : UserControl
    {
        #region Команды

        /// <summary> Клик по вершине </summary>
        public static readonly DependencyProperty VertexClickCommandProperty = DependencyProperty.Register(
            "VertexClickCommand", 
            typeof(ICommand), 
            typeof(TaskTemplate), 
            new PropertyMetadata(default(ICommand)));

        /// <summary> Клик по вершине </summary>
        public ICommand VertexClickCommand
        {
            get { return (ICommand)GetValue(VertexClickCommandProperty); }
            set { SetValue(VertexClickCommandProperty, value); }
        }

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public static readonly DependencyProperty OnLoadedCommandProperty =
            DependencyProperty.Register("OnLoadedCommand", typeof(ICommand), typeof(TaskTemplate), new PropertyMetadata(default(ICommand)));

        /// <summary> Загрузка silvelight-модуля выполнена </summary>
        public ICommand OnLoadedCommand
        {
            get { return (ICommand)GetValue(OnLoadedCommandProperty); }
            set { SetValue(OnLoadedCommandProperty, value); }
        }

        #endregion

        /// <summary> Ctor. </summary>
        public TaskTemplate()
        {
            InitializeComponent();
            
            // Куча Binding'ов (в реальных заданиях)
            SetBinding(VertexClickCommandProperty, new Binding("VertexClickCmd"));
            SetBinding(OnLoadedCommandProperty, new Binding("OnLoadedCmd"));
        }

        private void OnVertexClick(object sender, VertexClickEventArgs e)
        {
            if (VertexClickCommand != null)
            {
                VertexClickCommand.Execute(e.Vertex);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (OnLoadedCommand != null)
            {
                OnLoadedCommand.Execute(null);
            }
        }
    }
}
