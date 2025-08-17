using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConnection 
{
    private readonly WarServer _server;
    private readonly float _timeoutSeconds;
    private readonly int _maxRetries;

    public ServerConnection(WarServer server, float timeoutSeconds = 3f, int maxRetries = 1)
    {
        _server = server;
        _timeoutSeconds = timeoutSeconds;
        _maxRetries = maxRetries;
    }

    public async UniTask<RoundResolution> DrawRoundWithRetryAsync(Action<string> onStatus)
    {
        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
                return await _server.DrawRoundAsync(cts.Token);
            }
            catch (FakeTimeoutException)
            {
                onStatus?.Invoke("Timeout – retrying…");
                await UniTask.Delay(300);
            }
            catch (FakeNetworkException)
            {
                onStatus?.Invoke("Network error – retrying…");
                await UniTask.Delay(300);
            }
        }

        throw new Exception("Failed after retries.");
    }
}
