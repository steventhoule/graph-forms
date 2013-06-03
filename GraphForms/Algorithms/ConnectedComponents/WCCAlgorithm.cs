using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    public class WCCAlgorithm<Node, Edge>
        : DepthFirstSearchAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private int[] mComponents;
        private List<int> mComponentEquivalences = new List<int>();
        private int mComponentCount = 0;
        private int mCurrentComponent = 0;

        private Node[][] mWeakComponents;

        public WCCAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, true, false)
        {
        }

        public WCCAlgorithm(Digraph<Node, Edge> graph,
            bool reversed)
            : base(graph, true, reversed)
        {
        }

        public Node[][] Components
        {
            get { return this.mWeakComponents; }
        }

        public override void Initialize()
        {
            this.mComponentCount = 0;
            this.mCurrentComponent = 0;
            this.mComponentEquivalences.Clear();
            this.mComponents = new int[this.mGraph.NodeCount];
            for (int i = 0; i < this.mComponents.Length; i++)
            {
                this.mComponents[i] = -1;
            }
            base.Initialize();
        }

        private void CompileComponents()
        {
            // updating component numbers
            int i, component, equivalent;
            for (i = 0; i < this.mComponents.Length; i++)
            {
                component = this.mComponents[i];
                equivalent = this.GetComponentEquivalence(component);
                if (component != equivalent)
                    this.mComponents[i] = equivalent;
            }

            List<Node>[] comps = new List<Node>[this.mComponentCount];
            for (i = 0; i < this.mComponentCount; i++)
            {
                comps[i] = new List<Node>();
            }

            for (i = 0; i < this.mComponents.Length; i++)
            {
                comps[this.mComponents[i]].Add(this.mGraph.NodeAt(i));
            }

            this.mWeakComponents = new Node[this.mComponentCount][];
            for (i = 0; i < this.mComponentCount; i++)
            {
                this.mWeakComponents[i] = comps[i].ToArray();
            }
        }

        protected override void OnFinished()
        {
            this.CompileComponents();
            base.OnFinished();
        }

        protected override void OnStartNode(Node n, int index)
        {
            // we are looking on a new tree
            this.mCurrentComponent = this.mComponentEquivalences.Count;
            this.mComponentEquivalences.Add(this.mCurrentComponent);
            this.mComponentCount++;
            this.mComponents[index] = this.mCurrentComponent;
            base.OnStartNode(n, index);
        }

        protected override void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            // new edge, we store with the current component number
            this.mComponents[reversed ? srcIndex : dstIndex] 
                = this.mCurrentComponent;
            base.OnTreeEdge(e, srcIndex, dstIndex, reversed);
        }

        private int GetComponentEquivalence(int component)
        {
            int equivalent = component;
            int temp = this.mComponentEquivalences[equivalent];
            bool compress = false;
            while (temp != equivalent)
            {
                equivalent = temp;
                temp = this.mComponentEquivalences[equivalent];
                compress = true;
            }

            // path compression
            if (compress)
            {
                int c = component;
                temp = this.mComponentEquivalences[c];
                while (temp != equivalent)
                {
                    temp = this.mComponentEquivalences[c];
                    this.mComponentEquivalences[c] = equivalent;
                }
            }

            return equivalent;
        }

        protected override void OnBlackEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            // we have touched another tree, 
            // updating count and current component
            int otherComponent = this.GetComponentEquivalence(
                this.mComponents[reversed ? srcIndex : dstIndex]);
            if (otherComponent != this.mCurrentComponent)
            {
                this.mComponentCount--;
                if (this.mCurrentComponent > otherComponent)
                {
                    this.mComponentEquivalences[this.mCurrentComponent]
                        = otherComponent;
                    this.mCurrentComponent = otherComponent;
                }
                else
                {
                    this.mComponentEquivalences[otherComponent] 
                        = this.mCurrentComponent;
                }
            }
            base.OnBlackEdge(e, srcIndex, dstIndex, reversed);
        }
    }
}
