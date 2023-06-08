using Dzik.crypto.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dzik.keyStorageWindows
{
    internal static class PasswordStrengthInterpreter
    {
        internal static (string, Brush) Interprete(string candidate)
        {
            if (candidate.Length == 0)
            {
                
                return ("", Brushes.Red);
            }

            var estimate = PasswordStrengthGauge.Gauge(candidate);
            var tip = "";
            var estimateColor = Brushes.Red;
            switch (estimate)
            {
                case PasswordStrengthGauge.PasswordStrength.WEAK: tip = "(0/5) Słabe w opór"; estimateColor = Brushes.Red; break;
                case PasswordStrengthGauge.PasswordStrength.CASUAL: tip = "(1/5) Może starczy na ciekawskich"; estimateColor = Brushes.Orange; break;
                case PasswordStrengthGauge.PasswordStrength.SEMI_SERIOUS: tip = "(2/5) Starczy na ciekawskich"; estimateColor = Brushes.DarkKhaki; break;
                case PasswordStrengthGauge.PasswordStrength.ENTREPRISE: tip = "(3/5) Bezpieczne!"; estimateColor = Brushes.Green; break;
                case PasswordStrengthGauge.PasswordStrength.TOP_SECRET: tip = "(4/5) Bardzo bezpieczne!"; estimateColor = Brushes.DarkGreen; break;
                case PasswordStrengthGauge.PasswordStrength.MAXED_OUT: tip = "(5/5) Golden"; estimateColor = Brushes.Gold; break;
            }

            return (tip, estimateColor);
        }
    }
}
