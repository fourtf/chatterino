using Chatterino.Common;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class TwitchEmote
    {
        public string Url { get; set; } = null;
        public string Name { get; set; } = null;
        public bool Animated { get; set; } = false;
        public Func<object> LoadAction { get; set; } = null;

        public string Tooltip { get; set; } = null;

        bool loading = false;
        private object image = null;

        public object Image
        {
            get
            {
                if (image != null)
                    return image;
                if (!loading)
                {
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
                                WebRequest request = WebRequest.Create(Url);
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
                        image = img;

                        if (img != null)
                        {
                            GuiEngine.Current.HandleAnimatedTwitchEmote(this);
                        }
                    }));
                }
                return null;
            }
        }
    }
}
