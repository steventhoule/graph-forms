using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class BalloonTreeLayoutParameters : LayoutParameters
    {
        private int mMinRadius = 2;
        private float mBorder = 20.0f;

        public int MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                if (value != this.mMinRadius)
                {
                    this.mMinRadius = value;
                    this.OnPropertyChanged("MinRadius");
                }
            }
        }


        public float Border
        {
            get { return this.mBorder; }
            set
            {
                if (value != this.mBorder)
                {
                    this.mBorder = value;
                    this.OnPropertyChanged("Border");
                }
            }
        }

        public BalloonTreeLayoutParameters()
            : base(0, 0, 300, 300)
        {
        }
    }
}
