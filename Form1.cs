using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Rc5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


        }
            private void btnEncrypt_Click(object sender, EventArgs e)
            {
                string plainText = txtInput.Text;
                string key = txtKey.Text;

                try
                {
                    RC5 rc5 = new RC5(key);
                    string encryptedText = rc5.Encrypt(plainText);
                    txtOutput.Text = encryptedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void btnDecrypt_Click(object sender, EventArgs e)
            {
                string encryptedText = txtInput.Text;
                string key = txtKey.Text;

                try
                {
                    RC5 rc5 = new RC5(key);
                    string decryptedText = rc5.Decrypt(encryptedText);
                    txtOutput.Text = decryptedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
    }

    public class RC5
        {
            private const int WordSize = 64;
            private const int Rounds = 12;
            private const int KeyLength = 16;
            private uint[] S;

            public RC5(string key)
            {
                InitializeKey(Encoding.UTF8.GetBytes(key));
            }

            private void InitializeKey(byte[] key)
            {
                int words = (KeyLength + 3) / 4;
                uint[] L = new uint[words];

                for (int i = 0; i < key.Length; i++)
                {
                    L[i / 4] = (L[i / 4] << 8) + key[i];
                }

                S = new uint[2 * (Rounds + 1)];
                S[0] = 0xB7E15163;
                for (int i = 1; i < S.Length; i++)
                {
                    S[i] = S[i - 1] + 0x9E3779B9;
                }

                uint A = 0, B = 0;
                int iter = 3 * Math.Max(words, S.Length);

                for (int k = 0, i = 0, j = 0; k < iter; k++)
                {
                    A = S[i] = RotateLeft(S[i] + A + B, 3);
                    B = L[j] = RotateLeft(L[j] + A + B, (int)(A + B));
                    i = (i + 1) % S.Length;
                    j = (j + 1) % words;
                }
            }

            public string Encrypt(string plainText)
            {
                byte[] data = Encoding.UTF8.GetBytes(plainText);
                int paddedLength = (data.Length + 7) / 8 * 8;
                byte[] paddedData = new byte[paddedLength];
                Array.Copy(data, paddedData, data.Length);

                StringBuilder result = new StringBuilder();

                for (int i = 0; i < paddedData.Length; i += 8)
                {
                    uint A = BitConverter.ToUInt32(paddedData, i);
                    uint B = BitConverter.ToUInt32(paddedData, i + 4);

                    A += S[0];
                    B += S[1];

                    for (int round = 1; round <= Rounds; round++)
                    {
                        A = RotateLeft(A ^ B, (int)B) + S[2 * round];
                        B = RotateLeft(B ^ A, (int)A) + S[2 * round + 1];
                    }

                    result.Append(BitConverter.ToString(BitConverter.GetBytes(A)));
                    result.Append(BitConverter.ToString(BitConverter.GetBytes(B)));
                }

                return result.ToString().Replace("-", "");
            }

            public string Decrypt(string cipherText)
            {
                byte[] data = new byte[cipherText.Length / 2];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Convert.ToByte(cipherText.Substring(i * 2, 2), 16);
                }

                StringBuilder result = new StringBuilder();

                for (int i = 0; i < data.Length; i += 8)
                {
                    uint A = BitConverter.ToUInt32(data, i);
                    uint B = BitConverter.ToUInt32(data, i + 4);

                for (int round = Rounds; round > 0; round--)
                    {
                        B = RotateRight(B - S[2 * round + 1], (int)A) ^ A;
                        A = RotateRight(A - S[2 * round], (int)B) ^ B;
                    }

                    B -= S[1];
                    A -= S[0];

                    result.Append(Encoding.UTF8.GetString(BitConverter.GetBytes(A)));
                    result.Append(Encoding.UTF8.GetString(BitConverter.GetBytes(B)));
                }

                return result.ToString().TrimEnd('\0');
            }

            private uint RotateLeft(uint value, int shift)
            {
                return (value << shift) | (value >> (WordSize - shift));
            }

            private uint RotateRight(uint value, int shift)
            {
                return (value >> shift) | (value << (WordSize - shift));
            }
        }
}


