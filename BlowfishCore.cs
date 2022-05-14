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
        const int N = 16; // Константа количества итераций.

        private uint[,] _s; // Матрица подстановки (S-блоки).
        private uint[] _p; // Матрица раундовых ключей (P-блок).

        /// <summary>
        /// Производит расширение предоставленного ключа и генерирует блоки.
        /// </summary>
        /// <param name="key">Ключ шифрования.</param>
        private void KeyExtension(byte[] key)
        {
            short i;
            short j = 0;
            _p = (uint[])Constants.P.Clone(); // Заполняет P-блок мантиссой числа Пи.
            _s = (uint[,])Constants.S.Clone(); // Заполняет S-блок мантиссой числа Пи.

            /* Делит ключ на блоки по 4 байта и складываем по модулю 2 с P-блоком. */
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

            /* Левая и правая части инициализирующего (64-битного) блока, каждая по 32 бита. */
            uint dataL = 0;
            uint dataR = 0;

            /* Генерирует матрицу раундовых ключей (P-блок). */
            for (i = 0; i < N + 2; i += 2)
            {
                (dataL, dataR) = Encipher(dataL, dataR); // шифрует инициализирующий блок с помощью Blowfish.
                /* Перезаписывает матрицу раундовых ключей (P-блок). */
                _p[i] = dataL;
                _p[i + 1] = dataR;
            }

            /* Генерирует матрицу подстановки (S-блоки). */
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 256; j += 2)
                {
                    (dataL, dataR) = Encipher(dataL, dataR); // шифрует инициализирующий блок с помощью Blowfish.
                    /* Перезаписывает матрицу подстановки (S-блоки). */
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
            byte[] data = Encoding.UTF8.GetBytes(key); // Кодирует строку в последовательность байтов.
            KeyExtension(data);
        }

        /// <summary>
        /// Реализует функцию F.
        /// </summary>
        /// <param name="x">Входящее 32-битовое значение.</param>
        /// <returns>32-битное выходное значение.</returns>
        private uint F(uint x)
        {
            /* Делит входные данные на блоки по 8 бит */
            var d = (ushort)(x & 0x00FF);
            x >>= 8;
            var c = (ushort)(x & 0x00FF);
            x >>= 8;
            var b = (ushort)(x & 0x00FF);
            x >>= 8;
            var a = (ushort)(x & 0x00FF);

            var y = _s[0, a] + _s[1, b]; // Складывает 1-ый блок со 2-ым по модулю 2 в 32.
            y ^= _s[2, c]; // Складывает результат предыдущего действия по модулю 2 с с 3-им блоком
            y += _s[3, d]; // Складывает результат предыдущего действия по модулю 2 в 32 с 4-ым блоком.

            return y; // Возвращает 32-битное выходное значение.
        }

        /// <summary>
        /// Шифрует битовый массив.
        /// </summary>
        /// <param name="data">Массив для шифрования.</param>
        /// <param name="length">Длина массива для шифрования.</param>
        private void Encipher(byte[] data, int length)
        {
            /* Проверяет кратность блока. */
            if (length % 8 != 0)
                throw new Exception("Invalid Length");

            for (int i = 0; i < length; i += 8)
            {
                /* Шифрует данные блоками по 32 бита. */
                var xl = (uint)((data[i] << 24) |
                                (data[i + 1] << 16) |
                                (data[i + 2] << 8) |
                                data[i + 3]);
                var xr = (uint)((data[i + 4] << 24) |
                                (data[i + 5] << 16) |
                                (data[i + 6] << 8) |
                                data[i + 7]);
                (xl, xr) = Encipher(xl, xr);
                /* Теперь перезаписывает данные. */
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
        /// Шифрует 64 бита данных (1 блок).
        /// </summary>
        /// <param name="xl">Левая часть 32 бита.</param>
        /// <param name="xr">Правая часть 32 бита.</param>
        /// <returns>Зашифрованные данные.</returns>
        private (uint data_l, uint data_r) Encipher(uint xl, uint xr)
        {
            short i;

            var xL = xl;
            var xR = xr;

            /* Производит 16 итераций в сети Фейстеля. */
            for (i = 0; i < N; ++i)
            {
                xL ^= _p[i];
                xR = F(xL) ^ xR; // Применяет F функцию.

                /* Меняет местами Xl и Xr. */
                (xL, xR) = Swap(xL, xR);
            }

            /* Меняет местами Xl и Xr. */
            (xL, xR) = Swap(xL, xR);


            /* Складывает 17 и 18 блок по модулю 2. */
            xR ^= _p[N];
            xL ^= _p[N + 1];

            xl = xL;
            xr = xR;

            return (xl, xr); // Возврашает зашифрованные данные.
        }

        /// <summary>
        /// Шифрует строку.
        /// </summary>
        /// <param name="data">Строка для шифрования.</param>
        /// <returns>Зашифрованную строку.</returns>
        public string Encipher(string data)
        {
            var b = Encoding.UTF8.GetBytes(data); // Кодирует строку в последовательность байтов.

            /* Дополняет последний блок до 64 бит. */
            if (b.Length % 8 != 0)
                b = b.Concat(new byte[8 - b.Length % 8]).ToArray();

            Encipher(b, b.Length); // Шифрует полученный массив данных.

            return Convert.ToBase64String(b); // Возвращает зашифрованную строку.
        }

        /// <summary>
        /// Расшифровывает битовый массив.
        /// </summary>
        /// <param name="data">Массив для расшифрования.</param>
        /// <param name="length">Длина массива для расшифрования.</param>
        private void Decipher(byte[] data, int length)
        {
            /* Проверяет кратность блока. */
            if (length % 8 != 0)
                throw new Exception("Invalid Length");

            /* Расшифровывает данные блоками по 32 бита. */
            for (var i = 0; i < length; i += 8)
            {
                var xl = (uint)((data[i] << 24) |
                                (data[i + 1] << 16) |
                                (data[i + 2] << 8) | data[i + 3]);
                var xr = (uint)((data[i + 4] << 24) |
                                (data[i + 5] << 16) |
                                (data[i + 6] << 8) | data[i + 7]);
                (xl, xr) = Decipher(xl, xr);
                /* Теперь перезаписывает данные. */
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
        /// Расшифровывает 64 бита данных (1 блок).
        /// </summary>
        /// <param name="xl">Левая часть 32 бита.</param>
        /// <param name="xr">Правая часть 32 бита.</param>
        /// <returns>Расшифрованные данные.</returns>
        private (uint xL, uint xR) Decipher(uint xl, uint xr)
        {
            short i;

            var xL = xl;
            var xR = xr;

            /* Производит 16 итераций в сети Фейстеля. */
            for (i = N + 1; i > 1; --i)
            {
                xL ^= _p[i];
                xR = F(xL) ^ xR; // Применяет F функцию.

                /* Меняет местами Xl и Xr. */
                (xL, xR) = Swap(xL, xR);
            }

            /* Меняет местами Xl и Xr. */
            (xL, xR) = Swap(xL, xR);

            /* Складывает 0 и 1 блок по модулю 2. */
            xR ^= _p[1];
            xL ^= _p[0];

            return (xL, xR); // Возврашает расшифрованные данные.
        }

        /// <summary>
        /// Расшифровывает строку.
        /// </summary>
        /// <param name="data">Строка для расшифрования.</param>
        /// <returns>Расшифровыванную строку.</returns>
        public string Decipher(string data)
        {            
            byte[] b = Convert.FromBase64String(data); // Преобразует строку (в кодировке Base64) в массив 8-разрядных чисел.

            /* Дополняет последний блок до 64 бит. */
            if (b.Length % 8 != 0)
                b = b.Concat(new byte[8 - b.Length % 8]).ToArray();

            Decipher(b, b.Length); // Расшифровывает полученный массив данных.

            return Encoding.UTF8.GetString(b); // Возвращает расшифровыванную строку.
        }

        /// <summary>
        /// Меняет переменные местами.
        /// </summary>
        /// <param name="a">Левая переменная.</param>
        /// <param name="b">Правая переменная.</param>
        /// <returns>Переменные, которые поменяли местами.</returns>
        private (uint left, uint right) Swap(uint a, uint b)
        {
            var tmp = a;
            a = b;
            b = tmp;
            return (a, b);
        }
    }
}