using Chatterino.Common;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class LazyLoadedImage
    {
        public string Url { get; set; } = null;
        public string Name { get; set; } = null;
        public bool IsAnimated { get; set; } = false;
        public Func<object> LoadAction { get; set; } = null;
        public bool IsHat { get; set; } = false;
        public bool HasTrailingSpace { get; set; } = true;
        public bool IsEmote { get; set; }

        public double Scale { get; set; } = 1;
        public Margin Margin { get; set; } = null;

        public string Tooltip { get; set; } = null;

        public bool IsDanke = false;

        bool loading = false;
        private object image = null;
        public object xd; 

        public object Image
        {
            get
            {
                if (image != null)
                    return image;

                if (loading) return null;

                loading = true;
                Task.Run((() =>
                {
                    object img;

                    if (LoadAction != null)
                    {
                        img = LoadAction();
                    }
                    else
                    {
                        try
                        {
                            var request = WebRequest.Create(Url);
                            if (AppSettings.IgnoreSystemProxy)
                            {
                                request.Proxy = null;
                            }
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                img = GuiEngine.Current.ReadImageFromStream(stream);
                            }

                            GuiEngine.Current.FreezeImage(img);
                        }
                        catch
                        {
                            img = null;
                        }
                    }
                    if (img != null)
                    {
                        GuiEngine.Current.HandleAnimatedTwitchEmote(this, img);
                    }
                    image = img;

                    GuiEngine.Current.TriggerEmoteLoaded();
                }));
                return null;
            }
        }

        public LazyLoadedImage()
        {
            
        }

        public LazyLoadedImage(object image)
        {
            this.image = image;
            loading = false;
        }
    }
}
