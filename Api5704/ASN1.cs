#region License
/*
Copyright 2024 Dmitrii Evdokimov
Open source software

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System.Text;

namespace Api5704;

public static class ASN1
{
    public static int BufferSize { get; set; } = 4096;

    public static async Task<byte[]> CleanSignAsync(byte[] signedData)
    {
        try
        {
            using var stream = new MemoryStream(signedData);
            using var reader = new BinaryReader(stream);

            // type 0x30, length 0x80 or 1..4 bytes additionally
            SkipTypeLength();

            // type 0x06 length 0x09 data... - ObjectIdentifier (signedData "1.2.840.113549.1.7.2")
            if (!ReadOid(0x02))
                return [];

            // 0xA0 0x80 0xA0 0x80
            SkipTypeLength(2);

            // 0x02 0x01 0x01 - Integer (version 1)
            if (!ReadVersion())
                return [];

            // 0x31 ... - list of used algoritms
            SkipTypeLengthData();

            // 0x30 0x80
            SkipTypeLength();

            // type 0x06 length 0x09 data... - ObjectIdentifier (data "1.2.840.113549.1.7.1")
            if (!ReadOid(0x01))
                return [];

            // 0xA0 0x80 0x24 0x80
            SkipTypeLength(2);

            // type 0x04 - OctetString
            if (reader.ReadByte() != 0x04)
                return [];

            // length of enclosed data (long or undefined)
            var len = ReadLength();

            if (len is null) // undefined
            {
                var start = stream.Position;
                var end = Seek(stream, [0x00, 0x00]);

                len = end - start;
                stream.Position = start;
            }

            // start of enclosed data
            using var output = new MemoryStream();
                await stream.CopyToAsync(output); //TODO copy len bytes only
                output.SetLength((long)len); // truncate tail

            return output.ToArray();

            #region local functions
            // 1..5 bytes
            long? ReadLength()
            {
                byte b = reader.ReadByte();

                // undefined: end by 0x00 0x00 bytes
                if (b == 0x80)
                    return null;

                // 1 next byte: 128..255
                if (b == 0x81)
                {
                    var v = reader.ReadByte();
                    return v;
                }

                // 2 next bytes: 256..65535
                if (b == 0x82)
                {
                    var v = reader.ReadBytes(2);
                    return
                        v[0] * 0x100 +
                        v[1];
                }

                // 3 next bytes: 65536..16777215
                if (b == 0x83)
                {
                    var v = reader.ReadBytes(3);
                    return
                        v[0] * 0x10000 +
                        v[1] * 0x100 +
                        v[2];
                }

                // 4 next bytes, 2 standards:
                // 1 .. 4 294 967 295
                // 16 777 216 .. 4 294 967 295 (4 Gb)
                if (b == 0x84)
                {
                    var v = reader.ReadBytes(4);
                    return
                        v[0] * 0x1000000 +
                        v[1] * 0x10000 +
                        v[2] * 0x100 +
                        v[3];
                }

                // this byte: 0..127
                else
                    return b;
            }

            // 06 09 then
            // 2A 86 48 86 F7 0D 01 07 02 - oid 1.2.840.113549.1.7.2 "signedData"
            // 2A 86 48 86 F7 0D 01 07 01 - oid 1.2.840.113549.1.7.1 "data"
            bool ReadOid(byte n)
            {
                var b = reader.ReadBytes(11);
                int i = 0;
                return
                    b[i++] == 0x06 && // type 06 => ObjectIdentifier
                    b[i++] == 0x09 && // length => 9 bytes

                    b[i++] == 0x2A && // data ...
                    b[i++] == 0x86 &&
                    b[i++] == 0x48 &&
                    b[i++] == 0x86 &&
                    b[i++] == 0xF7 &&
                    b[i++] == 0x0D &&
                    b[i++] == 0x01 &&
                    b[i++] == 0x07 &&
                    b[i++] == n;
            }

            // 02 01 01
            bool ReadVersion()
            {
                var b = reader.ReadBytes(3);
                int i = 0;
                return
                    b[i++] == 0x02 && // type 02 => Integer
                    b[i++] == 0x01 && // length => 1 byte
                    b[i++] == 0x01;   // data (1 => version 1)
            }

            // skip type and length
            // 30 80
            // 02 01
            void SkipTypeLength(int n = 1)
            {
                for (int i = 0; i < n; i++)
                {
                    //type
                    stream.Position++;

                    //length
                    byte b = reader.ReadByte();

                    if (b > 0x80)
                    {
                        stream.Position += b - 0x80;
                    }
                }
            }

            // skip type, length and data by this length
            // 02 01 01
            void SkipTypeLengthData()
            {
                // type
                stream.Position++;

                // length
                var len = ReadLength();

                //data
                if (len is null) // undefined
                {
                    var end = Seek(stream, [0x00, 0x00]);
                    stream.Position = end + 2;
                }
                else
                {
                    stream.Position += (long)len;
                }
            }
            #endregion local functions
        }
        catch
        {
            return [];
        }
    }

    // https://keestalkstech.com/2010/11/seek-position-of-a-string-in-a-file-or-filestream/
    // Written by Kees C. Bakker, updated on 2022-09-18

    /* EXAMPLE:
        var url = "https://keestalkstech.com/wp-content/uploads/2020/06/photo-with-xmp.jpg?1";

        using var client = new HttpClient();
        using var downloadStream = await client.GetStreamAsync(url);

        using var stream = new MemoryStream();
        await downloadStream.CopyToAsync(stream);

        stream.Position = 0;
        var enc = Encoding.UTF8;
        var start = Seek(stream, "<x:xmpmeta", enc);
        var end = Seek(stream, "<?xpacket", enc);

        stream.Position = start;
        var buffer = new byte[end - start];
        stream.Read(buffer, 0, buffer.Length);
        var xmp = enc.GetString(buffer);
    */

    public static long Seek(Stream stream, string str, Encoding encoding)
    {
        var search = encoding.GetBytes(str);
        return Seek(stream, search);
    }

    public static long Seek(Stream stream, byte[] search)
    {
        int bufferSize = BufferSize;

        if (bufferSize < search.Length * 2)
            bufferSize = search.Length * 2;

        var buffer = new byte[bufferSize];
        var size = bufferSize;
        var offset = 0;
        var position = stream.Position;

        while (true)
        {
            var r = stream.Read(buffer, offset, size);

            // when no bytes are read -- the string could not be found
            if (r <= 0)
                return -1;

            // when less then size bytes are read, we need to slice
            // the buffer to prevent reading of "previous" bytes
            ReadOnlySpan<byte> ro = buffer;

            if (r < size)
                ro = ro[..(offset + size)];

            // check if we can find our search bytes in the buffer
            var i = ro.IndexOf(search);

            if (i > -1)
                return position + i;

            // when less then size was read, we are done and found nothing
            if (r < size)
                return -1;

            // we still have bytes to read, so copy the last search
            // length to the beginning of the buffer. It might contain
            // a part of the bytes we need to search for

            offset = search.Length;
            size = bufferSize - offset;
            Array.Copy(buffer, buffer.Length - offset, buffer, 0, offset);
            position += bufferSize - offset;
        }
    }

    public static byte[] Oid(string oid)
    {
        string[] parts = oid.Split('.');
        MemoryStream ms = new();

        //0
        int x = int.Parse(parts[0]);

        //1
        int y = int.Parse(parts[1]);
        ms.WriteByte((byte)(40 * x + y));

        //2+
        for (int i = 2; i < parts.Length; i++)
        {
            string part = parts[i];
            int octet = int.Parse(part);
            byte[] b = EncodeOctet(octet);
            ms.Write(b, 0, b.Length);
        }

        return ms.ToArray();
    }

    public static string Oid(byte[] oid)
    {
        StringBuilder sb = new();

        // Pick apart the OID
        int x = oid[0] / 40;
        int y = oid[0] % 40;
        
        if (x > 2)
        {
            // Handle special case for large y if x = 2
            y += (x - 2) * 40;
            x = 2;
        }
        
        sb.Append(x).Append('.').Append(y);
        long val = 0;
        
        for (int i = 1; i < oid.Length; i++)
        {
            val = (val << 7) | ((byte)(oid[i] & 0x7F));
        
            if ((oid[i] & 0x80) != 0x80)
            {
                sb.Append('.').Append(val);
                val = 0;
            }
        }
        
        return sb.ToString();
    }

    public static byte[] EncodeOctet(int value)
    {
        /*
           For example, the OID value is 19200300.

           1. Convert 19200300 to Hex 0x124F92C
           2.   0x124F92C        & 0x7F         = 0x2C -- Last Byte
           3. ((0x124F92C >> 7)  & 0x7F) | 0x80 = 0xF2 --- 3rd Byte
           4. ((0x124F92C >> 14) & 0x7F) | 0x80 = 0x93 ---- 2nd Byte
           5. ((0x124F92C >> 21) & 0x7F) | 0x80 = 0x89 ----- 1st Byte

           So after encoding, it becomes 0x89 0x93 0xF2 0x2C.
        */

        uint x = (uint)value;

        if (x > 0xFFFFFF) // 4 Bytes
        {
            var b = new byte[4];

            b[0] = Convert.ToByte((x >> 21) & 0x7F | 0x80);
            b[1] = Convert.ToByte((x >> 14) & 0x7F | 0x80);
            b[2] = Convert.ToByte((x >>  7) & 0x7F | 0x80);
            b[3] = Convert.ToByte( x        & 0x7F);

            return b;
        }

        if (x > 0xFFFF) // 3 Bytes
        {
            var b = new byte[3];

            b[0] = Convert.ToByte((x >> 14) & 0x7F | 0x80);
            b[1] = Convert.ToByte((x >>  7) & 0x7F | 0x80);
            b[2] = Convert.ToByte( x        & 0x7F);

            return b;
        }

        if (x > 127) // 2 Bytes
        {
            var b = new byte[2];

            b[0] = Convert.ToByte((x >> 7) & 0x7F | 0x80);
            b[1] = Convert.ToByte( x       & 0x7F);

            return b;
        }

        // (x < 127) // 1 Byte
        return [Convert.ToByte(x)];
    }
}

/*
using System.Security.Cryptography.Pkcs;

/// <summary>
/// Извлечь из PKCS#7 с ЭП чистый исходный текст.
/// Криптопровайдер и проверка ЭП здесь не используются - только извлечение блока данных из формата ASN.1
/// </summary>
/// <param name="data">Массив байтов с сообщением в формате PKCS#7.</param>
/// <returns>Массив байтов с исходным сообщением без ЭП.</returns>
public static byte[] CleanSign(byte[] data)
{
    var signedCms = new SignedCms();
    signedCms.Decode(data);

    return signedCms.ContentInfo.Content;
}

/// <summary>
/// Извлечь из файла PKCS#7 с ЭП чистый исходный файл.
/// Криптопровайдер и проверка ЭП здесь не используются - только извлечение блока данных из формата ASN.1
/// </summary>
/// <param name="src">Исходный файл.</param>
/// <param name="dst">Файл с результатом.</param>
/// <returns></returns>
public static async Task CleanSignAsync(string src, string dst)
{
    byte[] data = await File.ReadAllBytesAsync(src);
    byte[] data2 = CleanSign(data);
    await File.WriteAllBytesAsync(dst, data2);
}
*/
