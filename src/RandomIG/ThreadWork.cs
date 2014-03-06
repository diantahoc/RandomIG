using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomIG
{
    public class ThreadWork
    {

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public int StartX { get; set; }
        public int EndX { get; set; }

        public System.Drawing.Color[] arr { get; set; }

        public bool UsePrev { get; set; }

        private Bestcode.MathParser.MathParser Evaluator { get; set; }
        private Bestcode.MathParser.MathParser SeedCombiner { get; set; }

        public System.Drawing.Bitmap Data { get; set; }


        public ThreadWork(string exp, string seedexp)
        {
            this.Evaluator = new Bestcode.MathParser.MathParser() { Expression = exp };
            this.Evaluator.Parse();
            this.Evaluator.Optimize();

            if (!string.IsNullOrEmpty(seedexp))
            {
                this.SeedCombiner = new Bestcode.MathParser.MathParser() { Expression = exp };
                this.SeedCombiner.Parse();
                this.SeedCombiner.Optimize();
            }
        }
        public void Start()
        {
            System.Threading.Thread t = new System.Threading.Thread(this._Start);
            t.Start();
        }

        private void _Start()
        {
            this.Data = new System.Drawing.Bitmap(this.EndX - this.StartX, this.ImageHeight);

            //FastBitmap f = new FastBitmap(this.Data);
            //f.LockImage();

            int old_seed = this.StartX == 0 ? 0 : eval(this.StartX - 1, 0);

            for (int y = 0; y < this.Data.Height; y++)
            {
                for (int x = 0; x < this.Data.Width; x++)
                {
                    try
                    {
                        int new_seed = eval(this.StartX + x, y);

                        Random rnd = null;

                        if (UsePrev)
                        {
                            SeedCombiner.X = old_seed;
                            SeedCombiner.Y = new_seed;

                            rnd = new Random(Convert.ToInt32(SeedCombiner.ValueAsDouble));

                            old_seed = new_seed;
                        }
                        else
                        {
                            rnd = new Random(new_seed);
                        }

                        this.Data.SetPixel(x, y, arr[rnd.Next(arr.Length)]);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                   
                }
            }


            //f.UnlockImage();
            FinishedEv(this);

        }

        private int eval(int x, int y)
        {
            Evaluator.X = x;
            Evaluator.Y = y;
            return Convert.ToInt32(Evaluator.ValueAsDouble);
        }

        public delegate void Finished(ThreadWork t);
        public event Finished FinishedEv;


    }
}
