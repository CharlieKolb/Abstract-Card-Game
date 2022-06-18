public class Resources {
    public int cost;

    public Resources(int cost) {
        this.cost = cost;
    }

    public bool canBePaid() {
        return this.cost <= GS.gameStateData.activeController.player.side.resources.cost;
    }    
}