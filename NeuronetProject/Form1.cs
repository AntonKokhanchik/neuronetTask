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
	public partial class Form1 : Form
	{
		// Параметры задачи
		private int n;							// количество нейронов
		private float T;						// отрезок времени
		private float B;						// ограничение на синаптические веса
		private float M1;	
		private float M2;
		private float[] a;						// начальные значени я характеристик нейронов
		private float[] A;	
		private float[] Y;

		// параметры метода
		private int q = 10; // ?				// мелкость разбиения (и количество слоёв)
		private float dt;
		
		private float[/*шаг*/][/*номер нейрона*/] x;					// Характеристики нейрона
		private float[/*шаг*/][/*номер нейрона*/,/*номер слоя*/] w;		// синаптические веса нейрона

		public Form1()
		{
			InitializeComponent();

			dataGridView1.Rows.Add();
			dataGridView1["i", 0].Value = 1;
		}

		private void fieldN_ValueChanged(object sender, EventArgs e)
		{
			int n = Decimal.ToInt32(fieldN.Value);

			while (dataGridView1.Rows.Count < n)
			{
				dataGridView1.Rows.Add();
				dataGridView1["i", dataGridView1.Rows.Count - 1].Value = dataGridView1.Rows.Count;
			}
			while (dataGridView1.Rows.Count > n)
				dataGridView1.Rows.RemoveAt(n);
		}

		private void buttonEnterTaskParams_Click(object sender, EventArgs e)
		{
			ParseParams();
			InitW();
			InitX();
			FillDataGridViewX();
		}

		private void ParseParams()
		{
			n = Decimal.ToInt32(fieldN.Value);

			if (!float.TryParse(fieldT.Text, out T))
			{
				MessageBox.Show("В поле T ошибка, вводите только числа");
				return;
			}
			if (!float.TryParse(fieldB.Text, out B))
			{
				MessageBox.Show("В поле B ошибка, вводите только числа");
				return;
			}
			if (!float.TryParse(fieldM1.Text, out M1))
			{
				MessageBox.Show("В поле M1 ошибка, вводите только числа");
				return;
			}
			if (!float.TryParse(fieldM2.Text, out M2))
			{
				MessageBox.Show("В поле M2 ошибка, вводите только числа");
				return;
			}

			a = new float[n];
			A = new float[n];
			Y = new float[n];
			for (int i = 0; i < n; i++)
			{
				if (dataGridView1["ai", i].Value == null || !float.TryParse(dataGridView1["ai", i].Value.ToString(), out a[i]))
				{
					MessageBox.Show("В поле a" + (i + 1) + " ошибка, вводите только числа");
					return;
				}
				if (dataGridView1["Aai", i].Value == null || !float.TryParse(dataGridView1["Aai", i].Value.ToString(), out A[i]))
				{
					MessageBox.Show("В поле A" + (i + 1) + " ошибка, вводите только числа");
					return;
				}
				if (dataGridView1["Yi", i].Value == null || !float.TryParse(dataGridView1["Yi", i].Value.ToString(), out Y[i]))
				{
					MessageBox.Show("В поле Y" + (i + 1) + " ошибка, вводите только числа");
					return;
				}
			}
		}

		private void InitW()
		{
			w = new float[q][,];

			for(int k=0;k<q;k++)
				w[k] = new float[n, n];
		}

		private void InitX()
		{
			dt = T / q;
			x = new float[q + 1][];

			x[0] = a;

			for (int k = 0; k < q; k++)
			{
				x[k + 1] = new float[n];
				for (int i = 0; i < n; i++)
				{ 
					float Swx = 0;
					for (int j = 0; j < n; j++)
						Swx += w[k][i, j] * x[k][j];

					x[k+1][i] = x[k][i] + dt * (-Y[i] * x[k][i] + Swx);
				}
			}
		}

		private void FillDataGridViewX()
		{
			while (dataGridViewX.Columns.Count < n + 1)
				dataGridViewX.Columns.Add("x" + (dataGridViewX.Columns.Count), "x" + (dataGridViewX.Columns.Count));
			while (dataGridViewX.Columns.Count > n + 1)
				dataGridViewX.Columns.RemoveAt(n + 1);

			for (int k = 0; k <= q; k++)
			{
				dataGridViewX.Rows.Add(k.ToString());
				for (int i = 0; i < n; i++)
					dataGridViewX["x" + (i + 1), k].Value = x[k][i];
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Graphics g = pictureBox2.CreateGraphics();
			g.ScaleTransform(20, 20);
			g.TranslateTransform(1, 10);

			Pen pen = new Pen(Color.Gray,0.1f);
			Point[] tmpoints = new Point[2];
			tmpoints[0] = new Point(-1, 0);
			tmpoints[1] = new Point(100, 0);
			g.DrawLines(pen, tmpoints);
			tmpoints[0] = new Point(0, 100);
			tmpoints[1] = new Point(0, -100);
			g.DrawLines(pen, tmpoints);

			Random r = new Random();

			for (int i=0; i<n; i++)
			{
				pen = new Pen(Color.FromArgb(r.Next(255), r.Next(255), r.Next(255), r.Next(255)), 0.1f);
				PointF[] points = new PointF[q];
				for (int k=0;k<q;k++)
				{
					points[k] = new PointF(k, -x[k][i]);
				}
				g.DrawLines(pen, points);
			}
		}
	}
}
