using System;

namespace Chatterino.Common
{
    public struct HSLColor
    {
        private float hue;

        public float Hue
        {
            get { return hue; }
        }

        private float saturation;

        public float Saturation
        {
            get { return saturation; }
            private set
            {
                saturation = saturation > 1 ? 1 : (saturation < 0 ? 0 : value);
            }
        }

        private float luminosity;

        public float Luminosity
        {
            get { return luminosity; }
            private set
            {
                luminosity = luminosity > 1 ? 1 : (luminosity < 0 ? 0 : value);
            }
        }

        public HSLColor(float h, float s, float l)
        {
            hue = h > 1 ? 1 : (h < 0 ? 0 : h);
            saturation = s > 1 ? 1 : (s < 0 ? 0 : s);
            luminosity = l > 1 ? 1 : (l < 0 ? 0 : l);
        }

        public HSLColor WithHue(float hue) => new HSLColor(hue, saturation, luminosity);
        public HSLColor WithSaturation(float saturation) => new HSLColor(hue, saturation, luminosity);
        public HSLColor WithLuminosity(float luminosity) => new HSLColor(hue, saturation, luminosity);

        public override string ToString()
        {
            return $"H: {Hue:#0.##} S: {Saturation:#0.##} L: {Luminosity:#0.##}";
        }

        public static HSLColor FromRGB(float red, float green, float blue)
        {
            float r = red;
            float g = green;
            float b = blue;
            float v;
            float m;
            float vm;
            float r2, g2, b2;

            float h = 0; // default to black
            float s = 0;
            float l = 0;
            v = Math.Max(r, g);
            v = Math.Max(v, b);
            m = Math.Min(r, g);
            m = Math.Min(m, b);
            l = (m + v) / 2f;
            if (l <= 0.0)
            {
                return new HSLColor(h, s, l);
            }
            vm = v - m;
            s = vm;
            if (s > 0.0)
            {
                s /= (l <= 0f) ? (v + m) : (2f - v - m);
            }
            else
            {
                return new HSLColor(h, s, l);
            }
            r2 = (v - r) / vm;
            g2 = (v - g) / vm;
            b2 = (v - b) / vm;
            if (r == v)
            {
                h = (g == m ? 5f + b2 : 1f - g2);
            }
            else if (g == v)
            {
                h = (b == m ? 1f + r2 : 3f - b2);
            }
            else
            {
                h = (r == m ? 3f + g2 : 5f - r2);
            }
            h /= 6f;
            return new HSLColor(h, s, l);
        }

        public static HSLColor FromRGB(int color)
        {
            return unchecked(FromRGB((byte)(color >> 16) / 255f, (byte)((color >> 8) & 255) / 255f, (byte)(color & 255) / 255f));
        }

        public void ToRGB(out float r, out float g, out float b)
        {
            float v, hue = this.hue;

            r = luminosity;
            g = luminosity;
            b = luminosity;

            v = (luminosity <= 0f) ? (luminosity * (1f + saturation)) : (luminosity + saturation - luminosity * saturation);

            if (v > 0)
            {
                float m;
                float sv;
                int sextant;
                float fract, vsf, mid1, mid2;

                m = luminosity + luminosity - v;
                sv = (v - m) / v;
                hue *= 6f;
                sextant = (int)hue;
                fract = hue - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;

                switch (sextant)
                {
                    case 0:
                    case 6:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
        }
    }
}
