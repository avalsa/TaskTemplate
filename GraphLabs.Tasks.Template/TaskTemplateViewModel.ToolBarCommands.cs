using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Utils;

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

            // Перемещение вершин - toogleCommand
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

            // Перемещение вершин
            var dontTouch = new ToolBarInstantCommand(
                () => UserActionsManager.RegisterMistake("Сказали же НЕ ТРОГАТЬ!!!", 1),
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("DontTouch.png")),
                Description = "НЕ ТРОГАТЬ"
            };

            ToolBarCommands.Add(moveCommand);
            ToolBarCommands.Add(dontTouch);
        }
    }
}
