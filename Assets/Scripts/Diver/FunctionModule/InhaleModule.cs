[System.Serializable]
public class InhaleModule
{
    private bool enabled = false;

    public bool Enabled
    {
        get
        {
            return enabled;
        }
        set
        {
            if (enabled != value)
            {
                enabled = value;
                ToggleModule();
            }
        }
    }
    public float InhaleStrength = 100;
    public float MaxInhaleRange = 100;
    public float ConeAngle = 50;

    private void ToggleModule()
    {
        if (RelativePositionControl.Instance.MyPlayerControl != null)
        {
            RelativePositionControl.Instance.MyPlayerControl.InhaleModuleEnabled = enabled;
        }
    }
}