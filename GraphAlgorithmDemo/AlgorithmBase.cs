using System;
using GraphForms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphAlgorithmDemo
{
    public class ElasticLayoutForCircles
        : ElasticLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public ElasticLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration, double statusInPercent, 
            double distanceChange, double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent, 
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, statusInPercent, 
                    distanceChange, maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Simple Elastic";
        }
    }

    public class FRLayoutForCircles
        : FRLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FRLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration, double statusInPercent,
            double distanceChange, double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, statusInPercent,
                    distanceChange, maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Fruchterman-Reingold";
        }
    }

    public class ISOMLayoutForCircles
        : ISOMLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public ISOMLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration, double statusInPercent, 
            double distanceChange, double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent, 
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, statusInPercent, 
                    distanceChange, maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "ISOM";
        }
    }

    public class KKLayoutForCircles
        : KKLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public KKLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration, double statusInPercent, 
            double distanceChange, double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent, 
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, statusInPercent, 
                    distanceChange, maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Kamada-Kawai";
        }
    }

    public class LinLogLayoutForCircles
        : LinLogLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public LinLogLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration, double statusInPercent, 
            double distanceChange, double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent, 
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, statusInPercent, 
                    distanceChange, maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Lin-Log";
        }
    }
}
