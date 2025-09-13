using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Нейрон
{
    public partial class Form1 : Form
    {
        Form2 constructor; Graphics g;bool animtd;
        NeurCell NC;
        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics(); DoubleBuffered = true; NC = new NeurCell();
            animtd =false;
            NC.Axons = new Axon((pictureBox1.Width - 2 * (int)NC.DiamOut) / 3 + (int)NC.DiamOut / 2, pictureBox1.Height / 2 + (int)NC.DiamOut / 2);
            constructor = new Form2();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void button1_Click(object sender, EventArgs e)
        {
                label1.Visible = false; animtd = false;pictureBox1.Invalidate(); button3.Text = "Запустить визуализацию";
            if (constructor.Created == false)
            {
                constructor = new Form2();
                constructor.ChangesSaved += ChangesSent;
                constructor.NC2 = new NeurCell();
                constructor.NC2.DiamOut = NC.DiamOut;

                constructor.NC2.Axons = new Axon(NC.Axons.Xout, NC.Axons.Yout);
                constructor.NC2.Axons.ApminOut = NC.Axons.ApminOut; constructor.NC2.Axons.Wasapset = NC.Axons.Wasapset;
                constructor.NC2.Axons.LOut = NC.Axons.LOut; constructor.NC2.Axons.cOut = NC.Axons.cOut; constructor.NC2.Axons.IsChosen = false;

                for (int i = 0; i < NC.Dendrites.Count; i++)
                {
                    constructor.NC2.Dendrites.Add(new Dendrite(NC.Dendrites[i].Xout, NC.Dendrites[i].Yout));
                    constructor.NC2.Dendrites[i].cOut = NC.Dendrites[i].cOut;
                    constructor.NC2.Dendrites[i].LOut = NC.Dendrites[i].LOut;
                    constructor.NC2.Dendrites[i].RAxOut = NC.Dendrites[i].RAxOut;
                    constructor.NC2.Dendrites[i].Recconc = NC.Dendrites[i].Recconc;
                    constructor.NC2.Dendrites[i].dXout = 0; constructor.NC2.Dendrites[i].dYout = 0;
                    constructor.NC2.Dendrites[i].IsChosen = false; constructor.NC2.Dendrites[i].IsHit = false;
                    constructor.NC2.Dendrites[i].Leftorright = NC.Dendrites[i].Leftorright;
                    constructor.NC2.Dendrites[i].rXout = NC.Dendrites[i].rXout; constructor.NC2.Dendrites[i].rYout = NC.Dendrites[i].rYout;
                    constructor.NC2.Dendrites[i].Bout = NC.Dendrites[i].Bout;
                    constructor.NC2.Dendrites[i].Aout = NC.Dendrites[i].Aout;
                    constructor.NC2.Dendrites[i].WhichRec = NC.Dendrites[i].WhichRec;
                    constructor.NC2.Dendrites[i].WhichVesh = NC.Dendrites[i].WhichVesh;
                    constructor.NC2.Dendrites[i].IsRecVesh = NC.Dendrites[i].IsRecVesh;
                    constructor.NC2.Dendrites[i].Cosout = NC.Dendrites[i].Cosout; constructor.NC2.Dendrites[i].Sinout = NC.Dendrites[i].Sinout;
                }
                constructor.ShowDialog(); 
            }
            else {  constructor.Activate();  }
        }

        private void button3_Click(object sender, EventArgs e)//анимация показать если не включена/убрать если включена
        {
            if (animtd == false)
            {
                if (NC.DiamOut <= 0 || NC.Axons.Wasapset == false) { MessageBox.Show("Нет критически необходимой информации:\nдиаметра клетки или минимального потенциала действия");  }//если не задана клетка - предупреждение
                else
                {
                    bool b = false;
                    for (int i = 0; i < NC.Dendrites.Count; i++) { if (NC.Dendrites[i].LOut <= 0 | NC.Dendrites[i].Recconc <= 0 | NC.Dendrites[i].RAxOut <= 0 | NC.Dendrites[i].WhichRec == 0 | NC.Dendrites[i].WhichVesh == 0 | NC.Dendrites[i].IsRecVesh == false) { b = true; NC.Dendrites[i].cOut = Color.Gold; pictureBox1.Invalidate(); }
                        else { NC.Dendrites[i].cOut = Color.Crimson; pictureBox1.Invalidate(); } }
                    if (b)
                    {
                        pictureBox1.Invalidate(); MessageBox.Show("Желтые дендриты не определены.\nДоопределите или удалите\nих, чтобы продолжить");//если недозаданы дендриты - предупреждение
                    }
                    else {button3.Text = "Отключить визуализацию";animtd = true; NC.ResultImpulseSum(); NC.Animate(g,pictureBox1.Width);  }
                }
            }
            else { animtd = false; button3.Text = "Запустить визуализацию";pictureBox1.Invalidate(); }
        }   

        private void button2_Click(object sender, EventArgs e)//вычисления
        {
            if (NC.DiamOut <= 0 || NC.Axons.Wasapset==false) { MessageBox.Show("Нет критически необходимой информации:\nдиаметра клетки или минимального потенциала действия"); }//если не задана клетка - предупреждение
            else {
                    bool b = false;
                for (int i = 0; i < NC.Dendrites.Count; i++) { if (NC.Dendrites[i].LOut <= 0 | NC.Dendrites[i].Recconc <= 0 | NC.Dendrites[i].RAxOut <= 0 | NC.Dendrites[i].WhichRec == 0 | NC.Dendrites[i].WhichVesh == 0 | NC.Dendrites[i].IsRecVesh == false) { b = true; NC.Dendrites[i].cOut = Color.Gold; pictureBox1.Invalidate(); }
                    else { NC.Dendrites[i].cOut = Color.Crimson; pictureBox1.Invalidate(); } }
                    if (b) {
                    pictureBox1.Invalidate(); MessageBox.Show("Желтые дендриты не определены.\nДоопределите или удалите\nих, чтобы продолжить");//если недозаданы дендриты - предупреждение
                }
                else {
                    string s = (NC.ResultImpulseSum() >= NC.Axons.ApminOut) ? "Потенциал действия\nвозникнет" : "Потенциал действия\nНЕ возникнет";

                    label1.Text = "Суммарный импульс =\n " + Convert.ToString(Math.Round(NC.ResultImpulseSum(),4))+" мВ\n"+s; label1.Visible = true;
                }
            }
        }

        public void ChangesSent(object sender, ApplyChangesEventArgs e)
        {
            NC = new NeurCell();
            NC.DiamOut = e.NC.DiamOut;

            NC.Axons = new Axon((pictureBox1.Width - 2 * (int)NC.DiamOut) / 3 + (int)NC.DiamOut / 2, pictureBox1.Height / 2 + (int)NC.DiamOut / 2);
            NC.Axons.ApminOut = e.NC.Axons.ApminOut; NC.Axons.Wasapset = e.NC.Axons.Wasapset;
            NC.Axons.LOut = e.NC.Axons.LOut; NC.Axons.cOut = e.NC.Axons.cOut;NC.Axons.IsChosen = false;

            for (int i = 0; i < e.NC.Dendrites.Count; i++)
            {
                NC.Dendrites.Add(new Dendrite((int)((pictureBox1.Width - 2 * NC.DiamOut) / 3 + NC.DiamOut / 2 + e.NC.Dendrites[i].rXout), (int)(pictureBox1.Height / 2 + e.NC.Dendrites[i].rYout)));
                NC.Dendrites[i].cOut = e.NC.Dendrites[i].cOut;
                NC.Dendrites[i].LOut = e.NC.Dendrites[i].LOut;
                NC.Dendrites[i].RAxOut = e.NC.Dendrites[i].RAxOut;
                NC.Dendrites[i].Recconc = e.NC.Dendrites[i].Recconc;
                NC.Dendrites[i].dXout = 0; NC.Dendrites[i].dYout = 0;
                NC.Dendrites[i].IsChosen = false; NC.Dendrites[i].IsHit = false;
                NC.Dendrites[i].Leftorright = e.NC.Dendrites[i].Leftorright;
                NC.Dendrites[i].Cosout = e.NC.Dendrites[i].Cosout; NC.Dendrites[i].Sinout = e.NC.Dendrites[i].Sinout;
                NC.Dendrites[i].Aout = e.NC.Dendrites[i].Aout*DrawingDiam(NC.DiamOut,pictureBox1.Width,pictureBox1.Height)/( e.NC.Dendrites[i].Bout * 2);
                NC.Dendrites[i].Bout = DrawingDiam(NC.DiamOut, this.pictureBox1.Width, this.pictureBox1.Height) / 2;
                NC.Dendrites[i].rXout = NC.Dendrites[i].Aout * NC.Dendrites[i].Cosout;
                NC.Dendrites[i].rYout = NC.Dendrites[i].Bout * NC.Dendrites[i].Sinout;

                if (NC.Dendrites[i].Leftorright)
                { NC.Dendrites[i].Xout = (int)((this.pictureBox1.Width - 2 * NC.Dendrites[i].Bout * 2) / 3 + NC.Dendrites[i].Bout * 2 / 2 + NC.Dendrites[i].rXout); }
                else
                { NC.Dendrites[i].Xout = (int)((pictureBox1.Width - 2 * NC.Dendrites[i].Bout * 2) * 2 / 3 + NC.Dendrites[i].Bout * 2 * 3 / 2 + NC.Dendrites[i].rXout); }

                NC.Dendrites[i].Yout =(int)(NC.Dendrites[i].rYout+pictureBox1.Height/2);
                NC.Dendrites[i].WhichRec = e.NC.Dendrites[i].WhichRec;
                NC.Dendrites[i].WhichVesh = e.NC.Dendrites[i].WhichVesh;
                NC.Dendrites[i].IsRecVesh = e.NC.Dendrites[i].IsRecVesh;
            }
            Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            double diam = DrawingDiam(NC.DiamOut, pictureBox1.Width, pictureBox1.Height);

            NC.Axons.Xout= (pictureBox1.Width - 2 * (int)diam) / 3 + (int)diam / 2;
            NC.Axons.Yout = pictureBox1.Height / 2 + (int)diam / 2;

            foreach (Dendrite d in NC.Dendrites)
            {
                d.rXout = d.Cosout * d.Aout;
                if (d.Leftorright)
                { d.Xout = (int)((pictureBox1.Width - 2 * diam) / 3 + diam / 2 + d.rXout); }
                else { d.Xout = (int)((pictureBox1.Width - 2 * diam) * 2 / 3 + diam * 3 / 2 + d.rXout); }
                   
                d.rYout = (int)(d.Bout * d.Sinout);
                d.Yout = (int)(pictureBox1.Height / 2 + d.rYout);
            }

            NC.Draw(g, pictureBox1.Width, pictureBox1.Height);

            if (animtd) NC.Animate(g,pictureBox1.Width);
        }
        private double DrawingDiam(double diameter, int gwidth, int gheight)
        {
            double d;
            if (diameter * 1.5 > gwidth / 2)
            {
                if (diameter * 1.5 < gheight - 100)
                {
                    d = gwidth / 2;
                }
                else
                {
                    if (gheight - 100 < gwidth / 2)
                    {
                        d = gheight - 100;
                    }
                    else
                    {
                        d = gwidth / 2;
                    }
                }

            }
            else
            {
                if (diameter * 1.5 >= gheight - 100)
                {
                    d = gheight - 100;
                }
                else
                {
                    if (diameter * 1.5 < 90)
                    {
                        d = 90;
                    }
                    else
                    {
                        d = diameter * 1.5;
                    }
                }
            }
            return d;
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            double diam = DrawingDiam(NC.DiamOut, pictureBox1.Width, pictureBox1.Height);
            foreach (Dendrite d in NC.Dendrites) { d.Aout = d.Aout * (diam / 2) / d.Bout; d.Bout = diam / 2; }
            pictureBox1.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        void SaveState()
        {
            saveFileDialog1 = new SaveFileDialog(); saveFileDialog1.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*"; saveFileDialog1.Title = "Сохранить модель нейрона";saveFileDialog1.ShowDialog();
            BinaryFormatter bf = new BinaryFormatter();

            if (saveFileDialog1.FileName != "")
            {
                FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                bf.Serialize(fs, NC);
                fs.Close();
            }
        }
        void LoadState()
        {
            openFileDialog1 = new OpenFileDialog();openFileDialog1.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";  openFileDialog1.Title = "Открыть модель нейрона";
            BinaryFormatter bf = new BinaryFormatter(); FileStream fs;
            if (openFileDialog1.ShowDialog() == DialogResult.OK&&((fs= (System.IO.FileStream)openFileDialog1.OpenFile())!=null))
            {
                NC = (NeurCell)(bf.Deserialize(fs));
                fs.Close();
            }
        }

        private void очиститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удалить созданную модель?", "Предупреждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                animtd = false;
                label1.Visible = false;
                NC = new NeurCell();
                pictureBox1.Invalidate();
                NC.Axons = new Axon((pictureBox1.Width - 2 * (int)NC.DiamOut) / 3 + (int)NC.DiamOut / 2, pictureBox1.Height / 2 + (int)NC.DiamOut / 2);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
             LoadState(); Refresh();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveState();
        }
    }
}
