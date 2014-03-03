using System;
using System.Linq;
using System.Windows;
using GraphLabs.Common;
using GraphLabs.Common.UserActionsRegistrator;
using GraphLabs.CommonUI;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;
using GraphLabs.Utils.Services;
using Moq;

namespace GraphLabs.Tasks.Template
{
    /// <summary> TaskTemplate app </summary>
    public partial class App : Application
    {
        private DisposableWcfClientWrapper<ITasksDataServiceClient> _dataServiceClientWrapper;
        private DisposableWcfClientWrapper<IUserActionsRegistratorClient> _actionsRegistratorClientWrapper;

        /// <summary> Ctor. </summary>
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var taskId = !Current.IsRunningOutOfBrowser ? e.GetTaskId() : 0;
            var sessionGuid = !Current.IsRunningOutOfBrowser ? e.GetSessionGuid() : new Guid();
            

            _dataServiceClientWrapper = GetDataServiceClient();
            _actionsRegistratorClientWrapper = GetActionRegistratorClient();
            var dateTimeService = new DateTimeService();

            var viewModel = new TaskTemplateViewModel(taskId, sessionGuid, 
                _dataServiceClientWrapper, _actionsRegistratorClientWrapper, dateTimeService);
            this.RootVisual = new TaskTemplate { DataContext = viewModel };
        }

        /// <summary> Только для эмуляции </summary>
        private int _currentScore = UserActionsManager.StartingScore;

        private DisposableWcfClientWrapper<IUserActionsRegistratorClient> GetActionRegistratorClient()
        {
            IUserActionsRegistratorClient client;
            if (!Current.IsRunningOutOfBrowser)
            {
                client = new UserActionsRegistratorClient();
            }
            else
            {
                client = GetMockedUserActionsRegistratorClient();
            }

            return new DisposableWcfClientWrapper<IUserActionsRegistratorClient>(client);
        }

        private DisposableWcfClientWrapper<ITasksDataServiceClient> GetDataServiceClient()
        {
            ITasksDataServiceClient client;
            if (!Current.IsRunningOutOfBrowser)
            {
                client = new TasksDataServiceClient();
            }
            else
            {
                client = GetMockedTasksDataServiceClient();
            }

            return new DisposableWcfClientWrapper<ITasksDataServiceClient>(client);
        }


        #region Заглушки сервисов

        private static ITasksDataServiceClient GetMockedTasksDataServiceClient()
        {
            var debugGraph = DirectedGraph.CreateEmpty(7);
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[0]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[1]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[3], debugGraph.Vertices[4]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[4], debugGraph.Vertices[2]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[4], debugGraph.Vertices[3]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[5], debugGraph.Vertices[6]));
            debugGraph.AddEdge(new DirectedEdge(debugGraph.Vertices[6], debugGraph.Vertices[4]));
            var serializedGraph = GraphSerializer.Serialize(debugGraph);
            var taskVariantInfo = new TaskVariantInfo
            {
                Data = serializedGraph,
                GeneratorVersion = "1.0",
                Number = "Debug",
                Version = 1
            };

            var dataServiceMock = new Mock<ITasksDataServiceClient>(MockBehavior.Loose);
            dataServiceMock.Setup(srv => srv.GetVariantAsync(It.IsAny<long>(), It.IsAny<Guid>()))
                .Callback(() =>
                    dataServiceMock.Raise(mock => mock.GetVariantCompleted += null,
                        new GetVariantCompletedEventArgs(new object[] {taskVariantInfo}, null, false, null)));

            return dataServiceMock.Object;
        }

        private IUserActionsRegistratorClient GetMockedUserActionsRegistratorClient()
        {
            var registratorMock = new Mock<IUserActionsRegistratorClient>(MockBehavior.Loose);
            registratorMock.Setup(reg => reg.RegisterUserActionsAsync(
                It.IsAny<long>(),
                It.IsAny<Guid>(),
                It.Is<ActionDescription[]>(d => d.Count() == 1 && d[0].Penalty == 0),
                It.IsAny<bool>()))
                .Callback(() =>
                    registratorMock.Raise(mock => mock.RegisterUserActionsCompleted += null,
                        new RegisterUserActionsCompletedEventArgs(new object[] {_currentScore}, null, false, null)));
            registratorMock.Setup(reg => reg.RegisterUserActionsAsync(
                It.IsAny<long>(),
                It.IsAny<Guid>(),
                It.Is<ActionDescription[]>(d => d.Count() == 1 && d[0].Penalty != 0),
                It.IsAny<bool>()))
                .Callback<long, Guid, ActionDescription[], bool>(
                    (l, g, d, b) => registratorMock.Raise(mock => mock.RegisterUserActionsCompleted += null,
                        new RegisterUserActionsCompletedEventArgs(new object[] { _currentScore = _currentScore - d[0].Penalty },
                            null, false, null)));

            return registratorMock.Object;
        }

        #endregion


        private void Application_Exit(object sender, EventArgs e)
        {
            if (_dataServiceClientWrapper != null)
            {
                _dataServiceClientWrapper.Dispose();
                _dataServiceClientWrapper = null;
            }
            if (_actionsRegistratorClientWrapper != null)
            {
                _actionsRegistratorClientWrapper.Dispose();
                _actionsRegistratorClientWrapper = null;
            }
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // Если приложение выполняется вне отладчика, воспользуйтесь для сообщения об исключении
            // механизмом исключений браузера. В IE исключение будет отображаться в виде желтого значка оповещения 
            // в строке состояния, а в Firefox - в виде ошибки сценария.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // ПРИМЕЧАНИЕ. Это позволит приложению выполняться после того, как исключение было выдано,
                // но не было обработано. 
                // Для рабочих приложений такую обработку ошибок следует заменить на код, 
                // оповещающий веб-сайт об ошибке и останавливающий работу приложения.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
