public class Stats
{
    int attack;
    int health;

    public Stats(int attack, int health)
    {
        this.attack = attack;
        this.health = health;
    }

    public override string ToString() {
        return attack + "/" + health;
    }
}
