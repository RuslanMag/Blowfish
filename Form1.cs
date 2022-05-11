using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blowfish
{
    public partial class Form1 : Form
    {
        private BlowfishCore bf = new BlowfishCore();

        public Form1()
        {
            InitializeComponent();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dlg.FileName;
                StreamReader reader = new StreamReader(dlg.FileName, Encoding.Default);
                richTextBox1.Text = reader.ReadToEnd();
                reader.Close();
            }

            dlg.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);
                writer.Write(richTextBox1.Text);
                writer.Close();
            }

            dlg.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CheckKey();
            bf.KeyExtention(textBox1.Text);
            richTextBox1.Text = bf.Decipher(richTextBox2.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CheckKey();
            bf.KeyExtention(textBox1.Text);
            richTextBox2.Text = bf.Encipher(richTextBox1.Text);
        }

        private void CheckKey()
        {
            if (String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show(
                 "Введите ключ",
                 "Внимание!",
                 MessageBoxButtons.OK,
                 MessageBoxIcon.Warning,
                 MessageBoxDefaultButton.Button1,
                 MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dlg.FileName;
                StreamReader reader = new StreamReader(dlg.FileName, Encoding.Default);
                richTextBox2.Text = reader.ReadToEnd();
                reader.Close();
            }

            dlg.Dispose();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);
                writer.Write(richTextBox2.Text);
                writer.Close();
            }

            dlg.Dispose();
        }
    }
}
