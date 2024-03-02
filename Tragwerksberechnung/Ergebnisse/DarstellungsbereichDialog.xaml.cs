using System.Globalization;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class DarstellungsbereichDialog
{
    public double Tmin, Tmax;
    public double MaxVerformung;
    public double MaxBeschleunigung;

    public DarstellungsbereichDialog(double tmin, double tmax, double maxVerformung, double maxBeschleunigung)
    {
        InitializeComponent();
        Tmin = tmin;
        Tmax = tmax;
        MaxVerformung = maxVerformung;
        MaxBeschleunigung = maxBeschleunigung;
        //TxtMinZeit.Text = Tmin.ToString(CultureInfo.CurrentCulture);
        TxtMaxZeit.Text = Tmax.ToString(CultureInfo.CurrentCulture);
        TxtMaxVerformung.Text = MaxVerformung.ToString("N4");
        TxtMaxBeschleunigung.Text = MaxBeschleunigung.ToString("N4");
        ShowDialog();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        //Tmin = double.Parse(TxtMinZeit.Text);
        Tmax = double.Parse(TxtMaxZeit.Text);
        MaxVerformung = double.Parse(TxtMaxVerformung.Text);
        MaxBeschleunigung = double.Parse(TxtMaxBeschleunigung.Text);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}