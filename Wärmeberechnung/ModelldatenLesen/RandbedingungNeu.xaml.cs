using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class RandbedingungNeu
{
    private readonly FeModell _modell;
    private AbstraktRandbedingung _vorhandeneRandbedingung;
    private readonly RandbedingungenKeys _randbedingungenKeys;

    public RandbedingungNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        _randbedingungenKeys = new RandbedingungenKeys(modell);
        _randbedingungenKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var randbedingungId = RandbedingungId.Text;
        var knotenId = KnotenId.Text;
        double temperatur = 0;
        if (randbedingungId == "")
        {
            _ = MessageBox.Show("Randbedingung Id muss definiert sein", "neue Randbedingung");
            return;
        }

        // vorhandene Randbedingung
        if (_modell.Randbedingungen.Keys.Contains(randbedingungId))
        {
            _modell.Randbedingungen.TryGetValue(randbedingungId, out _vorhandeneRandbedingung);
            Debug.Assert(_vorhandeneRandbedingung != null, nameof(_vorhandeneRandbedingung) + " != null");

            if (Temperatur.Text.Length > 0) _vorhandeneRandbedingung.Vordefiniert[0] = double.Parse(Temperatur.Text);
        }
        // neues Randbedingung
        else
        {
            if (Temperatur.Text.Length > 0)
                temperatur = double.Parse(Temperatur.Text);

            var randbedingung = new Modelldaten.Randbedingung(randbedingungId, knotenId, temperatur)
            {
                RandbedingungId = randbedingungId
            };
            _modell.Randbedingungen.Add(randbedingungId, randbedingung);
        }
        _randbedingungenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _randbedingungenKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Randbedingungen.Keys.Contains(RandbedingungId.Text)) return;
        _modell.Randbedingungen.Remove(RandbedingungId.Text);
        _randbedingungenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void RandbedingungIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Randbedingungen.ContainsKey(RandbedingungId.Text))
        {
            KnotenId.Text = "";
            Temperatur.Text = "";
            return;
        }

        // vorhandene Randbedingungsdefinitionen
        _modell.Randbedingungen.TryGetValue(RandbedingungId.Text, out _vorhandeneRandbedingung);
        Debug.Assert(_vorhandeneRandbedingung != null, nameof(_vorhandeneRandbedingung) + " != null");
        KnotenId.Text = _vorhandeneRandbedingung.KnotenId;
        Temperatur.Text = _vorhandeneRandbedingung.Vordefiniert[0].ToString("G3");
    }
}