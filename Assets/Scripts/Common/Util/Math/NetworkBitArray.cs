using System;
using System.Runtime.CompilerServices;
using Fusion;

[Serializable]
public unsafe struct NetworkBitArray : INetworkStruct
{
    public const int BitCount = 2048;
    private const int ByteCount = BitCount / 8;

    private fixed byte data[ByteCount];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetBit(int index)
    {
        int byteIndex = index >> 3;
        int bitIndex = index & 7;
        return (data[byteIndex] & (1 << bitIndex)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int index, bool value)
    {
        int byteIndex = index >> 3;
        int bitIndex = index & 7;
        byte mask = (byte)(1 << bitIndex);

        if (value)
        {
            data[byteIndex] |= mask;
        }
        else
        {
            data[byteIndex] &= (byte)~mask;
        }
    }
}
