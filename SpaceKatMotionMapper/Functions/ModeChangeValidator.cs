using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceKatMotionMapper.Functions;

public class ModeChangeValidator
{
    // 邻接矩表
    private List<List<int>> _adj = [];

    private Dictionary<int, int> _nodeNum = [];

    public void AddNode(int node)
    {
        _adj.Add([]);
        _nodeNum.Add(node, _adj.Count - 1);
    }

    public void AddEdge(int fromNode, int toNode)
    {
        var nodeAdj = _adj[_nodeNum[fromNode]];
        if (!nodeAdj.Contains(toNode))
        {
            nodeAdj.Add(toNode);
        }
    }

    public (List<int>, List<int>) Validate()
    {
        var n = _adj.Count;

        // Step 1: 找出原图中从0出发可达的所有节点
        var visitedOriginal = new bool[n];
        var queue = new Queue<int>();
        queue.Enqueue(0);
        visitedOriginal[0] = true;

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            foreach (var v in _adj[u].Where(v => !visitedOriginal[v]))
            {
                visitedOriginal[v] = true;
                queue.Enqueue(v);
            }
        }

        // 收集原图可达节点
        HashSet<int> reachableFromStart = [];
        for (var i = 0; i < n; i++)
        {
            if (visitedOriginal[i]) reachableFromStart.Add(i);
        }

        // Step 2: 找出原图中从0出发不可达的所有节点
        var cannotToList = _nodeNum.Keys.Where(e => !reachableFromStart.Contains(e)).Select(e => _nodeNum[e]).ToList();

        // Step 3: 构建反向图
        List<List<int>> reverseAdj = [];
        for (var i = 0; i < n; i++)
        {
            reverseAdj.Add([]);
        }

        for (var u = 0; u < n; u++)
        {
            foreach (var v in _adj[u])
            {
                reverseAdj[v].Add(u); // 将边反向
            }
        }

        // Step 4: 在反向图中找出从0可达的节点
        var visitedReverse = new bool[n];
        queue.Clear();
        queue.Enqueue(0);
        visitedReverse[0] = true;

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            foreach (var v in reverseAdj[u].Where(v => !visitedReverse[v]))
            {
                visitedReverse[v] = true;
                queue.Enqueue(v);
            }
        }

        // 收集反向可达节点
        HashSet<int> reachableFromReverse = [];
        for (var i = 0; i < n; i++)
        {
            if (visitedReverse[i]) reachableFromReverse.Add(i);
        }


        // Step 5: 计算原图可达但反向不可达的节点
        var cannotReturnList = reachableFromStart
            .Where(node => !reachableFromReverse.Contains(node))
            .Select(e => _nodeNum[e]).ToList();
        return (cannotToList, cannotReturnList);
    }
}