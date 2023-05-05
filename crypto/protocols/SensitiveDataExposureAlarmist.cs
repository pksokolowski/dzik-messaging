using Dzik.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dzik.crypto.protocols
{
    internal class SensitiveDataExposureAlarmist
    {
        private CancellationTokenSource TaskController;

        internal void StartEggTimer()
        {
            if (TaskController != null) throw new Exception("Illegal state - task controller already exists");

            TaskController = new CancellationTokenSource();
            var token = TaskController.Token;
            Task.Delay(Constants.MostSensitiveDataRamExposureMaxTimeMillis, token).ContinueWith((_) =>
            {
                if (!token.IsCancellationRequested)
                {
                    DialogShower.ShowError("Krytyczne. Wykryto niespodziewanie długą ekspozycję chronionych danych w pamięci RAM.\n\nPoinformuj autora Dzika o zaistnieniu problemu.");
                }
            }, token);
        }

        internal void CancelEggTimer()
        {
            TaskController?.Cancel();
            TaskController?.Dispose();
            TaskController = null;
        }
    }
}
