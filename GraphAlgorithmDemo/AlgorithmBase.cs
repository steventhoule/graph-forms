using System;
using GraphForms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout.ForceDirected;
using GraphForms.Algorithms.Layout.Circular;

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

        protected override bool OnIterationEnded(int iteration, 
            double statusInPercent, double distanceChange, 
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent, 
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, 
                    statusInPercent, distanceChange, 
                    maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Simple Elastic";
        }
    }

    public class FRFreeLayoutForCircles
        : FRLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FRFreeLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration, 
            double statusInPercent, double distanceChange, 
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration, 
                    statusInPercent, distanceChange, 
                    maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Fruchterman-Reingold";
        }
    }

    public class FRBoundedLayoutForCircles
        : FRLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FRBoundedLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override FRLayoutParameters DefaultParameters
        {
            get { return new FRBoundedLayoutParameters(); }
        }

        protected override bool OnIterationEnded(int iteration,
            double statusInPercent, double distanceChange,
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration,
                    statusInPercent, distanceChange,
                    maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Bounded Fruchterman-Reingold";
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

        protected override bool OnIterationEnded(int iteration,
            double statusInPercent, double distanceChange,
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration,
                    statusInPercent, distanceChange,
                    maxDistanceChange, message);
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

        protected override bool OnIterationEnded(int iteration,
            double statusInPercent, double distanceChange,
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration,
                    statusInPercent, distanceChange,
                    maxDistanceChange, message);
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

        protected override bool OnIterationEnded(int iteration,
            double statusInPercent, double distanceChange,
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration,
                    statusInPercent, distanceChange,
                    maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Lin-Log";
        }
    }

    public class FDSingleCircleLayoutForCircles
        : FDSingleCircleLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FDSingleCircleLayoutForCircles(CircleNodeScene scene)
            : base(scene.Graph, null)
        {
            this.mScene = scene;
        }

        protected override bool OnIterationEnded(int iteration,
            double statusInPercent, double distanceChange,
            double maxDistanceChange, string message)
        {
            bool keep = false;
            if (base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message))
            {
                keep = this.mScene.OnLayoutIterEnded(iteration,
                    statusInPercent, distanceChange,
                    maxDistanceChange, message);
            }
            return keep;
        }

        public override string ToString()
        {
            return "Single Circle";
        }
    }
}
