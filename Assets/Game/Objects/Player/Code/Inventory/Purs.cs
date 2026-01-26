using UnityEngine;

public class Purs
{
    private int coins = 0;
    public bool setcoins(int _coins)
    {   
        coins = _coins;
        return true;
    }
    public int getcoins()
    {
        return coins;
    }
    public bool addCoins(int _coins)
    {
        coins += _coins;
        return true;
    }
}
