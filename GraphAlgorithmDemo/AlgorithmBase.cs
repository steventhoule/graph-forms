using System;
using System.Drawing;
using GraphForms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout;
using GraphForms.Algorithms.Layout.Circular;
using GraphForms.Algorithms.Layout.ForceDirected;
using GraphForms.Algorithms.Layout.Tree;

namespace GraphAlgorithmDemo
{
    public class ElasticLayoutForCircles
        : ElasticLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public ElasticLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public ElasticLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration, 
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
        }

        public override string ToString()
        {
            return "Simple Elastic";
        }
    }

    public class FRFreeLayoutForCircles
        : FRFreeLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FRFreeLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public FRFreeLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
        }

        public override string ToString()
        {
            return "Fruchterman-Reingold";
        }
    }

    public class FRBoundedLayoutForCircles
        : FRBoundedLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FRBoundedLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public FRBoundedLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
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

        public ISOMLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public ISOMLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
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

        public KKLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public KKLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
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

        public LinLogLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public LinLogLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
        }

        public override string ToString()
        {
            return "Lin-Log";
        }
    }

    public class FDSCircleLayoutForCircles
        : FDSingleCircleLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public FDSCircleLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public FDSCircleLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
        }

        public override string ToString()
        {
            return "Single Circle";
        }
    }

    public class BalloonTreeLayoutForCircles
        : BalloonTreeLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public BalloonTreeLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public BalloonTreeLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
        }

        public override string ToString()
        {
            return "Balloon Tree";
        }
    }

    public class SimpleTreeLayoutForCircles
        : SimpleTreeLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public SimpleTreeLayoutForCircles(CircleNodeScene scene,
            RectangleF boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
        }

        public SimpleTreeLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
        }

        protected override void OnEndIteration(uint iteration,
            double totalDistanceChange)
        {
            this.mScene.OnLayoutIterEnded(iteration,
                totalDistanceChange);
            base.OnEndIteration(iteration, totalDistanceChange);
        }

        public override string ToString()
        {
            return "Simple Tree";
        }
    }
}
