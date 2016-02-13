using System;
using System.IO;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;

namespace DebugVariantDataGenerator
{
    /// <summary>
    /// Это генератор данных тестового варианта для отладки на сайте.
    /// Если не получается запустить его из-за ошибки вида
    /// "Could not copy the file "...\GraphLabs.Tasks.Template\Properties\DebugVariantData.bin" because it was not found.",
    /// то нужно временно выгрузить проект модуля-задания (правой кнопкой по проекту -> UnloadProject)
    /// </summary>
    class Program
    {
        public static byte[] GetSerializedGraph()
        {
            var graph = DirectedGraph.CreateEmpty(7);
            graph.AddEdge(new DirectedEdge(graph.Vertices[0], graph.Vertices[5]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[1], graph.Vertices[0]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[1], graph.Vertices[5]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[2], graph.Vertices[1]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[2], graph.Vertices[5]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[3], graph.Vertices[4]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[4], graph.Vertices[2]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[4], graph.Vertices[3]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[5], graph.Vertices[6]));
            graph.AddEdge(new DirectedEdge(graph.Vertices[6], graph.Vertices[4]));

            return GraphSerializer.Serialize(graph);
        }

        static void Main(string[] args)
        {
            File.WriteAllBytes(@"..\..\..\GraphLabs.Tasks.Template\Debug\DebugVariantData.bin", GetSerializedGraph());
        }
    }
}