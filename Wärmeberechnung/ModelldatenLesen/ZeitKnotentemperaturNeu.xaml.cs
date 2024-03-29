﻿using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitKnotentemperaturNeu
{
    private readonly FeModell _modell;
    private AbstraktZeitabhängigeKnotenlast _vorhandeneKnotenlast;
    private readonly WärmelastenKeys _lastenKeys;
    private string _lastId;

    public ZeitKnotentemperaturNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _lastenKeys = new WärmelastenKeys(modell);
        _lastenKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        _lastId = LastId.Text;
        if (_lastId == "")
        {
            _ = MessageBox.Show("zeitabhängige Knotenlast Id muss definiert sein", "neue zeitabhängige Knotenlast");
            return;
        }

        // vorhandene Knotenlast
        if (_modell.ZeitabhängigeKnotenLasten.Keys.Contains(_lastId))
        {
            _modell.ZeitabhängigeKnotenLasten.TryGetValue(_lastId, out _vorhandeneKnotenlast);
            Debug.Assert(_vorhandeneKnotenlast != null, nameof(_vorhandeneKnotenlast) + " != null");

            if (KnotenId.Text.Length > 0)
                _vorhandeneKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);

            if (Datei.IsChecked == true) _vorhandeneKnotenlast.VariationsTyp = 0;
            else if (Konstant.Text.Length > 0)
            {
                _vorhandeneKnotenlast.VariationsTyp = 1;
                _vorhandeneKnotenlast.KonstanteTemperatur = double.Parse(Konstant.Text);
            }
            else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
            {
                _vorhandeneKnotenlast.VariationsTyp = 2;
                _vorhandeneKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                _vorhandeneKnotenlast.Frequenz = double.Parse(Frequenz.Text);
                _vorhandeneKnotenlast.PhasenWinkel = double.Parse(Winkel.Text);
            }
            else if (Linear.Text.Length > 0)
            {
                _vorhandeneKnotenlast.VariationsTyp = 3;
                var delimiters = new[] { '\t' };
                var teilStrings = Linear.Text.Split(delimiters);
                var k = 0;
                char[] paarDelimiter = { ';' };
                var intervall = new double[2 * teilStrings.Length];
                for (var i = 0; i < intervall.Length; i += 2)
                {
                    var wertePaar = teilStrings[k].Split(paarDelimiter);
                    intervall[i] = double.Parse(wertePaar[0]);
                    intervall[i + 1] = double.Parse(wertePaar[1]);
                    k++;
                }
                _vorhandeneKnotenlast.Intervall = intervall;
            }
        }
        // neue Knotenlast
        else
        {
            var knotenId = "";
            ZeitabhängigeKnotenLast knotenlast = null;
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Datei.IsChecked == true)
            {
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, true)
                {
                    LastId = _lastId,
                    VariationsTyp = 0
                };
            }
            else if (Konstant.Text.Length > 0)
            {
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, double.Parse(Konstant.Text))
                {
                    LastId = _lastId,
                    VariationsTyp = 1
                };

            }
            else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
            {
                var amplitude = double.Parse(Amplitude.Text);
                var frequenz = 2 * Math.PI * double.Parse(Frequenz.Text);
                var winkel = Math.PI / 180 * double.Parse(Winkel.Text);
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, amplitude, frequenz, winkel)
                {
                    LastId = _lastId,
                    VariationsTyp = 2
                };
            }
            else if (Linear.Text.Length > 0)
            {
                var delimiters = new[] { '\t' };
                var teilStrings = Linear.Text.Split(delimiters);
                var k = 0;
                char[] paarDelimiter = { ';' };
                var intervall = new double[2 * teilStrings.Length];
                for (var i = 0; i < intervall.Length; i += 2)
                {
                    var wertePaar = teilStrings[k].Split(paarDelimiter);
                    intervall[i] = double.Parse(wertePaar[0]);
                    intervall[i + 1] = double.Parse(wertePaar[1]);
                    k++;
                }
                knotenlast = new ZeitabhängigeKnotenLast(knotenId, intervall) { LastId = _lastId, VariationsTyp = 3 };
            }
            _modell.ZeitabhängigeKnotenLasten.Add(_lastId, knotenlast);
        }
        Close();
        _lastenKeys?.Close();
        StartFenster.WärmeVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _lastenKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.ContainsKey(LastId.Text)) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text);
        Close();
        StartFenster.WärmeVisual.Close();

        if (!_modell.ZeitabhängigeKnotenLasten.Keys.Contains(LastId.Text)) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text);
        _lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        // neue zeitabhängige Knotenlastdefinitionen
        if (!_modell.ZeitabhängigeKnotenLasten.ContainsKey(LastId.Text))
        {
            KnotenId.Text = "";
            Datei.IsChecked = false;
            Konstant.Text = "";
            Amplitude.Text = "";
            Frequenz.Text = "";
            Winkel.Text = "";
            Linear.Text = "";
            return;
        }

        // vorhandene zeitabhängige Knotenlastdefinitionen
        _modell.ZeitabhängigeKnotenLasten.TryGetValue(LastId.Text, out _vorhandeneKnotenlast);
        Debug.Assert(_vorhandeneKnotenlast != null, nameof(_vorhandeneKnotenlast) + " != null");

        _lastId = _vorhandeneKnotenlast.LastId;
        KnotenId.Text = _vorhandeneKnotenlast.KnotenId;
        switch (_vorhandeneKnotenlast.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
                Konstant.Text = _vorhandeneKnotenlast.KonstanteTemperatur.ToString("G2");
                break;
            case 2:
                Amplitude.Text = _vorhandeneKnotenlast.Amplitude.ToString("G2");
                Frequenz.Text = _vorhandeneKnotenlast.Frequenz.ToString("G2");
                Winkel.Text = _vorhandeneKnotenlast.PhasenWinkel.ToString("G2");
                break;
            case 3:
                {
                    var intervall = _vorhandeneKnotenlast.Intervall;
                    var sb = new StringBuilder();
                    sb.Append(intervall[0].ToString("G2") + ";");
                    sb.Append(intervall[1].ToString("G2"));
                    for (var i = 2; i < intervall.Length; i += 2)
                    {
                        sb.Append("\t");
                        sb.Append(intervall[i].ToString("G2") + ";");
                        sb.Append(intervall[i + 1].ToString("G2"));
                    }
                    Linear.Text = sb.ToString();
                    break;
                }
        }
    }
}