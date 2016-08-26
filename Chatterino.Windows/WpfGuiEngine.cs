using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Xwt;

namespace Chatterino.Windows
{
    public class WpfGuiEngine : Desktop.XwtGuiEngine
    {
        PropertyInfo framesProperty = null;
        PropertyInfo imageSourceProperty = null;

        public override void FreezeImage(object obj)
        {
            var t = Toolkit.GetBackend(obj);

            if (framesProperty == null)
            {
                framesProperty = t.GetType().GetProperty("Frames");
            }

            var array = ((IEnumerable)framesProperty.GetValue(t)).Cast<object>().ToArray();

            foreach (object o in array)
            {
                if (imageSourceProperty == null)
                {
                    imageSourceProperty = o.GetType().GetProperty("ImageSource");
                }

                BitmapImage img = (BitmapImage)imageSourceProperty.GetValue(o);

                while (!img.CanFreeze)
                    ;

                img.Freeze();
            }
        }
    }
}
