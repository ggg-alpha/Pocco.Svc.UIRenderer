using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Pocco.Client.Web.Services;

public class CircuitClosureDetector : CircuitHandler
{
    public event Action<string>? Disconnected;

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Disconnected?.Invoke(circuit.Id);
        if (Disconnected is not null)
        {
            foreach (Action<string> e in Disconnected.GetInvocationList().Cast<Action<string>>())
            {
                Disconnected -= e;
            }
        }
        return Task.CompletedTask;
    }
}
