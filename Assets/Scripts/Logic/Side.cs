public class Side
{
    public Player player;
    public Hand hand;
    public CreatureCollection creatures;
    public Deck deck;
    public Graveyard graveyard;
    public Energy energy;
    public Energy maxEnergy;

    // TODO(GlobalConfig) - This is a good example of something that should be part of the game config. The side itself shouldn't care about resetting its own energy, a rule in the gameConfig should do that
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
}
