using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class DialogLöschWärmemodellObjekt
{
    private bool _löschFlag;

    public DialogLöschWärmemodellObjekt(bool delete)
    {
        _löschFlag = delete;
        InitializeComponent();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        _löschFlag = false;
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _löschFlag = false;
        Close();
    }
}