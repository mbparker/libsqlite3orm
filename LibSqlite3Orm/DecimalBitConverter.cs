namespace LibSqlite3Orm;

public static class DecimalBitConverter
{
    public static decimal ToDecimal(byte[] buffer, int offset = 0)
    {
        var decimalBits = new int[4];

        decimalBits[0] = buffer[offset + 0] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
        decimalBits[1] = buffer[offset + 4] | (buffer[offset + 5] << 8) | (buffer[offset + 6] << 16) | (buffer[offset + 7] << 24);
        decimalBits[2] = buffer[offset + 8] | (buffer[offset + 9] << 8) | (buffer[offset + 10] << 16) | (buffer[offset + 11] << 24);
        decimalBits[3] = buffer[offset + 12] | (buffer[offset + 13] << 8) | (buffer[offset + 14] << 16) | (buffer[offset + 15] << 24);

        return new decimal(decimalBits);
    }

    public static byte[] GetBytes(decimal number)
    {
        var decimalBuffer = new byte[16];

        var decimalBits = decimal.GetBits(number);

        var lo = decimalBits[0];
        var mid = decimalBits[1];
        var hi = decimalBits[2];
        var flags = decimalBits[3];

        decimalBuffer[0] = (byte)lo;
        decimalBuffer[1] = (byte)(lo >> 8);
        decimalBuffer[2] = (byte)(lo >> 16);
        decimalBuffer[3] = (byte)(lo >> 24);

        decimalBuffer[4] = (byte)mid;
        decimalBuffer[5] = (byte)(mid >> 8);
        decimalBuffer[6] = (byte)(mid >> 16);
        decimalBuffer[7] = (byte)(mid >> 24);

        decimalBuffer[8] = (byte)hi;
        decimalBuffer[9] = (byte)(hi >> 8);
        decimalBuffer[10] = (byte)(hi >> 16);
        decimalBuffer[11] = (byte)(hi >> 24);

        decimalBuffer[12] = (byte)flags;
        decimalBuffer[13] = (byte)(flags >> 8);
        decimalBuffer[14] = (byte)(flags >> 16);
        decimalBuffer[15] = (byte)(flags >> 24);

        return decimalBuffer;
    }
}