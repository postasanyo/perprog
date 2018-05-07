using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parallel_solution
{
    /// <summary>
    /// Stores the index of the frame and the bitmap.
    /// </summary>
    class OneFrame
    {
        public int IndexOfFrame { get; set; }
        public Bitmap BMP { get; set; }

        public OneFrame() { }

        public OneFrame(int indexOfFrame, Bitmap bmpData)
        {
            IndexOfFrame = indexOfFrame;
            BMP = bmpData;
        }
    }
}
