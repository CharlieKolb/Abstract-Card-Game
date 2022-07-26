public class Side
{
    public Player player;
    public Hand hand;
    public CreatureCollection creatures;
    public Deck deck;
    public Graveyard graveyard;
    public Energy energy;
    public Energy maxEnergy;

    public Side(DeckBlueprint deckBlueprint, Player player) {
        deck = Deck.FromBlueprint(deckBlueprint);
        this.player = player;

        hand = new Hand();
        creatures = new CreatureCollection();
        graveyard = new Graveyard();
        maxEnergy = new Energy(); 
        energy = new Energy(maxEnergy);

        GS.ga_global.phaseActionHandler.after.on(PhaseActionKey.ENTER, p => {
            if (p.phase == Phases.drawPhase && GS.gameStateData_global.activeController.player == player) {
                energy = new Energy(maxEnergy);
            }
        });
    }

    public bool hasOptions()
    {
        foreach (var e in hand.getExisting())
        {
            if (e.value.canUseFromHand(player)) return true;
        }

        return false;
    }
}
