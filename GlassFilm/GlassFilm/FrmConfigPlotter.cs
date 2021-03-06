﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlassFilm
{
    public partial class FrmConfigPlotter : Form
    {
        public FrmConfigPlotter()
        {
            InitializeComponent();

            SetCBValue(cbLang, Program.Config["PlotterLang"]);
            SetCBValue(cbInterface, Program.Config["PlotterInterface"]);

            if (!string.IsNullOrEmpty(Program.Config["PlotterName"]))
                lbPlotterName.Text = Program.Config["PlotterName"];

            cbRotate.Checked = true;
            cbFlip.Checked = false;

            try { cbRotate.Checked = bool.Parse(Program.Config["RotateCut"]); } catch { }
            try { cbFlip.Checked = bool.Parse(Program.Config["FlipX"]); } catch { }
            try { cbForceAutoNest.Checked = bool.Parse(Program.Config["forceAutoNest"]); } catch { }
            try { numMargin.Value = int.Parse(Program.Config["margin"]); } catch { }

        }

        public void SetCBValue(ComboBox cb, string value)
        {
            cb.SelectedIndex = -1;

            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (cb.Items[i].Equals(value))
                {
                    cb.SelectedIndex = i;
                }
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            Program.Config["PlotterLang"] = cbLang.Text;
            Program.Config["PlotterInterface"] = cbInterface.Text;
            Program.Config["RotateCut"] = cbRotate.Checked.ToString();
            Program.Config["FlipX"] = cbFlip.Checked.ToString();
            Program.Config["forceAutoNest"] = cbForceAutoNest.Checked.ToString();
            Program.Config["margin"] = numMargin.Value.ToString();

            Close();

            MessageBox.Show("Configurações gravadas com sucesso", "ATENÇÃO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbInterface.SelectedIndex == 0)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    Program.Config["PlotterName"] = pd.PrinterSettings.PrinterName;
                }
            }
            else if (cbInterface.SelectedIndex == 1)
            {
                FrmCadSerial serial = new FrmCadSerial();
                serial.ShowDialog();
            }
            else
            {

            }

            if (!string.IsNullOrEmpty(Program.Config["PlotterName"]))
                lbPlotterName.Text = Program.Config["PlotterName"];
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.techconsultoria.com.br/");
        }
    }
}
