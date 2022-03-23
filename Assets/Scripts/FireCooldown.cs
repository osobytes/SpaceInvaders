using UnityEngine;

internal struct FireCooldown
{
    private readonly float BaseCooldown;
    private float CurrentCooldown;
    public bool CanFire => CurrentCooldown == 0f;

    public FireCooldown(float baseCooldown)
    {
        BaseCooldown = baseCooldown;
        CurrentCooldown = 0f;
    }

    public void Cooldown()
    {
        if (CurrentCooldown != 0f)
        {
            return;
        }
        CurrentCooldown = BaseCooldown;
    }

    public void Update()
    {
        if (CurrentCooldown > 0f)
        {
            CurrentCooldown = Mathf.Max(CurrentCooldown - Time.deltaTime, 0f);
        }
    }
}
