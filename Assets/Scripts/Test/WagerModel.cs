using System;
using UnityEngine;
/// <summary>
/// Claud Code
/// First Change: "Reset" Function is a Unity namespace so renamed to "ResetBet"
/// </summary>
public class WagerModel : MonoBehaviour
{

    public int AvailableInventory { get; private set; }
    public int Wager { get; private set; }
    public bool IsConfirmed { get; private set; }

    /// <summary>The confirmed wager, or 0 if nothing has been confirmed.</summary>
    public int ConfirmedWager { get; private set; }

    public bool CanDecrease => !IsConfirmed && Wager > 0;
    public bool CanIncrease => !IsConfirmed && Wager < AvailableInventory;
    public bool CanConfirm => !IsConfirmed && Wager >= 1 && Wager <= AvailableInventory;

    /// <summary>Raised once per successful confirmation, with the confirmed wager.</summary>
    public event Action<int> Confirmed;

    /*
     * Original Code: 
    public WagerModel(int inventory)
    {
        ResetBet(inventory);
    }*/

    /// <summary>Clamps negative inventory to 0, sets wager to 1 (or 0), clears confirmation.</summary>
    /// Corrected Code. Original was "public void Reset(int inventory)"
    public void ResetBet(int inventory)
    {
        AvailableInventory = Math.Max(0, inventory);
        Wager = AvailableInventory >= 1 ? 1 : 0;
        IsConfirmed = false;
        ConfirmedWager = 0;
    }

    public bool TryIncrease()
    {
        if (!CanIncrease) return false;
        Wager++;
        return true;
    }

    public bool TryDecrease()
    {
        if (!CanDecrease) return false;
        Wager--;
        return true;
    }

    /// <summary>
    /// Re-validates independently of any button state. Returns false (and changes nothing)
    /// if already confirmed or the wager is out of range.
    /// </summary>
    public bool TryConfirm()
    {
        if (!CanConfirm) return false;
        IsConfirmed = true;
        ConfirmedWager = Wager;
        Confirmed?.Invoke(ConfirmedWager);
        return true;
    }
}