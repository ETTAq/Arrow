using UnityEngine;

public record HitTarget;
public class BowAdded
{
    public GameObject bowObj;
    public BowAdded(GameObject bowObj) { this.bowObj = bowObj; }
}

public class ChargeSpeedUpgraded
{
    public float factor;
    public ChargeSpeedUpgraded(float factor) { this.factor = factor; }
}
