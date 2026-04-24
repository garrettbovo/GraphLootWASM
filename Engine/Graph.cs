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
        if (!_adjList.ContainsKey(start) || !_adjList.ContainsKey(end))
            return new PathResult(new List<string>(), 0, 0);

        var gScore = new Dictionary<string, double>();
        var fScore = new Dictionary<string, double>();
        var predecessors = new Dictionary<string, string?>();
        var pq = new PriorityQueue<(string, double), double>();

        foreach (var v in _adjList.Keys)
        {
            gScore[v] = double.MaxValue;
            fScore[v] = double.MaxValue;
            predecessors[v] = null;
        }

        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);
        pq.Enqueue((start, fScore[start]), fScore[start]);

        while (pq.Count > 0)
        {
            var (current, _) = pq.Dequeue();

            if (current == end)
                break;

            foreach (var edge in _adjList[current])
            {
                var tentative = gScore[current] + edge.Weight;
                if (tentative < gScore[edge.To])
                {
                    predecessors[edge.To] = current;
                    gScore[edge.To] = tentative;
                    fScore[edge.To] = gScore[edge.To] + Heuristic(edge.To, end);
                    pq.Enqueue((edge.To, fScore[edge.To]), fScore[edge.To]);
                }
            }
        }

        var path = ReconstructPath(start, end, predecessors);
        return new PathResult(path, gScore[end], path.Count - 1);
    }

    private double Heuristic(string from, string to)
    {
        if (!_nodes.ContainsKey(from) || !_nodes.ContainsKey(to))
            return 0;

        var n1 = _nodes[from];
        var n2 = _nodes[to];
        var dx = n1.X - n2.X;
        var dy = n1.Y - n2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
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
