﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using GlassFilm.Class;
using System.IO;
using VectorView;

namespace GlassFilm
{
    public partial class FrmPrincipal : Form
    {
        SeletorVeiculo sel = null;

        public FrmPrincipal()
        {
            InitializeComponent();

            sel = new SeletorVeiculo();
            sel.ListaTodas = false;

            sel.CbMarcas = cbMarca;
            sel.CbModelos = cbModelo;
            sel.CbVeiculos = cbAno;

            vvCorte.Document = new VectorDocument();

            vvModelo.DoubleClick += VvModelo_DoubleClick;
        }

        private void VvModelo_DoubleClick(object sender, EventArgs e)
        {
            if (vvModelo.Document != null)
            {
                VectorShape vs = vvModelo.Document.MouseHitShape;

                if (vs != null && vvCorte.Document != null)
                {
                    vvCorte.Document.ImportShape(vs);
                    vvCorte.AutoFit(VectorViewFitStyle.Both);
                    vvCorte.Invalidate();
                }
            }
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {           
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is MdiClient)
                {
                    ctrl.BackColor = ColorTranslator.FromHtml("#f7f6f6");
                }
            }

            FrmLogin frm = new FrmLogin();            
            frm.ShowDialog();

            if (frm.autorizado)
            {
                pnlFiltroInfo.Visible = true;
                pnlprincipal.Visible = true;
                //pnlMapa.Visible = true;
                splitDesenho.Visible = true;
            }

            sel.AtualizaMarcas();
            cbMarca.Focus();
        }

        private void cortadoraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmConfigPlotter conf = new FrmConfigPlotter();

            if (conf.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void cadastrarDesenhoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<FrmCadastroDesenho>().Count() > 0)
            {
                Mensagens.Atencao("Rotina já está aberta!");
                
            }
            else
            {
                FrmCadastroDesenho frm = new FrmCadastroDesenho();
                frm.ShowInTaskbar = false;
                frm.ShowDialog();
            }                        
        }

        private void cadastrarMarcaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<FrmCadMarca>().Count() > 0)
            {
                Mensagens.Atencao("Rotina já está aberta!");
            }
            else
            {
                FrmCadMarca frm = new FrmCadMarca();
                frm.ShowInTaskbar = false;
                frm.ShowDialog();
            }     
        }

        private void cadastroModeloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<FrmCadModelo>().Count() > 0)
            {
                Mensagens.Atencao("Rotina já está aberta!");
            }
            else
            {
                FrmCadModelo frm = new FrmCadModelo();
                frm.ShowInTaskbar = false;
                frm.ShowDialog();
            }     
        }       

        private void cbAno_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbAno.SelectedItem != null)
            {
                Veiculo v = (Veiculo)cbAno.SelectedItem;

                vvModelo.Document = null;

                string svg = DBManager.CarregarDesenho(v.Id);
                if (svg != null)
                {
                    vvModelo.Document = new VectorView.VectorDocument();

                    vvModelo.Document.AllowTransforms = false;
                    vvModelo.Document.AllowMove = false;
                    vvModelo.Document.AllowZoom = false;

                    vvModelo.Document.LoadSVG(svg);

                    vvModelo.AutoFit(VectorView.VectorViewFitStyle.Both);
                }
            }
        }

        private void vvModelo_Resize(object sender, EventArgs e)
        {
            vvModelo.AutoFit(VectorView.VectorViewFitStyle.Both);
            Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (vvModelo.Document != null)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    string cmds = vvModelo.Document.ToHPGL();
                    RawPrinterHelper.SendStringToPrinter(pd.PrinterSettings.PrinterName, cmds);

                    File.WriteAllText("teste.plt", cmds);
                }
            }
        }
    }
}
