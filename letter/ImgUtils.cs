using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Drawing;

namespace Dzik.letters.utils
{
    internal class ImgUtils
    {
        internal static void SetHighQualityScalingFor(DependencyObject obj)
        {
            RenderOptions.SetBitmapScalingMode(obj, BitmapScalingMode.HighQuality);
        }

        internal static ImageSource LoadImageWithCorrectRotation(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == ".jpg" || ext == ".jpeg")
            {
                var rotation = GetImageRotation(path);

                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(path);
                switch (rotation)
                {
                    case DataRotation.left:
                        img.Rotation = Rotation.Rotate90;
                        break;
                    case DataRotation.right:
                        img.Rotation = Rotation.Rotate180;
                        break;
                    case DataRotation.down:
                        img.Rotation = Rotation.Rotate270;
                        break;
                }
                img.EndInit();
                return img;
            }
            return new BitmapImage(new Uri(path));
        }

        // 274 ?
        private const int exifOrientationID = 0x112;

        private const int exifOrientationLeft = 6;
        private const int exifOrientationRight = 3;
        private const int exifOrientationDown = 8;

        private static DataRotation GetImageRotation(string path)
        {
            var img = new Bitmap(path);

            // the check is necessary, faulty query of a non-existent prop won't produce a null somehow...
            if (!img.PropertyIdList.Contains(exifOrientationID)) return DataRotation.none;

            var prop = img.GetPropertyItem(exifOrientationID);
            if (prop?.Value == null) return DataRotation.none;

            int val = BitConverter.ToUInt16(prop.Value, 0);

            switch (val)
            {
                case exifOrientationLeft: return DataRotation.left;
                case exifOrientationRight: return DataRotation.right;
                case exifOrientationDown: return DataRotation.down;
                default: return DataRotation.none;
            }
        }

        enum DataRotation
        {
            left,
            right,
            down,
            none
        }
    }
}
