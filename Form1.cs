using System;
using System.IO;
using System.Text;
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
            OpenFile(richTextBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFile(richTextBox1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CheckKey() && CheckTextBox(richTextBox2))
            {
                bf.KeyExtension(textBox1.Text);
                richTextBox1.Text = bf.Decipher(richTextBox2.Text);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (CheckKey() && CheckTextBox(richTextBox1))
            {
                bf.KeyExtension(textBox1.Text);
                richTextBox2.Text = bf.Encipher(richTextBox1.Text);
            }            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFile(richTextBox2);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveFile(richTextBox2);
        }

        private bool CheckKey()
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
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool CheckTextBox(RichTextBox richTextBox)
        {
            if (String.IsNullOrEmpty(richTextBox.Text))
            {
                MessageBox.Show(
                "Пустое поле",
                "Внимание!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void OpenFile(RichTextBox textField)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dlg.FileName;
                StreamReader reader = new StreamReader(dlg.FileName, Encoding.Default);
                textField.Text = reader.ReadToEnd();
                reader.Close();
            }

            dlg.Dispose();
        }

        private void SaveFile(RichTextBox textField)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);
                writer.Write(textField.Text);
                writer.Close();
            }

            dlg.Dispose();
        }
    }
}
