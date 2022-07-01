using System.Collections.Generic;

public abstract class CardData {
    public string name;
    public string pathToImage;
    public Energy cost;
    public Energy sac;

    public List<Effect> effects;

    public CardData(string name, Energy cost, Energy sac, string pathToImage, List<Effect> effects = null) {
        this.name = name;
        this.cost = cost;
        this.sac = sac;
        this.effects = effects == null ? new List<Effect>() : effects;
        this.pathToImage = pathToImage;
    }
}

public class CreatureCardData : CardData {
    public Stats stats;

    public CreatureCardData(string name, Stats stats, Energy cost, Energy sac, string pathToImage, List<Effect> effects = null) : base(name, cost, sac, pathToImage, effects) {
        this.stats = stats;
    }
}

public static class Cards {
    public static List<CreatureCardData> creatureList = new List<CreatureCardData> {
        new CreatureCardData("Brute", new Stats(4, 2), Energy.FromRed(2), Energy.FromRed(2), ""),
        new CreatureCardData("Fisher", new Stats(3, 3), Energy.FromBlue(2), Energy.FromBlue(2), ""),
        new CreatureCardData("Guardian", new Stats(2, 4), Energy.FromGreen(2), Energy.FromGreen(2), ""),
    };

    public static Dictionary<string, CardData> cards = new Dictionary<string, CardData>();
    public static Dictionary<string, CreatureCardData> creatures = new Dictionary<string, CreatureCardData>();

    static Cards() {
        foreach (var creature in creatureList) {
            cards[creature.name] = creature;
            creatures[creature.name] = creature;
        }
    }
}