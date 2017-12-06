using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuronetProject
{
	// TODO: проверить работу, допилить графики, сделать график I от шагов, сделать таблицу ввода w
    //       динамический вывдов I и x(q) на каждой итерации
	public partial class Form1 : Form
	{
		Random r;
		// Параметры задачи
		private int n;				// количество нейронов
		private double T;			// отрезок времени
		private double B;			// ограничение на синаптические веса
		private double M1;			// штрафные коэффициенты
		private double M2;
		private double[] a;			// начальные значени я характеристик нейронов
		private double[] A;	
		private double[] Y;

		// параметры метода
		private int q;				// мелкость разбиения и количество слоёв

		private double epsilon;		// точность
		private double alpha;        // шаг градиентного спуска
		private double dt;           // отрезок разбиения
		private double[/*номер слоя*/][/*номер нейрона*/] p;                        // Множители Лагранжа

		private double[/*номер слоя*/][/*номер нейрона*/] x0;						// Характеристики нейрона на предыдущем шаге
		private double[/*номер слоя*/][/*номер нейрона*/] x1;						// Характеристики нейрона на текущем шаге
		private double[/*номер слоя*/][/*номер нейрона 1*/,/*номер нейрона 2*/] w0;	// синаптические веса нейрона на предыдущем шаге
		private double[/*номер слоя*/][/*номер нейрона 1*/,/*номер нейрона 2*/] w1;	// синаптические веса нейрона на текущем шаге
		private double I0;           // значение целевой функции на предыдущем шаге
		private double I1;           // значение целевой функции на текущем шаге

		public Form1()
		{
			InitializeComponent();
			InitializeParameters();
		}

		private void InitializeParameters()
		{
			r = new Random();

			dataGridView1.Rows.Add();
			dataGridView1["i", 0].Value = 1;
			dataGridView1["ai", 0].Value = r.Next(10, 60) / 10f;
			dataGridView1["Aai", 0].Value = r.Next(0, 100) / 10f;
			dataGridView1["Yi", 0].Value = r.Next(10, 30) / 100f;

			fieldM1.Text = r.Next(1000, 10000).ToString();
			fieldM2.Text = r.Next(1000, 10000).ToString();
            fieldT.Text = "1"; // (r.Next(10, 1000) / 10f).ToString();
			fieldB.Text = (r.Next(0, 100) / 10f).ToString();
			fieldEpsilon.Text = "1E-" + r.Next(2, 10);
			fieldAlpha.Text = "9";
			ParseParams();
		}

		private void fieldN_ValueChanged(object sender, EventArgs e)
		{
			int n = Decimal.ToInt32(fieldN.Value);

			while (dataGridView1.Rows.Count < n)
			{
				dataGridView1.Rows.Add();
				dataGridView1["i", dataGridView1.Rows.Count - 1].Value = dataGridView1.Rows.Count;
				dataGridView1["ai", dataGridView1.Rows.Count - 1].Value = r.Next(10, 60) / 10f;
				dataGridView1["Aai", dataGridView1.Rows.Count - 1].Value = r.Next(0, 100) / 10f;
				dataGridView1["Yi", dataGridView1.Rows.Count - 1].Value = r.Next(10, 30) / 100f;
			}
			while (dataGridView1.Rows.Count > n)
				dataGridView1.Rows.RemoveAt(n);
		}

		private void buttonEnterTaskParams_Click(object sender, EventArgs e)
		{
			ParseParams();
			InitW();
			CalculateX();
			CalculateI();
			x0 = x1;
			w0 = w1;
			I0 = I1;
			
			FillDataGridViewX();
		}

		private void ParseParams()
		{
			n = Decimal.ToInt32(fieldN.Value);
			q = Decimal.ToInt32(fieldQ.Value);

			if (!double.TryParse(fieldT.Text, out T))
			{
				MessageBox.Show("В поле T ошибка, вводите только числа");
				return;
			}
			if (!double.TryParse(fieldB.Text, out B))
			{
				MessageBox.Show("В поле B ошибка, вводите только числа");
				return;
			}
			if (!double.TryParse(fieldM1.Text, out M1))
			{
				MessageBox.Show("В поле M1 ошибка, вводите только числа");
				return;
			}
			if (!double.TryParse(fieldM2.Text, out M2))
			{
				MessageBox.Show("В поле M2 ошибка, вводите только числа");
				return;
			}
			if(!double.TryParse(fieldEpsilon.Text, out epsilon))
			{
				MessageBox.Show("В поле ɛ ошибка, вводите только числа");
				return;
			}
			if (!double.TryParse(fieldAlpha.Text, out alpha))
			{
				MessageBox.Show("В поле α ошибка, вводите только числа");
				return;
			}

			a = new double[n];
			A = new double[n];
			Y = new double[n];
			for (int i = 0; i < n; i++)
			{
				if (dataGridView1["ai", i].Value == null || !double.TryParse(dataGridView1["ai", i].Value.ToString(), out a[i]))
				{
					MessageBox.Show("В поле a" + (i + 1) + " ошибка, вводите только числа");
					return;
				}
				if (dataGridView1["Aai", i].Value == null || !double.TryParse(dataGridView1["Aai", i].Value.ToString(), out A[i]))
				{
					MessageBox.Show("В поле A" + (i + 1) + " ошибка, вводите только числа");
					return;
				}
				if (dataGridView1["Yi", i].Value == null || !double.TryParse(dataGridView1["Yi", i].Value.ToString(), out Y[i]))
				{
					MessageBox.Show("В поле Y" + (i + 1) + " ошибка, вводите только числа");
					return;
				}
			}
		}

		private void InitW()
		{
			w1 = new double[q][,];

			for (int k = 0; k < q; k++)
			{
				w1[k] = new double[n, n];
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						w1[k][i, j] = 0.5;
			}
		}

		private void CalculateX()
		{
			dt = T / q;
			x1 = new double[q + 1][];

			x1[0] = a;

			for (int k = 0; k < q; k++)
			{
				x1[k + 1] = new double[n];
				for (int i = 0; i < n; i++)
				{ 
					double Swx = 0;
					for (int j = 0; j < n; j++)
						Swx += w1[k][i, j] * x1[k][j];

					x1[k+1][i] = x1[k][i] + dt * (-Y[i] * x1[k][i] + Swx);
				}
			}
		}

		private void CalculateI()
		{
			double Sw = 0;
			for (int k = 0; k < q; k++)
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						Sw += w1[k][i, j] * w1[k][i, j];

			double Sx = 0;
			for (int i = 0; i < n; i++)
				Sx += (x1[q][i] - A[i]) * (x1[q][i] - A[i]);

			I1 = M1 * dt * Sw + M2 * Sx;
		}

		private void RunOptimization()
		{
            CalculateP();
            while (true)
			{
				CalculateW();
				CalculateX();
				CalculateI();

                if (Math.Abs(I0 - I1) < epsilon)
                    break;

                if (I1 > I0)
                    alpha = alpha / 2;
                else
                {
                    x0 = x1;
                    w0 = w1;
                    I0 = I1;
                    CalculateP();
                }
            }
		}

		private void CalculateP()
		{
			p = new double[q+1][];

			p[q] = new double[n];
			for (int i = 0; i < n; i++)
				p[q][i] = -2 * M2 * (x0[q][i] - A[i]);

			for(int k=q-1; k>0; k--)
			{
				p[k] = new double[n];
				for (int i = 0; i < n; i++)
				{
					double Spw = 0;
					for (int j = 0; j < n; j++)
						Spw += p[k + 1][j] * w0[k][j, i];

					p[k][i] = p[k + 1][i] - dt * Y[i] * p[k + 1][i] + dt * Spw;
				}
			}
		}

		private void CalculateW()
		{
			for (int k = 0; k < q; k++)
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
					{
						w1[k][i, j] = w0[k][i, j] - alpha * dt * (2 * M1 * w0[k][i, j] - p[k + 1][i] * x0[k][j]);
						if (w1[k][i, j] < -B)
							w1[k][i, j] = -B;
						else if (w1[k][i, j] > B)
							w1[k][i, j] = B;
					}
		}

		private void FillDataGridViewX()
		{
			while (dataGridViewX.Columns.Count < n + 1)
				dataGridViewX.Columns.Add("x" + (dataGridViewX.Columns.Count), "x" + (dataGridViewX.Columns.Count));
			while (dataGridViewX.Columns.Count > n + 1)
				dataGridViewX.Columns.RemoveAt(n + 1);

			dataGridViewX.Rows.Clear();
			for (int k = 0; k <= q; k++)
			{
				dataGridViewX.Rows.Add(k.ToString());
				for (int i = 0; i < n; i++)
					dataGridViewX["x" + (i + 1), k].Value = x1[k][i];
			}
            fieldI.Text = I1.ToString();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Graphics g = pictureBox2.CreateGraphics();
			double scalex = 700 / q;
			double scaley = 175 / MaxX();
			g.TranslateTransform(3, 175);

			Pen pen = new Pen(Color.Gray, 2);
			Point[] tmpoints = new Point[2];
			tmpoints[0] = new Point(-3, 0);
			tmpoints[1] = new Point(700, 0);
			g.DrawLines(pen, tmpoints);
			tmpoints[0] = new Point(0, 175);
			tmpoints[1] = new Point(0, -175);
			g.DrawLines(pen, tmpoints);

			Random r = new Random();

			for (int i=0; i<n; i++)
			{
				pen = new Pen(Color.FromArgb(r.Next(255), r.Next(255), r.Next(255), r.Next(255)), 2);
				PointF[] points = new PointF[q+1];
				for (int k = 0; k <= q; k++)
					points[k] = new PointF(float.Parse((k * scalex).ToString()), float.Parse((-x1[k][i] * scaley).ToString()));
				g.DrawLines(pen, points);
			}
		}

		private double MaxX()
		{
			double max = x0[0].Max();
			for (int k=1;k<q;k++)
				if (max < x0[k].Max())
					max = x0[k].Max();
			return max;
		}

        private void button2_Click(object sender, EventArgs e)
        {
            RunOptimization();
            FillDataGridViewX();
        }
    }
}
