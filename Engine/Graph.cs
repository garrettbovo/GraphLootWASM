namespace GraphLootWASM.Engine;

using System.Collections.Generic;
using System.Linq;

public class Graph
{
    private Dictionary<string, List<Edge>> _adjList = new();
    private Dictionary<string, NodeData> _nodes = new();

    public IReadOnlyDictionary<string, List<Edge>> AdjList => _adjList;
    public IReadOnlyDictionary<string, NodeData> Nodes => _nodes;

    public void AddVertex(string name)
    {
        if (!_adjList.ContainsKey(name))
            _adjList[name] = new List<Edge>();
    }

    public void AddNode(string name, double x, double y)
    {
        if (!_nodes.ContainsKey(name))
        {
            AddVertex(name);
            _nodes[name] = new NodeData(name, x, y);
        }
    }

    public void AddEdge(string from, string to, double weight)
    {
        AddVertex(from);
        AddVertex(to);

        if (!_adjList[from].Any(e => e.To == to))
            _adjList[from].Add(new Edge(to, weight));
    }

    public List<string> ShortestPath(string start, string end)
    {
        var result = RunDijkstra(start, end);
        return result.Path;
    }

    public List<string> AStar(string start, string end)
    {
        var result = RunAStar(start, end);
        return result.Path;
    }

    public List<string> GetNeighbors(string node)
    {
        if (_adjList.TryGetValue(node, out var edges))
            return edges.Select(e => e.To).ToList();
        return new List<string>();
    }

    public PathResult RunDijkstra(string start, string end)
    {
        if (!_adjList.ContainsKey(start) || !_adjList.ContainsKey(end))
            return new PathResult(new List<string>(), 0, 0);

        var distances = new Dictionary<string, double>();
        var predecessors = new Dictionary<string, string?>();
        var pq = new PriorityQueue<(string, double), double>();

        foreach (var v in _adjList.Keys)
        {
            distances[v] = double.MaxValue;
            predecessors[v] = null;
        }

        distances[start] = 0;
        pq.Enqueue((start, 0), 0);

        while (pq.Count > 0)
        {
            var (current, currentDist) = pq.Dequeue();

            if (currentDist > distances[current])
                continue;

            if (current == end)
                break;

            foreach (var edge in _adjList[current])
            {
                var newDist = distances[current] + edge.Weight;
                if (newDist < distances[edge.To])
                {
                    distances[edge.To] = newDist;
                    predecessors[edge.To] = current;
                    pq.Enqueue((edge.To, newDist), newDist);
                }
            }
        }

        var path = ReconstructPath(start, end, predecessors);
        return new PathResult(path, distances[end], path.Count - 1);
    }

    public PathResult RunAStar(string start, string end)
    {
        return RunDijkstra(start, end);
    }

    private List<string> ReconstructPath(string start, string end, Dictionary<string, string?> predecessors)
    {
        var path = new List<string>();
        var current = end;

        while (current != null)
        {
            path.Add(current);
            current = predecessors[current];
        }

        path.Reverse();
        if (path.Count > 0 && path[0] == start)
            return path;

        return new List<string>();
    }
}
