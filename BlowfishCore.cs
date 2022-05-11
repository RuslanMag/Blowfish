using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blowfish
{
    internal class BlowfishCore
    {
        const int N = 16;

        private uint[,] _s;
        private uint[] _p;

        private void KeyExtention(byte[] key)
        {
            short i;
            short j = 0;
            _p = (uint[])Constants.P.Clone();
            _s = (uint[,])Constants.S.Clone();

            for (i = 0; i < N + 2; i++)
            {
                uint data = 0;
                short k;
                for (k = 0; k < 4; k++)
                {
                    data = (data << 8) | key[j];
                    j++;
                    if (j >= key.Length)
                    {
                        j = 0;
                    }
                }
                _p[i] ^= data;
            }

            uint dataL = 0;
            uint dataR = 0;

            for (i = 0; i < N + 2; i += 2)
            {
                (dataL, dataR) = Encipher(dataL, dataR);
                _p[i] = dataL;
                _p[i + 1] = dataR;
            }

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 256; j += 2)
                {
                    (dataL, dataR) = Encipher(dataL, dataR);
                    _s[i, j] = dataL;
                    _s[i, j + 1] = dataR;
                }
            }
        }

        public void KeyExtention(string key)
        {
            byte[] data = Encoding.ASCII.GetBytes(key);
            KeyExtention(data);
        }

        private uint F(uint x)
        {
            var d = (ushort)(x & 0x00FF);
            x >>= 8;
            var c = (ushort)(x & 0x00FF);
            x >>= 8;
            var b = (ushort)(x & 0x00FF);
            x >>= 8;
            var a = (ushort)(x & 0x00FF);

            var y = _s[0, a] + _s[1, b];
            y ^= _s[2, c];
            y += _s[3, d];

            return y;
        }

        private void Encipher(byte[] data, int length)
        {
            if (length % 8 != 0)
                throw new Exception("Invalid Length");

            for (int i = 0; i < length; i += 8)
            {
                var xl = (uint)((data[i] << 24) |
                                (data[i + 1] << 16) |
                                (data[i + 2] << 8) |
                                data[i + 3]);
                var xr = (uint)((data[i + 4] << 24) |
                                (data[i + 5] << 16) |
                                (data[i + 6] << 8) |
                                data[i + 7]);
                (xl, xr) = Encipher(xl, xr);

                data[i] = (byte)(xl >> 24);
                data[i + 1] = (byte)(xl >> 16);
                data[i + 2] = (byte)(xl >> 8);
                data[i + 3] = (byte)(xl);
                data[i + 4] = (byte)(xr >> 24);
                data[i + 5] = (byte)(xr >> 16);
                data[i + 6] = (byte)(xr >> 8);
                data[i + 7] = (byte)(xr);
            }
        }

        private (uint data_l, uint data_r) Encipher(uint xl, uint xr)
        {
            short i;

            var xL = xl;
            var xR = xr;

            for (i = 0; i < N; ++i)
            {
                xL ^= _p[i];
                xR = F(xL) ^ xR;

                (xL, xR) = Swap(xL, xR);
            }

            (xL, xR) = Swap(xL, xR);

            xR ^= _p[N];
            xL ^= _p[N + 1];

            xl = xL;
            xr = xR;

            return (xl, xr);
        }

        public string Encipher(string data)
        {
            var b = Encoding.ASCII.GetBytes(data);

            if (b.Length % 8 != 0)
                b = b.Concat(new byte[8 - b.Length % 8]).ToArray();

            Encipher(b, b.Length);

            return Convert.ToBase64String(b);
        }

        private void Decipher(byte[] data, int length)
        {
            if (length % 8 != 0)
                throw new Exception("Invalid Length");

            for (var i = 0; i < length; i += 8)
            {
                var xl = (uint)((data[i] << 24) |
                                (data[i + 1] << 16) |
                                (data[i + 2] << 8) | data[i + 3]);
                var xr = (uint)((data[i + 4] << 24) |
                                (data[i + 5] << 16) |
                                (data[i + 6] << 8) | data[i + 7]);
                (xl, xr) = Decipher(xl, xr);

                data[i] = (byte)(xl >> 24);
                data[i + 1] = (byte)(xl >> 16);
                data[i + 2] = (byte)(xl >> 8);
                data[i + 3] = (byte)(xl);
                data[i + 4] = (byte)(xr >> 24);
                data[i + 5] = (byte)(xr >> 16);
                data[i + 6] = (byte)(xr >> 8);
                data[i + 7] = (byte)(xr);
            }
        }

        private (uint xL, uint xR) Decipher(uint xl, uint xr)
        {
            short i;

            var xL = xl;
            var xR = xr;

            for (i = N + 1; i > 1; --i)
            {
                xL ^= _p[i];
                xR = F(xL) ^ xR;

                /* Exchange Xl and Xr */
                (xL, xR) = Swap(xL, xR);
            }

            /* Exchange Xl and Xr */
            (xL, xR) = Swap(xL, xR);

            xR ^= _p[1];
            xL ^= _p[0];

            return (xL, xR);
        }

        public string Decipher(string data)
        {
            byte[] b = Convert.FromBase64String(data);

            if (b.Length % 8 != 0)
                b = b.Concat(new byte[8 - b.Length % 8]).ToArray();

            Decipher(b, b.Length);

            return Encoding.ASCII.GetString(b);
        }

        private (uint left, uint right) Swap(uint a, uint b)
        {
            var tmp = a;
            a = b;
            b = tmp;
            return (a, b);
        }
    }
}
