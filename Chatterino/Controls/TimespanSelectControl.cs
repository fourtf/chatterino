using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public partial class TimespanSelectControl : UserControl
    {
        public TimespanSelectControl()
        {
            InitializeComponent();

            comboBox1.Text = "Minutes";

            numericUpDown1.ValueChanged += (s, e) => ValueChanged?.Invoke(this, EventArgs.Empty);
            comboBox1.TextChanged += (s, e) => ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public TimespanSelectControl(int value)
            : this()
        {
            SetValue(value);
        }

        public event EventHandler ValueChanged;

        static Dictionary<string, int> values = new Dictionary<string, int>
        {
            ["Seconds"] = 1,
            ["Minutes"] = 60,
            ["Hours"] = 60 * 60,
            ["Days"] = 60 * 60 * 24,
        };

        public void SetValue(int value)
        {
            try
            {
                numericUpDown1.Value = value;
                comboBox1.Text = "Seconds";

                foreach (var v in values)
                {
                    if (value > v.Value && value % v.Value == 0)
                    {
                        numericUpDown1.Value = value / v.Value;
                        comboBox1.Text = v.Key;
                    }
                }
            }
            catch { }
        }

        public int GetValue()
        {
            try
            {
                return (int)(values[comboBox1.Text] * numericUpDown1.Value);
            }
            catch { }
            return 5 * 60;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Parent.Controls.Remove(this);
            }
            catch { }
        }
    }
}
