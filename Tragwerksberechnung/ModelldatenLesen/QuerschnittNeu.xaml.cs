using FEBibliothek.Modell;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class QuerschnittNeu
{
    private readonly FeModell _modell;
    private Querschnitt _querschnitt, _vorhandenerQuerschnitt;
    private readonly QuerschnittKeys _querschnittKeys;
    public QuerschnittNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _querschnittKeys = new QuerschnittKeys(modell);
        _querschnittKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var querschnittId = QuerschnittId.Text;
        if (querschnittId == "")
        {
            _ = MessageBox.Show("Querschnitt Id muss definiert sein", "neuer Querschnitt");
            return;
        }

        // vorhandener Querschnitt
        if (_modell.Querschnitt.Keys.Contains(QuerschnittId.Text))
        {
            _modell.Querschnitt.TryGetValue(querschnittId, out _vorhandenerQuerschnitt);
            if(_vorhandenerQuerschnitt != null)
            {
                if (Fläche.Text == string.Empty)
                {
                    _ = MessageBox.Show("mindestens Fläche muss definiert sein", "neuer Querschnitt");
                    return;
                }
                _vorhandenerQuerschnitt.QuerschnittsWerte[0] = double.Parse(Fläche.Text);

                if (Ixx.Text != string.Empty) _vorhandenerQuerschnitt.QuerschnittsWerte[1] = double.Parse(Ixx.Text);
            }
        }
        // neuer Querschnitt
        else
        {
            if (Fläche.Text != string.Empty)
            {
                double ixx = 0;
                var fläche = double.Parse(Fläche.Text);
                if (Ixx.Text != string.Empty) ixx = double.Parse(Ixx.Text);
                _querschnitt = new Querschnitt(fläche, ixx)
                {
                    QuerschnittId = querschnittId
                };
                _modell.Querschnitt.Add(querschnittId, _querschnitt);
            }
        }
        _querschnittKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _querschnittKeys?.Close();
        Close();
    }

    private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Querschnitt.ContainsKey(QuerschnittId.Text))
        {
            Fläche.Text = "";
            Ixx.Text = "";
            return;
        }

        // vorhandene Querschnittdefinition
        _modell.Querschnitt.TryGetValue(QuerschnittId.Text, out _vorhandenerQuerschnitt);
        if (_vorhandenerQuerschnitt == null) return;
        QuerschnittId.Text = _vorhandenerQuerschnitt.QuerschnittId;

        Fläche.Text = _vorhandenerQuerschnitt.QuerschnittsWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        if (Ixx.Text == "") Ixx.Text = _vorhandenerQuerschnitt.QuerschnittsWerte[1].ToString("G3", CultureInfo.CurrentCulture);
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (_vorhandenerQuerschnitt != null) _modell.Querschnitt.Remove(_vorhandenerQuerschnitt.QuerschnittId);
        _querschnittKeys.Close();
        Close();
    }
}