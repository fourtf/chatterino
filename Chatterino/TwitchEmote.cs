using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public class TwitchEmote
    {
        public string Url { get; set; } = null;
        public string Name { get; set; } = null;
        public bool Animated { get; private set; } = false;
        public Func<Image> LoadAction { get; set; } = null;

        bool loading = false;
        private Image image = null;

        private FrameDimension dimension;
        private int frameCount;
        private int currentFrame = 0;

        public Image Image
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
                        Image img;

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
                                    img = Image.FromStream(stream);
                                }
                            }
                            catch
                            {
                                img = null;
                            }
                        }
                        image = img;

                        if (img != null)
                        {
                            lock (img)
                                Animated = ImageAnimator.CanAnimate(img);

                            if (Animated)
                            {
                                dimension = new FrameDimension(image.FrameDimensionsList[0]);
                                frameCount = image.GetFrameCount(dimension);

                                App.UpdateGifEmotes += (s, e) =>
                                {
                                    currentFrame += 1;

                                    if (currentFrame >= frameCount)
                                    {
                                        currentFrame = 0;
                                    }

                                    lock (image)
                                    {
                                        image.SelectActiveFrame(dimension, currentFrame);
                                    }
                                };
                            }
                            App.TriggerEmoteLoaded();
                        }
                    }));
                }
                return null;
            }
        }
    }
}
