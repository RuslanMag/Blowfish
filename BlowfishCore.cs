using System;
using System.Linq;
using System.Text;

namespace Blowfish
{

    /// <summary>
    /// Класс, обеспечивающий шифрование Blowfish.
    /// </summary>
    internal class BlowfishCore
    {
        const int N = 16; //Константа количества итераций

        private uint[,] _s; //Матрица подстановки (S-Блоки)
        private uint[] _p; //Матрица раундовых ключей (P-Блок)

        /// <summary>
        /// Производит расширение предоставленного ключа и подготавливает блоки.
        /// </summary>
        /// <param name="key">Ключ шифрования.</param>
        private void KeyExtension(byte[] key)
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

        /// <summary>
        /// Производит расширение предоставленного ключа и подготавливает блоки.
        /// </summary>
        /// <param name="key">Ключ шифрования.</param>
        public void KeyExtension(string key)
        {
            byte[] data = Encoding.UTF8.GetBytes(key);
            KeyExtension(data);
        }

        /// <summary>
        /// Реализует функцию F.
        /// </summary>
        /// <param name="x">Входящие данные</param>
        /// <returns></returns>
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

        /// <summary>
        /// Шифрует битовый массив.
        /// </summary>
        /// <param name="data">Массив для шифрования.</param>
        /// <param name="length">The amount to encrypt.</param>
        private void Encipher(byte[] data, int length)
        {
            if (length % 8 != 0)
                throw new Exception("Invalid Length");

            for (int i = 0; i < length; i += 8)
            {
                // Encode the data in 8 byte blocks.
                var xl = (uint)((data[i] << 24) |
                                (data[i + 1] << 16) |
                                (data[i + 2] << 8) |
                                data[i + 3]);
                var xr = (uint)((data[i + 4] << 24) |
                                (data[i + 5] << 16) |
                                (data[i + 6] << 8) |
                                data[i + 7]);
                (xl, xr) = Encipher(xl, xr);
                // Now Replace the data.
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

        /// <summary>
        /// Шифрует 8 бит данных (1 блок).
        /// </summary>
        /// <param name="xl">Левая часть 8 бит.</param>
        /// <param name="xr">Правая часть 8 бит.</param>
        private (uint data_l, uint data_r) Encipher(uint xl, uint xr)
        {
            short i;

            var xL = xl;
            var xR = xr;

            for (i = 0; i < N; ++i)
            {
                xL ^= _p[i];
                xR = F(xL) ^ xR;

                /* Exchange Xl and Xr */
                (xL, xR) = Swap(xL, xR);
            }

            /* Exchange Xl and Xr */
            (xL, xR) = Swap(xL, xR);

            xR ^= _p[N];
            xL ^= _p[N + 1];

            xl = xL;
            xr = xR;

            return (xl, xr);
        }

        /// <summary>
        /// Шифрует строку.
        /// </summary>
        /// <param name="data">Строка для шифрования.</param>
        /// <returns>Зашифрованную строку</returns>
        public string Encipher(string data)
        {
            var b = Encoding.UTF8.GetBytes(data);

            if (b.Length % 8 != 0)
                b = b.Concat(new byte[8 - b.Length % 8]).ToArray();

            Encipher(b, b.Length);

            return Convert.ToBase64String(b);
        }

        /// <summary>
        /// Расшифровывает битовый массив.
        /// </summary>
        /// <param name="data">Массив для расшифрования.</param>
        /// <param name="length">The amount to decrypt.</param>
        private void Decipher(byte[] data, int length)
        {
            if (length % 8 != 0)
                throw new Exception("Invalid Length");

            // Encode the data in 8 byte blocks.
            for (var i = 0; i < length; i += 8)
            {
                var xl = (uint)((data[i] << 24) |
                                (data[i + 1] << 16) |
                                (data[i + 2] << 8) | data[i + 3]);
                var xr = (uint)((data[i + 4] << 24) |
                                (data[i + 5] << 16) |
                                (data[i + 6] << 8) | data[i + 7]);
                (xl, xr) = Decipher(xl, xr);
                // Now Replace the data.
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

        /// <summary>
        /// Расшифровывает 8 бит данных (1 блок).
        /// </summary>
        /// <param name="xl">Левая часть 8 бит.</param>
        /// <param name="xr">Правая часть 8 бит.</param>
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

        /// <summary>
        /// Расшифровывает строку.
        /// </summary>
        /// <param name="data">Строка для расшифрования.</param>
        /// <returns>Расшифровыванную строку</returns>
        public string Decipher(string data)
        {
            byte[] b = Convert.FromBase64String(data);

            if (b.Length % 8 != 0)
                b = b.Concat(new byte[8 - b.Length % 8]).ToArray();

            Decipher(b, b.Length);

            return Encoding.UTF8.GetString(b);
        }

        /// <summary>
        /// Меняет переменные местами
        /// </summary>
        /// <param name="a">Левая переменная.</param>
        /// <param name="b">Правая переменная.</param>
        /// <returns>Переменные, которые поменяли местами</returns>
        private (uint left, uint right) Swap(uint a, uint b)
        {
            var tmp = a;
            a = b;
            b = tmp;
            return (a, b);
        }
    }
}