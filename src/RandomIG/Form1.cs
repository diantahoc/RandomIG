using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace RandomIG
{
    public partial class Form1 : Form
    {
        //DataTable table = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        //public double Evaluate(string expression)
        //{
        //    table.Columns.Add("expression", typeof(string), expression);
        //    DataRow row = table.NewRow();

        //    table.Rows.Add(row);

        //    double d = double.Parse(Convert.ToString(row["expression"]));

        //    table.Rows.Clear();
        //    table.Columns.Clear();

        //    return d;
        //}
        Color[] bw = { Color.White, Color.Black };

        Color[] ran = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet };

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;

            string expression = this.textBox1.Text;

            try
            {
                eval(expression, Convert.ToInt32(textBox2.Text), Convert.ToInt32(textBox3.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Cannot render image: {0}", ex.Message), "error");
                this.button1.Enabled = true;
                return;
            }

            if (listBox1.Items.IndexOf(expression) == -1)
            {
                listBox1.Items.Add(expression);
            }

        }

        private void eval(string exp, int h, int w)
        {
            Color[] arr = checkBox1.Checked ? bw : ran;

            bool use_prev = !(string.IsNullOrEmpty(textBox4.Text) | string.IsNullOrWhiteSpace(textBox4.Text));

            Bestcode.MathParser.MathParser seedCombiner = null;

            if (use_prev) 
            {
                seedCombiner = new Bestcode.MathParser.MathParser();
                seedCombiner.Expression = textBox4.Text;
                try
                {
                    seedCombiner.Parse();
                    seedCombiner.Optimize();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot parse PSeed expression: " + ex.Message);
                    this.button1.Enabled = true;
                    return;
                }
            }


            Task.Factory.StartNew(() =>
            {
                Bestcode.MathParser.MathParser mp = new Bestcode.MathParser.MathParser();
                mp.Expression = exp;

                try
                {
                    mp.Parse();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Expression error: " + ex.Message);
                    this.button1.Enabled = true;
                    return;
                }
                mp.Parse();
                mp.Optimize();

                Bitmap wb = new Bitmap(w, h);
                FastBitmap fb = new FastBitmap(wb);
                fb.LockImage();

                double t = Convert.ToDouble(h);

                int old_seed = 0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        //Random rnd = new Random(Convert.ToInt32(Evaluate(expression.Replace("x", x.ToString()).Replace("y", y.ToString()))));
                        mp.X = x;
                        mp.Y = y;

                        try
                        {
                            int new_seed = Convert.ToInt32(mp.ValueAsDouble);

                            Random rnd = null;

                            if (use_prev)
                            {
                                seedCombiner.X = old_seed;
                                seedCombiner.Y = new_seed;

                                rnd = new Random(Convert.ToInt32(seedCombiner.ValueAsDouble));

                                old_seed = new_seed;
                            }
                            else 
                            {
                                rnd = new Random(new_seed);
                            }

                            fb.SetPixel(x, y, arr[rnd.Next(arr.Length)]);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        Invoke((Action)delegate
                        {
                            double c = Convert.ToDouble(y);

                            this.progressBar1.Value = Convert.ToInt32((c / t) * 100.0);
                        });

                    }
                }

                fb.UnlockImage();

                Invoke((Action)delegate
                {
                    this.pictureBox1.Image = wb; this.button1.Enabled = true;
                });

            });

            
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null & button1.Enabled)
            {
                this.textBox1.Text = listBox1.SelectedItem.ToString();
                button1.PerformClick();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.Filter = "PNG Images|*.png";
                sf.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (sf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.pictureBox1.Image.Save(sf.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox2.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.Checked = !checkBox1.Checked;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (object item in listBox1.Items) 
            {
                if (!Properties.Settings.Default.func.Contains(item.ToString())) 
                {
                    Properties.Settings.Default.func.Add(item.ToString());
                }
            }
            Properties.Settings.Default.Save();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (SaveFileDialog sf = new SaveFileDialog()) 
            {
                sf.Filter = "TEXT Files|*.txt";
                if (sf.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
                {
                    List<string> a = new List<string>();
                    foreach (object item in listBox1.Items) 
                    {
                        a.Add(item.ToString());
                    }
                    System.IO.File.WriteAllLines(sf.FileName, a.ToArray());
                }
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OpenFileDialog sf = new OpenFileDialog())
            {
                sf.Filter = "TEXT Files|*.txt";
                if (sf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<string> a = new List<string>();
                    foreach (string item in System.IO.File.ReadAllLines(sf.FileName))
                    {
                        if (listBox1.Items.IndexOf(item) == -1) 
                        {
                            listBox1.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string item in Properties.Settings.Default.func)
            {
                if (listBox1.Items.IndexOf(item) == -1)
                {
                    listBox1.Items.Add(item);
                }
            }
        }

    }
}
