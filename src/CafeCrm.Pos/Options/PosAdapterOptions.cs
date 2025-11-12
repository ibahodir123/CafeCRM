namespace CafeCrm.Pos.Options;

public class PosAdapterOptions
{
    public bool Enabled { get; set; } = true;
    public int PollingIntervalSeconds { get; set; } = 30;
    public string Endpoint { get; set; } = "mock://pos";
}
