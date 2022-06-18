using System.Collections.Generic;

public class InteractionArgs {
    public List<Effect> effects;
}

public class Interactable {
    public delegate void OnInteraction(object sender, InteractionArgs e);

    public event OnInteraction onInteraction;
}