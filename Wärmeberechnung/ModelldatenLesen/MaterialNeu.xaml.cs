using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class MaterialNeu
{
    private readonly FeModell _modell;
    private AbstraktMaterial _material, _vorhandenesMaterial;
    private readonly MaterialKeys _materialKeys;

    public MaterialNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        _materialKeys = new MaterialKeys(modell);
        _materialKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var materialId = MaterialId.Text;
        if (materialId == "")
        {
            _ = MessageBox.Show("Material Id muss definiert sein", "neues Material");
            return;
        }

        var leitfähigkeit = new double[3];
        double dichteLeitfähigkeit = 0;
        // vorhandenes Material
        if (_modell.Material.Keys.Contains(MaterialId.Text))
        {
            _modell.Material.TryGetValue(materialId, out _vorhandenesMaterial);
            Debug.Assert(_vorhandenesMaterial != null, nameof(_vorhandenesMaterial) + " != null");

            if (LeitfähigkeitX.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[0] = double.Parse(LeitfähigkeitX.Text);
            if (LeitfähigkeitY.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[1] = double.Parse(LeitfähigkeitY.Text);
            if (LeitfähigkeitZ.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[2] = double.Parse(LeitfähigkeitZ.Text);
            if (DichteLeitfähigkeit.Text.Length > 0) _vorhandenesMaterial.MaterialWerte[3] = double.Parse(DichteLeitfähigkeit.Text);
        }
        // neues Material
        else
        {
            if (LeitfähigkeitX.Text.Length > 0)
                leitfähigkeit[0] = double.Parse(LeitfähigkeitX.Text);
            if (LeitfähigkeitY.Text.Length > 0)
                leitfähigkeit[1] = double.Parse(LeitfähigkeitY.Text);
            if (LeitfähigkeitZ.Text.Length > 0)
                leitfähigkeit[2] = double.Parse(LeitfähigkeitZ.Text);
            if (DichteLeitfähigkeit.Text.Length > 0)
                dichteLeitfähigkeit = double.Parse(DichteLeitfähigkeit.Text);
            _material = new Modelldaten.Material(materialId, leitfähigkeit, dichteLeitfähigkeit);
            _modell.Material.Add(materialId, _material);
        }
        _materialKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _materialKeys?.Close();
        Close();
    }

    private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Material.ContainsKey(MaterialId.Text))
        {
            LeitfähigkeitX.Text = "";
            LeitfähigkeitY.Text = "";
            LeitfähigkeitZ.Text = "";
            DichteLeitfähigkeit.Text = "";
            return;
        }

        // vorhandene Materialdefinition
        _modell.Material.TryGetValue(MaterialId.Text, out _vorhandenesMaterial);
        Debug.Assert(_vorhandenesMaterial != null, nameof(_vorhandenesMaterial) + " != null"); MaterialId.Text = "";

        MaterialId.Text = _vorhandenesMaterial.MaterialId;

        LeitfähigkeitX.Text = _vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        LeitfähigkeitY.Text = _vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
        LeitfähigkeitZ.Text = _vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        DichteLeitfähigkeit.Text = _vorhandenesMaterial.MaterialWerte[3].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (_vorhandenesMaterial != null) _modell.Material.Remove(_vorhandenesMaterial.MaterialId);
        _materialKeys.Close();
        Close();
    }
}