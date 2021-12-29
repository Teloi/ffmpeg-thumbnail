using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg_Thumbnail
{
    internal class ImageHelper
    {
        public static Image ResizeImage(Image img, int width, int height, bool keepRatio = true)
        {
            if (img == null)
            {
                return null;
            }
            Bitmap bmp = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(System.Drawing.Color.Black);
                if (keepRatio)
                {
                    Rectangle ns = getNewSize(img, width, height);
                    graphics.DrawImage(img, ns, new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                }
                else
                {
                    graphics.DrawImage(img, new Rectangle(0, 0, width, height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                }
            }
            return bmp;
        }

        private static Rectangle getNewSize(Image img, int width, int height)
        {
            double ratio = Math.Max(img.Width / width * 1.0, img.Height / height * 1.0);
            Size ns = new Size((int)(img.Width / ratio), (int)(img.Height / ratio));
            Point offset = new Point((width - ns.Width) / 2, (height - ns.Height) / 2);

            return new Rectangle(offset, ns);
        }
    }
}
