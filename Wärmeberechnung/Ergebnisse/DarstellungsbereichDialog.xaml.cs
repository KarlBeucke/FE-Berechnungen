using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class Darstellungsbereich
{
    public double Tmin, Tmax;
    public double MaxTemperatur;
    public double MaxWärmefluss;

    public Darstellungsbereich(double tmin, double tmax, double maxTemperatur, double maxWärmefluss)
    {
        InitializeComponent();
        Tmin = tmin;
        Tmax = tmax;
        MaxTemperatur = maxTemperatur;
        MaxWärmefluss = maxWärmefluss;
        //TxtMinZeit.Text = Tmin.ToString(CultureInfo.CurrentCulture);
        TxtMaxZeit.Text = Tmax.ToString(CultureInfo.CurrentCulture);
        TxtMaxTemperatur.Text = MaxTemperatur.ToString("N2");
        TxtMaxWärmefluss.Text = MaxWärmefluss.ToString("N2");
        ShowDialog();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        //Tmin = double.Parse(TxtMinZeit.Text);
        Tmax = double.Parse(TxtMaxZeit.Text);
        MaxTemperatur = double.Parse(TxtMaxTemperatur.Text);
        MaxWärmefluss = double.Parse(TxtMaxWärmefluss.Text);
        Close();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}