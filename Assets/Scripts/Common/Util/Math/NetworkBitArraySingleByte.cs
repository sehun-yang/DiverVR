using System;
using System.Runtime.CompilerServices;
using Fusion;

[Serializable]
public unsafe struct NetworkBitArraySingleByte : INetworkStruct
{
    private byte data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool GetBit(int index)
    {
        int bitIndex = index & 7;
        return (data & (1 << bitIndex)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int index, bool value)
    {
        int bitIndex = index & 7;
        byte mask = (byte)(1 << bitIndex);

        if (value)
        {
            data |= mask;
        }
        else
        {
            data &= (byte)~mask;
        }
    }
}
