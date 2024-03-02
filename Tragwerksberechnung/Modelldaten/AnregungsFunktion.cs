namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

internal class AnregungsFunktion(double dt, int nSteps, int dimension)
{
    private double _time;
    private double[][] _f;

    public double[][] GetForce()
    {
        _f = new double[nSteps + 1][];
        for (var i = 0; i < (nSteps + 1); i++) _f[i] = new double[dimension];
        const double t1 = 0.8;

        for (var counter = 1; counter < nSteps; counter++)
        {
            _time += dt;
            double force;
            if (_time >= 0 & _time <= t1) force = _time / t1;
            else if (_time > t1 & _time <= 2 * t1) force = 2 - _time / t1;
            else if (_time > 2 * t1 & _time <= 4 * t1) force = 1 - _time / (2 * t1);
            else if (_time > 4 * t1 & _time <= 6 * t1) force = -3 + _time / (2 * t1);
            else if (_time > 6 * t1 & _time <= 7 * t1) force = -6 + _time / t1;
            else if (_time > 7 * t1 & _time <= 8 * t1) force = 8 - _time / t1;
            else force = 0;
            for (var i = 0; i < dimension; i++)
                _f[counter][i] = force;
        }
        return _f;
    }
}