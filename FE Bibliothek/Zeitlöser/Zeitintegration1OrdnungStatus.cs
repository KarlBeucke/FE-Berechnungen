﻿using FEBibliothek.Gleichungslöser;
using FEBibliothek.Modell;
using FEBibliothek.Werkzeuge;

namespace FEBibliothek.Zeitlöser
{
    public class Zeitintegration1OrdnungStatus
    {
        private readonly int _dimension;
        private readonly double _dt, _alfa;
        private readonly double[] _c;
        private double[] That { get; set; }
        private readonly double[][] k;
        public double[][] ForcingFunction { get; }
        private readonly int[] _profile;
        private readonly bool[] _status;

        public double[][] Temperature { get; set; }
        public double[][] TemperaturGradient { get; set; }

        public Zeitintegration1OrdnungStatus(Gleichungen systemEquations, double[][] excitation,
                                                double dt, double alfa, double[][] initial)
        {
            _c = systemEquations.DiagonalMatrix;
            k = systemEquations.Matrix;
            ForcingFunction = excitation;
            _profile = systemEquations.Profil;
            _status = systemEquations.Status;
            _dt = dt;
            _alfa = alfa;
            Temperature = initial;
            _dimension = k.Length;
        }

        public Zeitintegration1OrdnungStatus(double[] c, double[][] k, double[][] excitation,
                                                int[] profile, bool[] status,
                                                double dt, double alfa, double[][] initial)
        {
            _c = c;
            this.k = k;
            ForcingFunction = excitation;
            _profile = profile;
            _status = status;
            _dt = dt;
            _alfa = alfa;
            Temperature = initial;
            _dimension = this.k.Length;
        }

        public void Ausführung()
        {
            var alfaDt = _alfa * _dt;
            var dtAlfa = _dt * (1 - _alfa);
            var primal = new double[_dimension];

            var timeSteps = Temperature.Length;
            That = new double[_dimension];
            TemperaturGradient = new double[timeSteps][];
            for (var i = 0; i < timeSteps; i++) { TemperaturGradient[i] = new double[_dimension]; }

            // berechne Temperaturgradienten am Anfang
            var rhs = MatrizenAlgebra.Mult(k, Temperature[0], _profile);
            for (var i = 0; i < _dimension; i++)
                TemperaturGradient[0][i] = (ForcingFunction[0][i] - rhs[i]) / _c[i];

            // ermittle konstante Koeffizientenmatrix
            var cm = new double[_dimension][];
            for (var row = 0; row < _dimension; row++)
            {
                cm[row] = new double[row + 1 - _profile[row]];
                for (var col = 0; col <= (row - _profile[row]); col++)
                    cm[row][col] = alfaDt * k[row][col];
                cm[row][row - _profile[row]] += _c[row];
            }

            var profileSolverStatus =
                            new ProfillöserStatus(cm, rhs, primal, _status, _profile);
            profileSolverStatus.Dreieckszerlegung();

            for (var counter = 1; counter < timeSteps; counter++)
            {
                // berechne Temperaturgradienten an festgehaltenen Knoten 
                for (var i = 0; i < _dimension; i++)
                    if (_status[i])
                        TemperaturGradient[counter][i] = (Temperature[counter][i] - Temperature[counter - 1][i]) / _dt;

                // berechne T(hat) für nächsten Schritt
                for (var i = 0; i < _dimension; i++)
                    if (_status[i]) That[i] = Temperature[counter][i];
                    else That[i] = Temperature[counter - 1][i] + dtAlfa * TemperaturGradient[counter - 1][i];

                // Modifikation der rechten Seite
                rhs = MatrizenAlgebra.Mult(k, That, _status, _profile);
                var rhSfr = MatrizenAlgebra.MultUl(cm, TemperaturGradient[counter], _status, _profile);
                for (var i = 0; i < _dimension; i++)
                    rhs[i] = ForcingFunction[counter][i] - rhSfr[i] - rhs[i];

                // Rückwärtseinsetzung
                profileSolverStatus.SetzRechteSeite(rhs);
                profileSolverStatus.LösPrimal();

                // Temperaturen und Gradienten am nächsten Zeitschritt an nicht festgehaltenen Knoten
                for (var i = 0; i < _dimension; i++)
                {
                    if (_status[i]) continue;
                    TemperaturGradient[counter][i] = primal[i];
                    Temperature[counter][i] = That[i] + alfaDt * primal[i];
                }
            }
        }
    }
}