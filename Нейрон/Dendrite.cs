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
using System.Data.SqlClient;
using System.Data.OleDb;

namespace Нейрон
{
    [Serializable]
    public class NeurCell
    {
        List<Dendrite> hands;
        Axon Axon1;
        double diameter;
        float resi;
        public NeurCell()
        {
            hands = new List<Dendrite>();diameter = 0;
        }
        public Axon Axons
        {
            get { return Axon1; }
            set { Axon1 = value; }
        }
        public double DiamOut
        {
            get { return diameter; }
            set { if (value >= 0) diameter = value; }
        }
        public List<Dendrite> Dendrites
        {
            get { return hands; }
        }
        public void Draw(Graphics g,int gwidth,int gheight)
        {
            if (diameter > 0)
            {
                double d;
                if (diameter * 1.5 > gwidth / 2)
            {
                if (diameter * 1.5 < gheight - 100)
                {
                    d=gwidth / 2;
                }
                else
                {
                    if (gheight-100 < gwidth / 2)
                    {
                       d=gheight -100;
                    }
                    else
                    {d=gwidth / 2 ;}
                }
            }
            else
            {
                if (diameter * 1.5 >= gheight - 100)
                {
                    d=gheight - 100;
                }
                else
                {
                    if (diameter * 1.5 < 90)
                    {d=90;}
                    else
                    {
                       d= diameter * 1.5;
                    }
                }
            }
                //круги
                g.DrawEllipse(new Pen(Color.Black, 1), (gwidth - (2 * (float)d)) / 3, gheight / 2 - (float)d / 2, (float)d, (float)d);
                g.DrawEllipse(new Pen(Color.Black, 1), (gwidth - (2 * (float)d)) * 2 / 3 + (float)d, gheight / 2 - (float)d / 2, (float)d, (float)d);
                //вертикальные дуги
                g.DrawEllipse(new Pen(Color.Black, 1), (gwidth - (2 * (float)d)) / 3 +(float)d/4, gheight / 2 - (float)d / 2, (float)d/2, (float)d);
                g.DrawEllipse(new Pen(Color.Black, 1), (gwidth - (2 * (float)d)) * 2 / 3  + (float)d*5/4, gheight / 2 - (float)d / 2, (float)d/2, (float)d);
                //вертикальные прямые
                g.DrawLine(new Pen(Color.Black, 1),(gwidth - (2 * (float)d)) / 3 + (float)d/2, gheight / 2 - (float)d / 2, (gwidth - (2 * (float)d)) / 3 + (float)d / 2, gheight / 2 + (float)d / 2);
                g.DrawLine(new Pen(Color.Black, 1), (gwidth - (2 * (float)d))*2 / 3 + (float)d*3 / 2, gheight / 2 - (float)d / 2, (gwidth - (2 * (float)d)) * 2 / 3 + (float)d * 3 / 2, gheight / 2 + (float)d / 2);
                //горизонтальные прямые
                g.DrawLine(new Pen(Color.Black, 1), (gwidth - (2 * (float)d)) / 3 , gheight / 2 , (gwidth - (2 * (float)d)) / 3 + (float)d , gheight / 2 );
                g.DrawLine(new Pen(Color.Black, 1), (gwidth - (2 * (float)d)) * 2 / 3 + (float)d , gheight / 2 , (gwidth - (2 * (float)d)) * 2 / 3 + 2*(float)d, gheight / 2 );

                Axon1.Draw(g,d, gwidth, gheight);
                if (hands.Count > 0) for (int i = 0; i < hands.Count; i++) hands[i].Draw(g,d, gwidth,gheight);
            }  
        }
        public void Animate(Graphics g,int gwidth)
        {
            foreach (Dendrite d in hands)
            {
                d.Animate(g,diameter);
            }
            if (resi > 0) { g.DrawEllipse(new Pen(Color.Blue), Axons.Xout - resi, Axons.Yout - resi, resi * 2,resi * 2);
                g.DrawEllipse(new Pen(Color.Blue),gwidth-( Axons.Xout )- resi, Axons.Yout - resi, resi * 2, resi * 2); }
            else if (resi < 0) { g.DrawEllipse(new Pen(Color.Red), Axons.Xout + resi, Axons.Yout + resi, -resi * 2, -resi * 2);
                g.DrawEllipse(new Pen(Color.Red), gwidth - (Axons.Xout )+ resi, Axons.Yout + resi, -resi * 2, -resi * 2); }
        }
        public double ResultImpulseSum()
        {
            double Ires = 0;
            foreach(Dendrite d in hands)
            { Ires += d.ResultImpulse(diameter); }

            resi = (float)Ires;

            return (Ires+(-70.0));
        }
    }

    [Serializable]
    public class Dendrite
    {
        int x, y,dx,dy,ivesh,irec;//dx dy смещение относительно центра кружочка при нажатии на него
        double l, r, a, b, cosf, sinf;
        double rx, ry;//rx ry смещение относительно центральной линии большого круга и экватора
        float rimp,rimp0;//величины изменения потенциала для отрисовки без пересчитывания
        Color c;
        bool hit,chosen,left,RecVesh;
        int receptorconc;
        public Dendrite(int x,int y)
        {
            this.x = x;this.y = y;l = -1;receptorconc = -1;r = -1;c = Color.Crimson;ivesh = 0;irec = 0;RecVesh = false;
        }
        public int Recconc
        {
            get { return receptorconc; }
            set { if (value >= -1) receptorconc = value; }
        }
        public double LOut
        {
            get { return l; }
            set { if (value >= -1) l = value; }
        }
        public Color cOut
        {
            get { return c; }
            set { c = value; }
        }
        public double RAxOut
        {
            get { return r; }
            set { if (value >= -1) r = value; }
        }
        public double Aout
        {
            get { return a; }
            set { a = value; }
        }
        public double Bout
        {
            get { return b; }
            set { b = value; }
        }
        public double Cosout
        {
            get { return cosf; }
            set { cosf = value; }
        }
        public double Sinout
        {
            get { return sinf; }
            set { sinf = value; }
        }
        public bool Leftorright
        {
            get { return left; }
            set { left = value; }
        }
        public bool IsRecVesh
        {
            get { return RecVesh; }
            set { RecVesh = value; }
        }
        public int WhichVesh
        {
            get { return ivesh; }
            set { ivesh = value; }
        }
        public int WhichRec
        {
            get { return irec; }
            set { irec = value; }
        }
        public int Xout
        {
            get { return x; }
            set { if (value >= 0) x = value; }
        }
        public int Yout
        {
            get { return y; }
            set { if (value >= 0) y = value; }
        }
        public int dXout
        {
            get { return dx; }
            set { dx = value; }
        }
        public int dYout
        {
            get { return dy; }
            set { dy = value; }
        }
        public double rXout
        {
            get { return rx; }
            set { rx = value; }
        }
        public double rYout
        {
            get { return ry; }
            set { ry = value; }
        }
        public bool IsHit
        {
            get { return hit; }
            set { hit = value; }
        }
        public bool IsChosen
        {
            get { return chosen; }
            set { chosen = value; }
        }
        public void Draw(Graphics g, double celld, int gwidth, int gheight)
        {
            if (IsChosen)
            {
                if (left)
                {
                    g.FillEllipse(new SolidBrush(Color.LawnGreen), (float)(a * cosf + (gwidth - 2 * celld) / 3 + celld / 2 - 5), (float)(b * sinf + gheight / 2 - 5), 10, 10);
                }
                else
                {
                    g.FillEllipse(new SolidBrush(Color.LawnGreen), (float)(a * cosf + (gwidth - 2 * celld) * 2 / 3 + celld * 3 / 2 - 5), (float)(b * sinf + gheight / 2 - 5), 10, 10);
                }
            }
            else
            {
                if (left)
                {
                    g.FillEllipse(new SolidBrush(c), (float)(a * cosf + (gwidth - 2 * celld) / 3 + celld / 2 - 5), (float)(b * sinf + gheight / 2 - 5), 10, 10);
                }
                else
                {
                    g.FillEllipse(new SolidBrush(c), (float)(a * cosf + (gwidth - 2 * celld) * 2 / 3 + celld * 3 / 2 - 5), (float)(b * sinf + gheight / 2 - 5), 10, 10);
                }
            }
        }
        public bool Ishit(int mx,int my)
        {
            if((mx-x)*(mx-x)+(my-y)*(my-y)<=25)
                return true;
            return false;
        }
        public void FindAB(double celld, int gwidth, int gheight)
        {
            b = celld / 2;
           a = Math.Sqrt(rx * rx / (1 - ((ry * ry) /( b * b))));
        }
        public void FindEllipse(double celld, int gwidth, int gheight)
        {
            left = (x < gwidth / 2) ? true : false;
            if (left)
            {
                rx =x- (gwidth - 2 * celld) / 3 - celld / 2;
            }
            else
            {
                rx = x-(gwidth - 2 * celld) * 2 / 3 - celld * 3 / 2;
            }
            ry = y - gheight / 2;

            FindAB(celld, gwidth, gheight);

            if (left)
            {
                if (a == 0) cosf = 0;
                else
                { cosf = (x - (gwidth - 2 * celld) / 3 - celld / 2) / a; }
                sinf = (y-gheight/2) / b;
            }
            else
            {
                if (a == 0) cosf = 0;
                else
                { cosf = (x - (gwidth - 2 * celld) * 2 / 3 - celld * 3 / 2) / a; }
                sinf =( y-gheight / 2) / b;
            }
            
        }
        public double ResultImpulse(double celld)//вычисление импульса
        {
            if (RecVesh & ivesh != 0 & irec != 0)
            {
                double Rrec;
                OleDbConnection connection = new OleDbConnection(Properties.Settings.Default.NeuronRecVeshTypesDatabaseConnectionString);
                connection.Open();
                OleDbCommand cmd = new OleDbCommand("SELECT ReceptorVeshestvo.receptor_resistance FROM ReceptorVeshestvo WHERE (ReceptorVeshestvo.rec_type = " + Convert.ToString(irec) + " AND ReceptorVeshestvo.vesh_type = " + Convert.ToString(ivesh) + " )", connection);
                bool b = true;
                try { Rrec = (double)cmd.ExecuteScalar(); }
                catch (NullReferenceException)
                {
                    b = false; Rrec = 0;
                }
                connection.Close();
                if (b)
                {
                    double Ux = 0;
                    //мкм = см/10000
                    double Isin = (-70.0 /*u membrane*/ - (-15.0) /*e sinapt*/  ) / (Math.Pow(10.0, 4) / (4 * Math.PI * (celld / 2) * (celld / 2) / 100000000.0) + ( Rrec/ (Recconc * Math.Pow(10,-9)*6.022*Math.Pow(10,23)))/*Rsin*/);
                    double Uo = Isin * Math.Pow(10.0, 4) / (4 * Math.PI * (celld / 2) * (celld / 2) / 100000000.0)/*=dU */;
                    double L = Math.Sqrt((celld / (4.0 * 10000.0)) * Math.Pow(10.0, 4)/*Rm одного см2*// 100.0/*Ri одного см3*/);
                    Ux =( Uo - (-70)) * Math.Exp((-r / 10000.0) / L);
                    rimp = (float)Ux; rimp0 = (float)Uo-(-70);//для визуализации
                    return Ux;
                }
                else { rimp = 0;rimp0 = 0; return 0; }
            }
            else { rimp = 0; rimp0 = 0; return 0; }
        }
        public void Animate(Graphics g,double celld)//визуализация
        {
            //бывший потенциал
            if (rimp0 < 0) g.DrawEllipse(new Pen(Color.Pink,1), x + rimp0, y + rimp0, -rimp0 * 2, -rimp0 * 2);
            else if(rimp > 0) g.DrawEllipse(new Pen(Color.LightSkyBlue,1), x - rimp0, y - rimp0, rimp0 * 2, rimp0 * 2);
            //итоговый потенциал
            if (rimp < 0) g.DrawEllipse(new Pen(Color.Red,1), x + rimp, y + rimp, -rimp * 2, -rimp * 2);
            else if (rimp > 0) g.DrawEllipse(new Pen(Color.Blue,1), x - rimp, y - rimp, rimp * 2, rimp * 2);
        }
    }

    [Serializable]
    public class Axon
    {
        int x, y;
        int apmin;bool wasapset,chosen;
        double l;
        Color c;
        public Axon(int x, int y)
        {
            this.x = x; this.y = y;wasapset = false; l = -1;c = Color.DodgerBlue;
        }
        public Color cOut
        {
            get { return c; }
            set { c = value; }
        }
        public int ApminOut
        {
            get { return apmin; }
            set { apmin = value; }
        }
        public double LOut
        {
            get { return l; }
            set { if (value >= -1) l = value; }
        }
        public int Xout
        {
            get { return x; }
            set { if (value >= 0) x = value; }
        }
        public int Yout
        {
            get { return y; }
            set { if (value >= 0) y = value; }
        }
        public bool Wasapset
        {
            get { return wasapset; }
            set { wasapset = value; }
        }
        public bool IsChosen
        {
            get { return chosen; }
            set { chosen = value; }
        }
        public bool Ishit(int mx,int my, int gwidth, int gheight)
        {
            if(((mx>=x-2) & (mx<=x+2))  |  (( mx<=gwidth-(x-2)) & (mx>=gwidth-(x+2))))
            {
                if ((my >= y) & (my <= y + 70)) { return true; }
            }
            return false;
        }
        public void Draw(Graphics g, double celld, int gwidth,int gheight)
        {
            //левый
            g.DrawLine(new Pen(Color.Black, 1), x - 3, y, x - 3, y + 50);
            g.DrawLine(new Pen(Color.Black, 1), x + 3, y, x + 3, y + 50);
            //правый
            g.DrawLine(new Pen(Color.Black, 1), gwidth - (x -3), y, gwidth - (x - 3), y + 50);
            g.DrawLine(new Pen(Color.Black, 1), gwidth - (x + 3), y, gwidth - (x + 3), y + 50);
        }
    }
}
