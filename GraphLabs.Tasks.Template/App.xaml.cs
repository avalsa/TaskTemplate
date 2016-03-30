using System.Collections.Generic;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Configuration;
using GraphLabs.Tasks.Template.Configuration;

namespace GraphLabs.Tasks.Template
{
    /// <summary> TaskTemplate app </summary>
    public partial class App : TaskApplicationBase
    {
        /// <summary> Получить конфигураторы сервисов </summary>
        private static IEnumerable<IDependencyResolverConfigurator> GetConfigurators()
        {
            // Wcf-сервисы
            yield return GetWcfServicesConfigurator();
            
            // Построитель View - сделано так, потому что в Xaml Silverlight нельзя подсунуть Generic
            yield return new ViewBuilderConfigurator<ViewBuilder<TaskTemplate, TaskTemplateViewModel>>();

            yield return new CommonItemsConfigurator();
        }

        /// <summary> Получить конфигуратор WCF-сервисов </summary>
        /// <returns>
        /// Если приложение запущено в браузере, то честно взаимодействуем с сайтом.
        /// Если запущено вне бразера - используется специальный эмулятор.
        /// </returns>
        private static IDependencyResolverConfigurator GetWcfServicesConfigurator()
        {
            return Current.IsRunningOutOfBrowser 
                ? (IDependencyResolverConfigurator)new MockedWcfServicesConfigurator()
                                                   {
                                                       GettingVariantDelay = 500
                                                   }
                : (IDependencyResolverConfigurator)new WcfServicesConfigurator()
                {
                    UserActionsRegistratorAddress = "http://localhost:49202/UserActionsRegistrator.svc",
                    VariantProviderServiceClientAddress = "http://localhost:49202/VariantProviderService.svc"
                };
        }

        /// <summary> TaskTemplate app </summary>
        public App() : base(GetConfigurators())
        {
            InitializeComponent();
        }
    }
}
