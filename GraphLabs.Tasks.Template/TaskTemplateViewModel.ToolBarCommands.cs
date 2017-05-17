using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using GraphLabs.CommonUI.Controls.ViewModels;


namespace GraphLabs.Tasks.Template
{
    public partial class TaskTemplateViewModel
    {
        private const string ImageResourcesPath = @"/GraphLabs.Tasks.Template;component/Images/";

        private Uri GetImageUri(string imageFileName)
        {
            return new Uri(ImageResourcesPath + imageFileName, UriKind.Relative);
        }

        private void InitToolBarCommands()
        {
            ToolBarCommands = new ObservableCollection<ToolBarCommandBase>();

            // Перемещение вершин
            var moveCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    _state = State.MoveVertex;
                    UserActionsManager.RegisterInfo("Включено перемещение вершин.");
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo("Отключено перемещение вершин.");
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Move.png")),
                Description = "Перемещение вершин"
            };

            // помечание вершин дерева
            var markVertex = new ToolBarToggleCommand(
               () =>
               {
                   _state = State.MarkVertex;
                   UserActionsManager.RegisterInfo("Включено помечание вершины для создание подуровня.");
               },
               () =>
               {
                   //SetGraphStdColors();
                   _state = State.Nothing;
                   UserActionsManager.RegisterInfo("Отключено помечание вершины для создание подуровня.");
               },
               () => _state == State.Nothing,
               () => true
               )
            {
                Image = new BitmapImage(GetImageUri("Mark.png")),
                Description = "Помечание вершины дерева"
            };

            // добаление вершин графа в поддерво помечанной вершины 
            var addVertex = new ToolBarToggleCommand(
               () =>
               {
                   _state = State.AddVertex;
                   UserActionsManager.RegisterInfo("Включено добавление вершин в подуровень.");
               },
               () =>
               {
                   SetGraphStdColors();
                   _state = State.Nothing;
                   UserActionsManager.RegisterInfo("Отключено добавление вершин в подуровень.");
               },
               () => _state == State.Nothing,
               () => true
               )
            {
                Image = new BitmapImage(GetImageUri("Add.png")),
                Description = "Добавление вершин в поддерево помечанной вершины"
            };

            // Завершение работы
            var finishTask = new ToolBarInstantCommand(
                () =>
                {
                    UserActionsManager.ReportThatTaskFinished();
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Complete.png")),
                Description = "Завершить задание"
            };

            ToolBarCommands.Add(moveCommand);
            ToolBarCommands.Add(addVertex);
            ToolBarCommands.Add(markVertex);
            ToolBarCommands.Add(finishTask);
        }
    }
}
