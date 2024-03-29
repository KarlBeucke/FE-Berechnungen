﻿using FEBibliothek.Modell;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen
{
    public partial class QuerschnittKeys
    {
        public QuerschnittKeys(FeModell modell)
        {
            InitializeComponent();
            Left = 2 * Width;
            Top = Height;
            var querschnitt = modell.Querschnitt.Select(item => item.Value).ToList();
            QuerschnittKey.ItemsSource = querschnitt;
        }
    }
}
