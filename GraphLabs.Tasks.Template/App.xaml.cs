using System.Collections.Generic;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Configuration;
using GraphLabs.Tasks.Template.Configuration;

namespace GraphLabs.Tasks.Template
{
    /// <summary> TaskTemplate app </summary>
    public partial class App : TaskApplicationBase
    {
        private static IEnumerable<IDependencyResolverConfigurator> GetConfigurators()
        {
            // Wcf-сервисы
            yield return GetWcfServicesConfigurator();
            
            // Построитель View - сделано так, потому что в Xaml Silverlight нельзя подсунуть Generic
            yield return new ViewBuilderConfigurator<ViewBuilder<TaskTemplate, TaskTemplateViewModel>>();

            yield return new CommonItemsConfigurator();
        }

        private static IDependencyResolverConfigurator GetWcfServicesConfigurator()
        {
            return Current.IsRunningOutOfBrowser 
                ? (IDependencyResolverConfigurator)new MockedWcfServicesConfigurator()
                                                   {
                                                       GettingVariantDelay = 500
                                                   }
                : (IDependencyResolverConfigurator)new WcfServicesConfigurator();
        }

        /// <summary> TaskTemplate app </summary>
        public App() : base(GetConfigurators())
        {
            InitializeComponent();
        }
    }
}
