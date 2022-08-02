

public class GAction {
    public class Phase {
        public class Enter : Invokable<Enter> {
            public GamePhase phase;
        }
    }


    void f() {
        GAction.Phase.Enter.From(new GS(null), "", new GAction.Phase.Enter());
    }
}