using GraphLabs.Common.VariantProviderService;
using GraphLabs.CommonUI.Configuration;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;

namespace GraphLabs.Tasks.Template.Configuration
{
    /// <summary> Конфигуратор заглушек wcf-сервисов </summary>
    public class MockedWcfServicesConfigurator : MockedWcfServicesConfiguratorBase
    {
        /// <summary> Сгенерировать отладочный вариант </summary>
        protected override TaskVariantDto GetDebugVariant()
        {
            var debugGraph = UndirectedGraph.CreateEmpty(6);
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[0]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[1]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[3], debugGraph.Vertices[4]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[4], debugGraph.Vertices[2]));
            var serializedGraph = VariantSerializer.Serialize(new IGraph[] { debugGraph });

            return new TaskVariantDto
            {
                Data = serializedGraph,
                GeneratorVersion = "1.0",
                Number = "Debug",
                Version = 1
            };
        }
    }
}