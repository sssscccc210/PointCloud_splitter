using System;

public static class ColorUtility
{
    // cf. https://kaizoudou.com/from-rgb-to-lab-color-space/


    /// <summary>
    /// [0, 255] のsRGBまたはlinear RGBを[0, 1]に正規化する
    /// </summary>
    /// <param name="r">R (0~255)</param>
    /// <param name="g">G (0~255)</param>
    /// <param name="b">B (0~255)</param>
    /// <returns>正規化されたRGB値（[0, 1]）</returns>
    public static (float, float, float) NormalizeRgb(byte r, byte g, byte b)
    {
        return (r / 255f, g / 255f, b / 255f);
    }


    /// <summary>
    /// [0, 1]のsRGBを linear RGBに変換する関数
    /// </summary>
    /// <param name="r">R (0~1)</param>
    /// <param name="g">G (0~1)</param>
    /// <param name="b">B (0~1)</param>
    /// <returns></returns>
    public static (float, float, float) LinearizeSrgb(float r, float g, float b)
    {
        if (r <= 0.04045f) r /= 12.92f;
        else r = MathF.Pow((r + 0.055f) / 1.055f, 2.4f);

        if (g <= 0.04045f) g /= 12.92f;
        else g = MathF.Pow((g + 0.055f) / 1.055f, 2.4f);

        if (b <= 0.04045f) b /= 12.92f;
        else b = MathF.Pow((b + 0.055f) / 1.055f, 2.4f);

        return (r, g, b);
    }

    /// <summary>
    /// [0, 1]のsRGBを linear RGBに変換する関数
    /// </summary>
    /// <param name="t">turple of (float r, float g, float b)</param>
    /// <returns></returns>
    public static (float, float, float) LinearizeSrgb((float r, float g, float b) t)
    {
        return LinearizeSrgb(t.r, t.g, t.b);
    }


    /// <summary>
    /// [0, 1]のlinear RGBをXYZ色空間に変換する関数
    /// </summary>
    /// <param name="r">R (0~1)</param>
    /// <param name="g">G (0~1)</param>
    /// <param name="b">B (0~1)</param>
    /// <returns></returns>
    public static (float, float, float) LinearRgb2Xyz(float r, float g, float b)
    {
        float x = 0.4124564f * r + 0.3575761f * g + 0.1804375f * b;
        float y = 0.2126729f * r + 0.7151522f * g + 0.0721750f * b;
        float z = 0.0193339f * r + 0.1191920f * g + 0.9503041f * b;

        return (x, y, z);
    }

    /// <summary>
    /// [0, 1]のlinear RGBをXYZ色空間に変換する関数
    /// </summary>
    /// <param name="t">turple of (float r, float g, float b)</param>
    /// <returns></returns>
    public static (float, float, float) LinearRgb2Xyz((float r, float g, float b) t)
    {
        return LinearRgb2Xyz(t.r, t.g, t.b);
    }



    private static float FuncForXyz2Lab(float t)
    {
        if (t > 0.008856f) return MathF.Pow(t, 0.3333333f);
        else return 7.787f * t + 0.137931f;
    }

    /// <summary>
    /// XYZ色空間をL*a*b*色空間に変換する関数
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <returns></returns>
    public static (float, float, float) Xyz2Lab(float x, float y, float z)
    {
        float l = 116 * FuncForXyz2Lab(y) - 16;
        float a = 500 * (FuncForXyz2Lab(x / 0.95047f) - FuncForXyz2Lab(y));
        float b = 200 * (FuncForXyz2Lab(y) - FuncForXyz2Lab(z / 1.08883f));

        return (l, a, b);
    }

    /// <summary>
    /// XYZ色空間をL*a*b*色空間に変換する関数
    /// </summary>
    /// <param name="t">turple of (float x, float y, float z)</param>
    /// <returns></returns>
    public static (float, float, float) Xyz2Lab((float x, float y, float z) t)
    {
        return Xyz2Lab(t.x, t.y, t.z);
    }



    /// <summary>
    /// [0, 255]のsRGB色空間からL*a*b*色空間に変換する関数
    /// </summary>
    /// <param name="r">R (0~255)</param>
    /// <param name="g">G (0~255)</param>
    /// <param name="b">B (0~255)</param>
    /// <returns></returns>
    public static (float, float, float) Srgb2Lab(byte r, byte g, byte b)
    {
        return Xyz2Lab(LinearRgb2Xyz(LinearizeSrgb(NormalizeRgb(r, g, b))));
    }
}
