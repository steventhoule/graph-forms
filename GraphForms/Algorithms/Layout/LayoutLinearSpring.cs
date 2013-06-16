using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    public class LayoutLinearSpring : ILayoutSpring
    {
        private float mMult;

        public LayoutLinearSpring()
        {
            this.mMult = 0.5f;
        }

        public LayoutLinearSpring(float multiplier)
        {
            //if (multiplier <= 0 || multiplier > 1)
            //    throw new ArgumentOutOfRangeException("multiplier");
            if (multiplier <= 0)
                this.mMult = float.Epsilon;
            else if (multiplier > 1)
                this.mMult = 1;
            else
                this.mMult = multiplier;
        }

        public float Multiplier
        {
            get { return this.mMult; }
            set
            {
                if (this.mMult != value)
                {
                    //if (value <= 0 || value > 1)
                    //    throw new ArgumentOutOfRangeException("value");
                    if (value <= 0)
                        this.mMult = float.Epsilon;
                    else if (value > 1)
                        this.mMult = 1;
                    else
                        this.mMult = value;
                }
            }
        }

        public PointF GetSpringForce(float dx, float dy)
        {
            return new PointF(this.mMult * dx, this.mMult * dy);
        }
    }
}
