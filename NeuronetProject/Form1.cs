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
		// Параметры метода
		private int n;
		private float T;
		private float B;
		private float M1;
		private float M2;
		private float[] a;
		private float[] A;
		private float[] Y;

		public Form1()
		{
			InitializeComponent();
			dataGridView1.Rows.Add();
			dataGridView1["i", 0].Value = 1;
		}

		private void fieldN_ValueChanged(object sender, EventArgs e)
		{
			int n = Decimal.ToInt32(fieldN.Value);
			if (n <= 0)
			{
				panel1_1.Enabled = false;
				return;
			}

			panel1_1.Enabled = true;
			while (dataGridView1.Rows.Count < n)
			{
				dataGridView1.Rows.Add();
				dataGridView1["i", dataGridView1.Rows.Count - 1].Value = dataGridView1.Rows.Count;
				foreach (DataGridViewCell cell in dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells)
					cell.ValueType = typeof(int);
			}
			while (dataGridView1.Rows.Count > n)
				dataGridView1.Rows.RemoveAt(n);
		}

		private void buttonEnterTaskParams_Click(object sender, EventArgs e)
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


	}
}
