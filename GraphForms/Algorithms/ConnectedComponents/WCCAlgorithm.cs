using System;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    public class WCCAlgorithm<Node, Edge>
        : DepthFirstSearch<Node, Edge>, ICCAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private int[] mComps;
        private int[] mRoots;
        private int[] mCompEquivalences;
        private int mCompEquivCount = 0;
        private int mCompCount = 0;
        private int mCurrentComp = 0;

        private Digraph<Node, Edge>.GNode[] mWeakRoots;
        private Digraph<Node, Edge>.GNode[][] mWeakComponents;

        public WCCAlgorithm(Digraph<Node, Edge> graph, bool reversed)
            : base(graph, true, reversed)
        {
            this.mCompEquivalences = new int[0];
        }

        public int ComponentCount
        {
            get { return this.mCompCount; }
        }

        public int[] ComponentIds
        {
            get { return this.mComps; }
        }

        public Digraph<Node, Edge>.GNode[][] Components
        {
            get { return this.mWeakComponents; }
        }

        public Digraph<Node, Edge>.GNode[] ComponentRoots
        {
            get { return this.mWeakRoots; }
        }

        public override void Initialize()
        {
            this.mCompCount = 0;
            this.mCurrentComp = 0;
            //this.mCompEquivalences.Clear();
            this.mCompEquivCount = 0;
            this.mRoots = new int[1];
            this.mComps = new int[this.mGraph.NodeCount];
            for (int i = 0; i < this.mComps.Length; i++)
            {
                this.mComps[i] = -1;
            }
            base.Initialize();
        }

        private int GetComponentEquivalence(int comp)
        {
            int equivalent = comp;
            int temp = this.mCompEquivalences[equivalent];
            bool compress = false;
            while (temp != equivalent)
            {
                equivalent = temp;
                temp = this.mCompEquivalences[equivalent];
                compress = true;
            }

            // path compression
            if (compress)
            {
                temp = this.mCompEquivalences[comp];
                while (temp != equivalent)
                {
                    temp = this.mCompEquivalences[comp];
                    this.mCompEquivalences[comp] = equivalent;
                }
            }

            return equivalent;
        }

        private void CompileComponents()
        {
            // updating component numbers
            int i, component, equivalent;
            for (i = 0; i < this.mComps.Length; i++)
            {
                component = this.mComps[i];
                equivalent = this.GetComponentEquivalence(component);
                if (component != equivalent)
                    this.mComps[i] = equivalent;
            }

            /*List<Node>[] comps = new List<Node>[this.mCompCount];
            for (i = 0; i < this.mCompCount; i++)
            {
                comps[i] = new List<Node>();
            }

            for (i = 0; i < this.mComps.Length; i++)
            {
                comps[this.mComps[i]].Add(this.mGraph.NodeAt(i));
            }/* */

            int j, count;
            Digraph<Node, Edge>.GNode[] comps 
                = new Digraph<Node, Edge>.GNode[this.mComps.Length];
            this.mWeakComponents 
                = new Digraph<Node, Edge>.GNode[this.mCompCount][];
            for (i = 0; i < this.mCompCount; i++)
            {
                count = 0;
                for (j = 0; j < this.mComps.Length; j++)
                {
                    if (this.mComps[j] == i)
                    {
                        comps[count++] = this.mGraph.InternalNodeAt(j);
                    }
                }
                this.mWeakComponents[i] 
                    = new Digraph<Node, Edge>.GNode[count];
                if (count > 0)
                {
                    Array.Copy(comps, 0, this.mWeakComponents[i], 0, count);
                }
            }

            this.mWeakRoots 
                = new Digraph<Node, Edge>.GNode[this.mRoots.Length];
            for (i = 0; i < this.mRoots.Length; i++)
            {
                this.mWeakRoots[i] 
                    = this.mGraph.InternalNodeAt(this.mRoots[i]);
            }
        }

        protected override void OnFinished()
        {
            this.CompileComponents();
            base.OnFinished();
        }

        protected override void OnStartNode(Digraph<Node, Edge>.GNode n)
        {
            // we are looking on a new tree
            //this.mCurrentComp = this.mCompEquivalences.Count;
            //this.mCompEquivalences.Add(this.mCurrentComp);
            this.mCurrentComp = this.mCompEquivCount;
            if (this.mCompEquivCount == this.mCompEquivalences.Length)
            {
                int[] compEquivs;
                if (this.mCompEquivCount == 0)
                {
                    compEquivs = new int[4];
                }
                else
                {
                    compEquivs = new int[2 * this.mCompEquivCount];
                    Array.Copy(this.mCompEquivalences, 0, 
                        compEquivs, 0, this.mCompEquivCount);
                }
                this.mCompEquivalences = compEquivs;
            }
            this.mCompEquivalences[this.mCompEquivCount++] 
                = this.mCurrentComp;
            if (this.mCompCount == this.mRoots.Length)
            {
                int[] roots = new int[this.mCompCount + 1];
                Array.Copy(this.mRoots, 0, roots, 0, this.mCompCount);
                this.mRoots = roots;
            }
            this.mRoots[this.mCompCount++] = n.Index;
            this.mComps[n.Index] = this.mCurrentComp;
        }

        protected override void OnTreeEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            // new edge, we store with the current component number
            this.mComps[reversed ? e.SrcNode.Index : e.DstNode.Index]//srcIndex : dstIndex] 
                = this.mCurrentComp;
        }

        protected override void OnBlackEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            // we have touched another tree, 
            // updating count and current component
            int otherComponent = this.GetComponentEquivalence(
                this.mComps[reversed ? e.SrcNode.Index : e.DstNode.Index]);//srcIndex : dstIndex]);
            if (otherComponent != this.mCurrentComp)
            {
                this.mCompCount--;
                if (this.mCurrentComp > otherComponent)
                {
                    this.mCompEquivalences[this.mCurrentComp]
                        = otherComponent;
                    this.mCurrentComp = otherComponent;
                }
                else
                {
                    this.mCompEquivalences[otherComponent] 
                        = this.mCurrentComp;
                }
            }
        }
    }
}
