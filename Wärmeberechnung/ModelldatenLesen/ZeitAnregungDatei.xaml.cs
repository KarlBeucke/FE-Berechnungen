using FEBibliothek.Modell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen
{
    public partial class ZeitAnregungDatei
    {
        private int _nSchritte, _dimension;
        private readonly double _tmax, _dt, _amplitude, _frequenz, _phasenwinkel;
        private double[][] _anregungsfunktion;

        public ZeitAnregungDatei(FeModell feModell)
        {
            InitializeComponent();
            ShowDialog();
            _amplitude = double.Parse(Amplitude.Text);
            _frequenz = double.Parse(Frequenz.Text);
            _phasenwinkel = double.Parse(Phasenwinkel.Text);
            _tmax = double.Parse(Tmax.Text);
            _dt = double.Parse(DeltaT.Text);
            feModell.Zeitintegration.Tmax = _tmax;
            feModell.Zeitintegration.Dt = _dt;
            SchreibDatei(Dateiname.Text);
        }

        private void SchreibDatei(string name)
        {
            _dimension = 1;

            _nSchritte = (int)(_tmax / _dt);
            _anregungsfunktion = new double[_nSchritte + 1][];
            for (var i = 0; i < _nSchritte + 1; i++)
                _anregungsfunktion[i] = new double[_dimension];

            _anregungsfunktion = Anregungsfunktion();
            var zeilen = new List<string>();
            try
            {
                for (var k = 0; k < _nSchritte + 1; k++)
                {
                    zeilen.Add(_anregungsfunktion[k][0].ToString(CultureInfo.InvariantCulture));
                    for (var i = 1; i < _dimension; i++)
                    {
                        zeilen.Add("\t" + _anregungsfunktion[k][i]);
                    }
                }
            }
            finally
            {
                var initial = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                              "\\FE Berechnungen\\Beispiele\\Wärmeberechnung\\instationär\\Anregungsdateien\\";
                var dateiDialog = new OpenFileDialog
                {
                    Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                    InitialDirectory = initial
                };

                if (Directory.Exists(dateiDialog.InitialDirectory))
                {
                    dateiDialog.InitialDirectory = initial;
                    var dateiPfad = dateiDialog.InitialDirectory + name + "." + DeltaT.Text + ".txt";
                    File.WriteAllLines(dateiPfad, zeilen);
                }
                else
                {
                    _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                        " Anregungsdatei am Speicherort von \\FE Berechnungen\\Beispiele\\Wärmeberechnung\\instationär\\Anregungsdateien",
                        "Wärmeberechnung");
                    dateiDialog.ShowDialog();
                }
            }
        }

        private double[][] Anregungsfunktion()
        {
            var frequenz = 2 * Math.PI * _frequenz;
            var phasenwinkel = Math.PI / 180 * _phasenwinkel;
            for (var k = 0; k < _nSchritte + 1; k++)
            {
                var zeit = k * _dt;
                _anregungsfunktion[k][0] = _amplitude * Math.Sin(frequenz * zeit + phasenwinkel);
                for (var i = 1; i < _dimension; i++)
                    _anregungsfunktion[k][i] = 0;
            }
            return _anregungsfunktion;
        }
        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}