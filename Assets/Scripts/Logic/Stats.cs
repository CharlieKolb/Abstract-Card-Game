public class Stats
{
    public int attack;
    public int health;

    public Stats(int attack, int health)
    {
        this.attack = attack;
        this.health = health;
    }

    public Stats(Stats other) {
        this.attack = other.attack;
        this.health = other.health;
    }

    public override string ToString() {
        return attack + "/" + health;
    }
}
