﻿using FEBibliothek.Modell;
using System.Windows;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class NeuerQuerschnitt
    {
        private readonly FEModell modell;

        public NeuerQuerschnitt(FEModell modell)
        {
            this.modell = modell;
            InitializeComponent();
            QuerschnittId.Text = "";
            Dicke.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var querschnittId = QuerschnittId.Text;
            double dicke = 0;
            if (Dicke.Text.Length != 0) { dicke = double.Parse(Dicke.Text); }
            var querschnitt = new Querschnitt(dicke)
            {
                QuerschnittId = querschnittId
            };
            modell.Querschnitt.Add(querschnittId, querschnitt);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}