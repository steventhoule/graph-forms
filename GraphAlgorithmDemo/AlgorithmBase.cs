﻿using System;
using System.Drawing;
using GraphForms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Collections;
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
            Box2F boundingBox)
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
            Box2F boundingBox)
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
            Box2F boundingBox)
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
            Box2F boundingBox)
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
            Box2F boundingBox)
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
            Box2F boundingBox)
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

    public class SCircleLayoutForCircles
        : SingleCircleLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public SCircleLayoutForCircles(CircleNodeScene scene,
            Box2F boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
            this.CenterX = boundingBox.X + boundingBox.W / 2;
            this.CenterY = boundingBox.Y + boundingBox.H / 2;
            this.AdaptToSizeChanges = true;
        }

        public SCircleLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
            Box2F bbox = clusterNode.LayoutBBox;
            this.CenterX = bbox.X + bbox.W / 2;
            this.CenterY = bbox.Y + bbox.H / 2;
            this.AdaptToSizeChanges = true;
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

    public class BalloonCirclesLayoutForCircles
        : BalloonCirclesLayoutAlgorithm2<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public BalloonCirclesLayoutForCircles(CircleNodeScene scene,
            Box2F boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
            this.AdaptToSizeChanges = true;
        }

        public BalloonCirclesLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
            this.AdaptToSizeChanges = true;
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
            return "Balloon Circles";
        }
    }

    public class BalloonTreeLayoutForCircles
        : BalloonTreeLayoutAlgorithm<CircleNode, ArrowEdge>
    {
        private CircleNodeScene mScene;

        public BalloonTreeLayoutForCircles(CircleNodeScene scene,
            Box2F boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
            this.AdaptToSizeChanges = true;
        }

        public BalloonTreeLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
            this.AdaptToSizeChanges = true;
        }

        public GTree<CircleNode, ArrowEdge, CircleGeom<CircleNode, ArrowEdge>> CircleTree
        {
            get { return this.DataTree; }
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
            Box2F boundingBox)
            : base(scene.Graph, boundingBox)
        {
            this.mScene = scene;
            this.AdaptToSizeChanges = true;
        }

        public SimpleTreeLayoutForCircles(CircleNodeScene scene,
            IClusterNode clusterNode)
            : base(scene.Graph, clusterNode)
        {
            this.mScene = scene;
            this.AdaptToSizeChanges = true;
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
