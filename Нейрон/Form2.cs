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
using System.Xml;
using System.Data.OleDb;

namespace Нейрон
{
    public partial class Form2 : Form
    {
        Graphics g2;
        public NeurCell NC2;
        public event ApplyChangesEventHandler ChangesSaved;
        bool ismousedown,changesmade,wascopied;
        Dendrite CopyD;
        public Form2()
        {
            InitializeComponent();
            OleDbConnection connection = new OleDbConnection(Properties.Settings.Default.NeuronRecVeshTypesDatabaseConnectionString);
            connection.Open();
            //заполнение списка веществ
            OleDbCommand cmd = new OleDbCommand("SELECT Veshestva.vesh_name FROM Veshestva", connection);

                OleDbDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        comboBox1.Items.Add(dr["vesh_name"]);
                    }
                }
                dr.Close();

                //заполнение списка рецепторов
                cmd = new OleDbCommand("SELECT Receptors.rec_name FROM Receptors", connection);
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        comboBox2.Items.Add(dr["rec_name"]);
                    }
                }
            dr.Close();
            connection.Close();

            g2 = pictureBox1.CreateGraphics();
            ismousedown = false; changesmade = false; wascopied = false;
            DoubleBuffered = true;
            textBox1.Enabled = false; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = false;
            comboBox1.Enabled = false; comboBox2.Enabled = false;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (NC2.DiamOut > 0) { textBox5.Enabled = true; textBox5.Text = Convert.ToString(NC2.DiamOut); }
            changesmade = false;

            // TODO: данная строка кода позволяет загрузить данные в таблицу "neuronRecVeshTypesDatabaseDataSet.Veshestva". При необходимости она может быть перемещена или удалена.
            this.veshestvaTableAdapter.Fill(this.neuronRecVeshTypesDatabaseDataSet.Veshestva);
        }

        private void button1_Click(object sender, EventArgs e)//перенос данных в форму1
        {
            ChangesSaved(this, new ApplyChangesEventArgs(NC2));changesmade = false;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ismousedown = true;
            foreach(Dendrite d in NC2.Dendrites) { d.IsChosen = false;d.IsHit = false; }
            NC2.Axons.IsChosen = false;
            if (NC2.DiamOut > 0)
            {
                if (isNhit(e.X, e.Y, DrawingDiam(NC2.DiamOut,pictureBox1.Width,pictureBox1.Height)))
                {
                    
                    if (isDhit(e.X, e.Y))
                    {

                        if (e.Button == MouseButtons.Left)
                        {
                            radioButton1.Checked = false; radioButton2.Checked = true;
                            textBox4.Enabled = false;
                            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                            comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;

                            foreach (Dendrite d in NC2.Dendrites)//нахождение дендритов, на которые попали
                            {
                                if (d.Ishit(e.X, e.Y))
                                { d.IsHit = true;
                                  d.dXout = d.Xout - e.X; d.dYout = d.Yout -e.Y;
                                }
                            }
                            int nd = -1; bool b = false;

                            for (int i = 0; i < NC2.Dendrites.Count; i++)
                            { if (NC2.Dendrites[i].IsHit) { if (b == true) { nd = -1; break; } nd = i; b = true; } }

                            if (nd >= 0)//просматривать и изменять значения можно только одного выбранного дендрита
                            { NC2.Dendrites[nd].IsChosen = true;  textBox1.Text = Convert.ToString(NC2.Dendrites[nd].LOut); textBox1.Enabled = true;
                                textBox2.Text = Convert.ToString(NC2.Dendrites[nd].Recconc); textBox2.Enabled = true;
                                textBox3.Text = Convert.ToString(Math.Round(NC2.Dendrites[nd].RAxOut, 3)); textBox3.Enabled = true;
                                comboBox1.Enabled = true; comboBox1.SelectedIndex = NC2.Dendrites[nd].WhichVesh;
                                comboBox2.Enabled = true; comboBox2.SelectedIndex = NC2.Dendrites[nd].WhichRec; }
                        }
                        else
                        {
                            if (e.Button == MouseButtons.Right)//удаление дендритов по нажатию правой кнопкой
                            {
                                foreach (Dendrite d in NC2.Dendrites)
                                {
                                    if (d.Ishit(e.X, e.Y))
                                    {
                                        d.IsHit = true;
                                        d.dXout = d.Xout - e.X; d.dYout = d.Yout - e.Y;
                                    }
                                }

                                for (int i = 0; i < NC2.Dendrites.Count; i++)
                                { if (NC2.Dendrites[i].IsHit) { NC2.Dendrites.RemoveAt(i); i--; } }
                                changesmade = true;
                                textBox1.Enabled = false; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = false;
                                comboBox1.Enabled = false; comboBox2.Enabled = false;
                                textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                                comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
                            }
                        }
                    }
                    else
                    {
                        if (radioButton1.Checked != true)
                        {
                            if (radioButton2.Checked == true&e.Button==MouseButtons.Left)//добавление "пустого" дендрита по нажатию левой
                            {
                                textBox1.Enabled = true; textBox2.Enabled = true; textBox3.Enabled = true; textBox4.Enabled = false;
                                comboBox1.Enabled = true; comboBox2.Enabled = true;
                                textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = "";  textBox4.Text = "";
                                comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
                                NC2.Dendrites.Add(new Dendrite(e.X, e.Y));
                                NC2.Dendrites[NC2.Dendrites.Count - 1].FindEllipse(DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height),pictureBox1.Width,pictureBox1.Height);
                                NC2.Dendrites[NC2.Dendrites.Count - 1].RAxOut = (Math.PI / 2 + Math.Asin(-NC2.Dendrites[NC2.Dendrites.Count - 1].Sinout)) * NC2.DiamOut / 2;
                                NC2.Dendrites[NC2.Dendrites.Count - 1].IsChosen = true;
                                textBox1.Text = Convert.ToString(NC2.Dendrites[NC2.Dendrites.Count - 1].LOut); textBox1.Enabled = true;
                                textBox2.Text = Convert.ToString(NC2.Dendrites[NC2.Dendrites.Count - 1].Recconc); textBox2.Enabled = true;
                                textBox3.Text = Convert.ToString(Math.Round(NC2.Dendrites[NC2.Dendrites.Count-1].RAxOut, 3)); textBox3.Enabled = true;
                                comboBox1.Enabled = true; comboBox2.Enabled = true;
                                changesmade = true; Invalidate();
                            }
                            else
                            {
                                if (e.Button == MouseButtons.Right&wascopied)//вставка скопированного дендрита
                                {
                                        textBox1.Enabled = true; textBox2.Enabled = true; textBox3.Enabled = true; textBox4.Enabled = false;
                                        comboBox1.Enabled = true; comboBox2.Enabled = true;
                                        textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                                        comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
                                        NC2.Dendrites.Add(new Dendrite(e.X,e.Y));
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].FindEllipse(DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height), pictureBox1.Width, pictureBox1.Height);
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].RAxOut= (Math.PI / 2 + Math.Asin(-NC2.Dendrites[NC2.Dendrites.Count-1].Sinout)) * NC2.DiamOut / 2;
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].LOut = CopyD.LOut;
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].WhichRec = CopyD.WhichRec;
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].Recconc = CopyD.Recconc;
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].WhichVesh = CopyD.WhichVesh;
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].IsRecVesh = CopyD.IsRecVesh;
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].cOut = CopyD.cOut;

                                        foreach (Dendrite d in NC2.Dendrites)
                                        { d.IsHit = false; d.IsChosen = false; }
                                        NC2.Dendrites[NC2.Dendrites.Count - 1].IsChosen = true;
                                        textBox1.Text = Convert.ToString(NC2.Dendrites[NC2.Dendrites.Count - 1].LOut); textBox1.Enabled = true;
                                        textBox2.Text = Convert.ToString(NC2.Dendrites[NC2.Dendrites.Count - 1].Recconc); textBox2.Enabled = true;
                                        textBox3.Text = Convert.ToString(Math.Round(NC2.Dendrites[NC2.Dendrites.Count - 1].RAxOut, 3)); textBox3.Enabled = true;
                                        comboBox1.Enabled = true; comboBox1.SelectedIndex = NC2.Dendrites[NC2.Dendrites.Count - 1].WhichVesh; 
                                        comboBox2.Enabled = true; comboBox2.SelectedIndex = NC2.Dendrites[NC2.Dendrites.Count - 1].WhichRec;
                                        changesmade = true; Invalidate();
                                }
                            }
                        }
                    }
                }

                else
                {
                    if (isAhit(e.X, e.Y))//вывод значений параметров аксона
                    {
                        radioButton2.Checked = false; radioButton1.Checked = true;
                        textBox1.Enabled = true; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = true;
                        comboBox1.Enabled = false; comboBox2.Enabled = false;
                        NC2.Axons.IsChosen = true;
                        if (NC2.Axons.Wasapset) { textBox4.Text = Convert.ToString(NC2.Axons.ApminOut); }else { textBox4.Text = ""; }
                        textBox1.Text = Convert.ToString(NC2.Axons.LOut);
                    }


                    else//если не попали по клетке
                    {
                        textBox1.Enabled = false; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = false;
                        comboBox1.Enabled = false; comboBox2.Enabled = false;
                        textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                        comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
                        
                    }
                }
            }
            else//если диаметр клетки не задан
            {
                textBox1.Enabled = false; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = false;
                comboBox1.Enabled = false; comboBox2.Enabled = false;
                textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
                comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
                
            }
            
        }
        private bool isNhit(int x, int y,double d)//d - диаметр тела клетки
        {
            
            if ((x - ((pictureBox1.Width - (2 * (float)d)) / 3 + (float)d / 2)) * (x - ((pictureBox1.Width - (2 * (float)d)) / 3 + (float)d / 2)) + (y - pictureBox1.Height / 2) * (y - pictureBox1.Height / 2) <= (d / 2) * (d / 2)) return true;
            if ((x - ((pictureBox1.Width - (2 * (float)d)) * 2 / 3 + (float)d*3/2)) * (x - ((pictureBox1.Width - (2 * (float)d)) * 2 / 3 + (float)d*3/2)) + (y - pictureBox1.Height / 2 ) * (y - pictureBox1.Height / 2 ) <= (d / 2) * (d / 2)) return true;
            return false;
        }
        private bool isDhit(int x, int y)
        {
            bool b = false;
            foreach(Dendrite d in NC2.Dendrites)
            {
                if (d.Ishit(x, y))b = true;
            }
            return b;
        } 
        private bool isAhit(int x, int y)
        {
            return NC2.Axons.Ishit(x, y, pictureBox1.Width, pictureBox1.Height);
        }
        private double DrawingDiam(double diameter,int gwidth,int gheight)
        {
            double d;
            if (diameter*1.5 > gwidth / 2)
            {
                if (diameter * 1.5 < gheight - 100)
                {
                    d = gwidth / 2;
                }
                else
                {
                    if (gheight-100 < gwidth / 2)
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

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ismousedown)
            {
                double diam = DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height);
                foreach (Dendrite d in NC2.Dendrites)
                {
                    if (isNhit(e.X, e.Y, diam)&isNhit(e.X + d.dXout, e.Y+d.dYout,diam)) { 
                    if (d.IsHit) { d.Xout = e.X +d.dXout ; d.Yout = e.Y+d.dYout ;
                            d.FindEllipse(diam, pictureBox1.Width, pictureBox1.Height);
                            d.RAxOut =  (Math.PI / 2 + Math.Asin(-d.Sinout))*NC2.DiamOut/2;
                            textBox3.Text = Convert.ToString(Math.Round(d.RAxOut,3));
                            changesmade = true;
                        }
                    }
                }
                Refresh();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            double diam = DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height);
            foreach (Dendrite d in NC2.Dendrites)
            {
                d.IsHit = false;
                if (d.Leftorright)
                {
                    d.rXout = d.Xout - (pictureBox1.Width - 2 * diam) / 3 - diam / 2;
                }
                else
                {
                    d.rXout = d.Xout - (pictureBox1.Width - 2 * diam) * 2 / 3 - diam * 3 / 2;
                }

                d.rYout = d.Yout - pictureBox1.Height / 2;
            }
            NC2.Draw(g2, pictureBox1.Width, pictureBox1.Height);

            pictureBox1.Invalidate();
            ismousedown = false;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g2 = e.Graphics;
            double diam = DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height);

            NC2.Axons.Xout = (pictureBox1.Width - 2 * (int)diam) / 3 + (int)diam / 2;
            NC2.Axons.Yout = pictureBox1.Height / 2 + (int)diam / 2;

            foreach(Dendrite d in NC2.Dendrites)
            {
               
                d.rXout = d.Cosout * d.Aout;
                if (d.Leftorright)
                { d.Xout = (int)((pictureBox1.Width - 2 * diam) / 3 + diam / 2 + d.rXout); }
                else { d.Xout = (int)((pictureBox1.Width - 2 * diam) * 2 / 3 + diam * 3 / 2 + d.rXout); }

                d.rYout = (int)(d.Bout * d.Sinout);
                d.Yout = (int)(pictureBox1.Height / 2 + d.rYout);
            }
            
                NC2.Draw(g2, pictureBox1.Width, pictureBox1.Height);
            
        }

        private void textBox5_TextChanged(object sender, EventArgs e)//диаметр клетки
        {
            double d,oldd;
            if (textBox5.Text == "") { NC2.DiamOut = 0; g2.Clear(pictureBox1.BackColor); }
            if ((textBox5.Text != "") && (Double.TryParse(textBox5.Text, out d)) && (double.Parse(textBox5.Text) > 0))
            {
                if (d > 200) { MessageBox.Show("Задан слишком большой диаметр"); textBox5.Text = "200"; }

                oldd = DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height);
                NC2.DiamOut = double.Parse(textBox5.Text);
                d = DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height);
                foreach (Dendrite den in NC2.Dendrites)
                {
                    den.Aout = den.Aout * d / oldd; den.Bout = d / 2;
                    den.RAxOut = (Math.PI / 2 + Math.Asin(-den.Sinout)) * NC2.DiamOut / 2;
                    den.rXout = den.Aout * den.Cosout; den.rYout = den.Bout * den.Sinout;
                    if (den.IsChosen) textBox3.Text = Convert.ToString(den.RAxOut);
                }
                NC2.Draw(g2, pictureBox1.Width, pictureBox1.Height);
            }

            else {textBox5.Text = ""; }
            pictureBox1.Invalidate(); changesmade = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Dendrite d in NC2.Dendrites) { d.IsHit = false; d.IsChosen = false; }
            NC2.Axons.IsChosen = false;
            comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
            textBox1.Enabled = false; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = false;
            comboBox1.Enabled = false; comboBox2.Enabled = false;
            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            foreach(Dendrite d in NC2.Dendrites) { d.IsHit = false; d.IsChosen = false; }
            NC2.Axons.IsChosen = false;
             comboBox1.SelectedIndex = 0; comboBox2.SelectedIndex = 0;
            textBox1.Enabled = false; textBox2.Enabled = false; textBox3.Enabled = false; textBox4.Enabled = false;
            comboBox1.Enabled = false; comboBox2.Enabled = false;
            textBox1.Text = ""; textBox2.Text = ""; textBox3.Text = ""; textBox4.Text = "";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)//length
        {
            double l;

            if ((textBox1.Text != "") && (Double.TryParse(textBox1.Text, out l)) && (double.Parse(textBox1.Text) > 0))
            {
                if (NC2.Axons.IsChosen)
                {
                    if (l > 20000000) { MessageBox.Show("Задана слишком большая длина"); textBox1.Text = "20000000"; l = 20000000; }
                    NC2.Axons.LOut = l;
                }
                else
                {
                    if (l > 3000) { MessageBox.Show("Задана слишком большая длина"); textBox1.Text = "3000"; l = 3000; }
                    foreach (Dendrite d in NC2.Dendrites) { if (d.IsChosen) d.LOut = l; }

                }
            }
            else { if (textBox1.Text == "") { foreach (Dendrite d in NC2.Dendrites) { if (d.IsChosen) d.LOut = -1; } if (NC2.Axons.IsChosen) { NC2.Axons.LOut = -1; } }else textBox1.Text = "";  }
            pictureBox1.Invalidate(); changesmade = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)//Receptor conc
        {
            int n;

            if ((textBox2.Text != "") && (Int32.TryParse(textBox2.Text, out n)) && (double.Parse(textBox2.Text) > 0))
            { foreach (Dendrite d in NC2.Dendrites) { if (d.IsChosen) d.Recconc = n; } NC2.Draw(g2, pictureBox1.Width, pictureBox1.Height); }

            else { if (textBox2.Text == "") { foreach (Dendrite d in NC2.Dendrites) { if (d.IsChosen) d.Recconc = -1; } }else textBox2.Text = "";   }
            pictureBox1.Invalidate(); changesmade = true;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//r den - axon
        {
            double r;

            if ((textBox3.Text != "") && (Double.TryParse(textBox3.Text, out r)) && (double.Parse(textBox3.Text) > 0) && (double.Parse(textBox3.Text) < Math.Round(NC2.DiamOut / 2 * Math.PI, 3)))
            { foreach (Dendrite d in NC2.Dendrites) {
                    if (d.IsChosen)
                    {
                        d.RAxOut = r;
                        //перерасчет косинуса и синуса расположения дендрита
                        d.Sinout = -Math.Sin(d.RAxOut *2 / NC2.DiamOut - Math.PI / 2);
                        d.Cosout = Math.Sign(d.Cosout) * Math.Sqrt(1 - d.Sinout * d.Sinout);
                    }
                } 
            }

            else {  if ((textBox3.Text != "") && (Double.TryParse(textBox3.Text, out r)) && (double.Parse(textBox3.Text) >= Math.Round(NC2.DiamOut / 2 * Math.PI, 3)))
                {
                    if (double.Parse(textBox3.Text) > Math.Round(NC2.DiamOut / 2 * Math.PI, 3))
                    { MessageBox.Show("Расстояние больше максимального,\n установлено максимальное возможное"); textBox3.Text = Convert.ToString(Math.Round(NC2.DiamOut / 2 * Math.PI, 3)); }
                    foreach (Dendrite d in NC2.Dendrites)
                    {
                        if (d.IsChosen)
                        {
                            d.RAxOut = NC2.DiamOut / 2 * Math.PI ;
                            d.Sinout = -Math.Sin(d.RAxOut * 2 / NC2.DiamOut  - Math.PI / 2);
                            d.Cosout = Math.Sign(d.Cosout) * Math.Sqrt(1 - d.Sinout * d.Sinout);
                        }
                    }
                }
                else {
                    
                    foreach (Dendrite d in NC2.Dendrites)
                    {
                        if (d.IsChosen)
                        {
                            d.RAxOut = 1;
                            d.Sinout = -Math.Sin(d.RAxOut * 2 / NC2.DiamOut  - Math.PI / 2);
                            d.Cosout = Math.Sign(d.Cosout) * Math.Sqrt(1 - d.Sinout * d.Sinout);
                        }
                    }
                    textBox3.Text = "";
                }
            }
            pictureBox1.Invalidate();
            changesmade = true;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)//min action potent
        {
            int p;

            if ((textBox4.Text != "") && (Int32.TryParse(textBox4.Text, out p))) { if (NC2.Axons.IsChosen) { NC2.Axons.ApminOut = p; NC2.Axons.Wasapset = true; } pictureBox1.Invalidate(); NC2.Draw(g2, pictureBox1.Width, pictureBox1.Height); }
            else
            {
                if (textBox4.Text == "-") { if (NC2.Axons.IsChosen) { NC2.Axons.ApminOut = -1; NC2.Axons.Wasapset = true; } pictureBox1.Invalidate(); NC2.Draw(g2, pictureBox1.Width, pictureBox1.Height); }
                else

                { if ((textBox4.Text == "") & (textBox4.Enabled == true))
                    {
                        NC2.Axons.Wasapset = false; textBox4.Text = "";
                    }
                    else {
                        if (!Int32.TryParse(textBox4.Text, out p) & (textBox4.Enabled == true)) { NC2.Axons.Wasapset = false; textBox4.Text = "";  }
                         } }
            }
            changesmade = true;
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            double diam = DrawingDiam(NC2.DiamOut, pictureBox1.Width, pictureBox1.Height);
            foreach (Dendrite d in NC2.Dendrites) { d.Aout = d.Aout * (diam/2) / d.Bout;d.Bout = diam / 2; }
            pictureBox1.Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//veshestvo
        {
            if (NC2.Dendrites.Count > 0 & ismousedown==false)
            {
                int j;bool b=false;

            for (j = 0; j< NC2.Dendrites.Count; j++)
            {
                if (NC2.Dendrites[j].IsChosen) { b = true; NC2.Dendrites[j].WhichVesh = comboBox1.SelectedIndex; break; }
            }
                if (b)
                {
                    if (comboBox1.SelectedIndex == 0) { NC2.Dendrites[j].IsRecVesh = false; MessageBox.Show("задайте тип вещества"); }
                    else
                    {
                        if (comboBox2.SelectedIndex == 0) { NC2.Dendrites[j].IsRecVesh = false; MessageBox.Show("задайте тип рецепторов");  }
                        else
                        {
                            OleDbConnection connection = new OleDbConnection(Properties.Settings.Default.NeuronRecVeshTypesDatabaseConnectionString);
                            connection.Open();
                            OleDbCommand cmd = new OleDbCommand("SELECT ReceptorVeshestvo.receptor_resistance FROM ReceptorVeshestvo WHERE (ReceptorVeshestvo.rec_type = " + Convert.ToString(NC2.Dendrites[j].WhichRec) + " AND ReceptorVeshestvo.vesh_type = " + Convert.ToString(NC2.Dendrites[j].WhichVesh) + " )", connection);
                            
                            //if result of executescalar = null => dU=0, same do then checking isrecvesh and then starting calculations or animation

                            bool exists = true;
                            try { cmd.ExecuteScalar(); }
                            catch (NullReferenceException)
                            {
                               exists = false; 
                            }
                            connection.Close();

                            if (!exists) { NC2.Dendrites[j].IsRecVesh = false; MessageBox.Show("тип вещества не соответствует выбранному типу рецепторов"); }
                            else { NC2.Dendrites[j].IsRecVesh = true; }

                        }
                    }
                }
        }
            changesmade = true;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//rec type
        {
            if (NC2.Dendrites.Count > 0 & ismousedown == false)
            {
                int j; bool b = false;
                for (j = 0; j < NC2.Dendrites.Count; j++)
                {
                    if (NC2.Dendrites[j].IsChosen) { b = true; NC2.Dendrites[j].WhichRec = comboBox2.SelectedIndex; break; }

                }

                if (b)
                {
                    
                    if (comboBox2.SelectedIndex == 0) { NC2.Dendrites[j].IsRecVesh = false; MessageBox.Show("задайте тип рецепторов"); }
                    else
                    {
                        if (comboBox1.SelectedIndex == 0) { NC2.Dendrites[j].IsRecVesh = false; MessageBox.Show("задайте тип вещества"); }
                        else
                        {
                            OleDbConnection connection = new OleDbConnection(Properties.Settings.Default.NeuronRecVeshTypesDatabaseConnectionString);
                            connection.Open();
                            OleDbCommand cmd = new OleDbCommand("SELECT ReceptorVeshestvo.receptor_resistance FROM ReceptorVeshestvo WHERE (ReceptorVeshestvo.rec_type = " + Convert.ToString(NC2.Dendrites[j].WhichRec) + " AND ReceptorVeshestvo.vesh_type = " + Convert.ToString(NC2.Dendrites[j].WhichVesh) + " )", connection);
                           
                            bool exists = true;
                            try { cmd.ExecuteScalar(); }
                            catch (NullReferenceException)
                            {
                                exists = false;

                            }
                            connection.Close();

                            if (!exists) { NC2.Dendrites[j].IsRecVesh = false;
                            MessageBox.Show("тип рецепторов не соответствует выбранному типу вещества"); }
                            else { NC2.Dendrites[j].IsRecVesh = true; }

                        }
                    }
                }
            }
            changesmade = true;
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.C)
            {
                foreach(Dendrite d in NC2.Dendrites)
                {
                    if (d.IsChosen)
                    {
                        wascopied = true;
                        CopyD = new Dendrite(0,0);
                        CopyD.LOut =d.LOut;
                        CopyD.WhichRec=d.WhichRec;
                        CopyD.Recconc = d.Recconc;
                        CopyD.WhichVesh = d.WhichVesh;
                        CopyD.IsRecVesh = d.IsRecVesh;
                        CopyD.cOut = d.cOut;
                    }
                }
            }
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (changesmade)
            {
                if (MessageBox.Show("Выйти без сохранения изменений?", "Предупреждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                { e.Cancel = true; return; }
            }
        }
    }
    public class ApplyChangesEventArgs : EventArgs
    {
        public NeurCell NC;
        public ApplyChangesEventArgs(NeurCell NC2)
        {
            NC = new NeurCell();
            NC.DiamOut = NC2.DiamOut;
            NC.Axons = new Axon(NC2.Axons.Xout, NC2.Axons.Yout);
            NC.Axons.ApminOut = NC2.Axons.ApminOut; NC.Axons.Wasapset = NC2.Axons.Wasapset;
            NC.Axons.LOut = NC2.Axons.LOut; NC.Axons.cOut = NC2.Axons.cOut; NC.Axons.IsChosen = false;

            for (int i = 0; i < NC2.Dendrites.Count; i++)
            {
                NC.Dendrites.Add(new Dendrite(NC2.Dendrites[i].Xout, NC2.Dendrites[i].Yout));
                NC.Dendrites[i].cOut = NC2.Dendrites[i].cOut;
                NC.Dendrites[i].LOut = NC2.Dendrites[i].LOut;
                NC.Dendrites[i].RAxOut = NC2.Dendrites[i].RAxOut;
                NC.Dendrites[i].Recconc = NC2.Dendrites[i].Recconc;
                NC.Dendrites[i].dXout = 0; NC.Dendrites[i].dYout = 0;
                NC.Dendrites[i].IsChosen = false; NC.Dendrites[i].IsHit = false;
                NC.Dendrites[i].Leftorright = NC2.Dendrites[i].Leftorright;
                NC.Dendrites[i].rXout = NC2.Dendrites[i].rXout; NC.Dendrites[i].rYout = NC2.Dendrites[i].rYout;
                NC.Dendrites[i].Bout = NC2.Dendrites[i].Bout;
                NC.Dendrites[i].Aout = NC2.Dendrites[i].Aout;
                NC.Dendrites[i].WhichRec = NC2.Dendrites[i].WhichRec;
                NC.Dendrites[i].WhichVesh = NC2.Dendrites[i].WhichVesh;
                NC.Dendrites[i].IsRecVesh = NC2.Dendrites[i].IsRecVesh;
                NC.Dendrites[i].Cosout = NC2.Dendrites[i].Cosout; NC.Dendrites[i].Sinout = NC2.Dendrites[i].Sinout;
            }
        }
    }
    public delegate void ApplyChangesEventHandler(object sender, ApplyChangesEventArgs e);
}
