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

        Color[] bw = { Color.White, Color.Black };

        Color[] ran = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet };

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;

            string expression = this.textBox1.Text;

            try
            {
                eval(expression, Convert.ToInt32(textBox2.Text), Convert.ToInt32(textBox3.Text));
                if (listBox1.Items.IndexOf(expression) == -1)
                {
                    listBox1.Items.Add(expression);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Cannot render image: {0}", ex.Message), "error");
                this.button1.Enabled = true;
                return;
            }

        }

        private bool verify(string name, string exp)
        {
            try
            {
                Bestcode.MathParser.MathParser s = new Bestcode.MathParser.MathParser();
                s.Expression = exp;
                s.Parse();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error in '{0}' expression: {1}", name, ex.Message));
                return false;
            }
        }


        private void eval(string exp, int h, int w)
        {

            bool use_prev = !(string.IsNullOrEmpty(textBox4.Text) || string.IsNullOrWhiteSpace(textBox4.Text));

            verify("Seed", textBox1.Text);

            if (use_prev)
            {
                verify("PSeed", textBox4.Text);
            }

            List<ThreadWork> thread_state = new List<ThreadWork>();

            int max_threads = Convert.ToInt32(textBox5.Text);
            int active_threads = 0;

            //int eachone_h = h / max_threads;
            int eachone_w = w / max_threads;

            int start_x = 0;
            //int start_y = 0;

            for (int i = 0; i < max_threads; i++)
            {

                ThreadWork tw = new ThreadWork(textBox1.Text, textBox4.Text)
                {
                    UsePrev = use_prev,
                    arr = checkBox1.Checked ? bw : ran,
                    StartX = start_x,
                    EndX = start_x + eachone_w,
                    ImageHeight = h,
                    ImageWidth = w
                };


                tw.FinishedEv += (aa) =>
                {
                    // data.Add(new int[] { aa.StartX, 0 }, aa.Data);

                    active_threads--;

                    this.Invoke((Action)delegate
                    {
                        using (Graphics g = Graphics.FromImage(this.pictureBox1.Image))
                        {
                            g.DrawImageUnscaled(aa.Data, new Point(aa.StartX, 0));
                            this.pictureBox1.Refresh();
                            Application.DoEvents();
                        }

                        int finished = max_threads - active_threads;
                        this.progressBar1.Value = Convert.ToInt32((double)finished / (double)max_threads * 100);

                        if (active_threads <= 0)
                        {
                            this.button1.Enabled = true;
                        }
                    });
                };

                thread_state.Add(tw);

                start_x += eachone_w;
            }

            Bitmap wb = new Bitmap(w, h);
            this.pictureBox1.Image = wb;

            active_threads = thread_state.Count;

            foreach (ThreadWork t in thread_state)
            {
                t.Start();
            }
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
            this.textBox5.Text = Environment.ProcessorCount.ToString();
        }

    }
}