using System;

[Serializable]
public readonly struct Half : Fusion.INetworkStruct
{
    private readonly ushort value;

    public Half(float f)
    {
        value = FloatToHalf(f);
    }

    public static explicit operator float(Half h)
    {
        return HalfToFloat(h.value);
    }

    public static implicit operator Half(float f)
    {
        return new Half(f);
    }

    private static unsafe ushort FloatToHalf(float f)
    {
        uint bits = *(uint*)&f;
        
        uint sign = (bits >> 16) & 0x8000;
        int exp = (int)((bits >> 23) & 0xFF) - 127 + 15;
        uint mant = bits & 0x007FFFFF;

        if (exp <= 0) return (ushort)sign;
        else if (exp >= 31) return (ushort)(sign | 0x7C00);

        mant >>= 13;
        return (ushort)(sign | ((uint)exp << 10) | mant);
    }

    private static unsafe float HalfToFloat(ushort h)
    {
        uint sign = (uint)(h & 0x8000) << 16;
        uint exp = (uint)(h & 0x7C00) >> 10;
        uint mant = (uint)(h & 0x03FF);

        if (exp == 0)
        {
            if (mant == 0)
                return *(float*)&sign;
            else
            {
                while ((mant & 0x400) == 0)
                {
                    mant <<= 1;
                    exp--;
                }
                mant &= 0x3FF;
                exp++;
            }
        }
        else if (exp == 31) exp = 255;
        else exp = exp - 15 + 127;

        uint bits = sign | (exp << 23) | (mant << 13);
        return *(float*)&bits;
    }
}