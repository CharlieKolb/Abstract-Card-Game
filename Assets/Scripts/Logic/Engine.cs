using System.Threading.Tasks;
using System.Collections.Generic;

public interface ITargetable {}

public interface IInteractionHandler {
    Task<EffectContext> resolveTarget(EffectTarget effectTarget);
    Task<Interaction> selectInteraction(List<Interaction> interactions);
}

public struct SideConfig {
    IInteractionHandler handler;
    DeckBlueprint deck;
}

public class Engine {
    GS gameState;

    List<IEnumerator<bool>> interactionQueue = new List<IEnumerator<bool>>();

    public Engine(SideConfig s1, SideConfig s2) {}

    public List<Interaction> getInteractions(GS gameState) {
        var res = new List<Interaction>();

        return res;
    }
}

