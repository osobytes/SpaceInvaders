using UnityEngine;

internal class FireCooldown
{
    private float value;
    public bool CanFire => value == 0f;
    public void SetCooldown(float value)
    {
        this.value = value;
    }

    public void Cooldown()
    {
        if (this.value > 0f)
        {
            this.value = Mathf.Max(0f, this.value - Time.deltaTime);
        }
    }
}