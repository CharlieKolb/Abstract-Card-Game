// AUTOGENERATED BY Assets/Scripts/Logic/reactions/generate.py
public class Reactions {
    public class CARD {
        public class SACCED : Invokable {
            public new static string Key = "card.sacced";
            // Payload `card`
            public Card card;
            // Payload `side`
            public Side side;
            public SACCED(
                GS gameState,
                // Payload `card`,
                Card card,
                // Payload `side`,
                Side side
            ) : base(gameState, SACCED.Key) {
                // Payload `card`
                this.card = card;
                // Payload `side`
                this.side = side;
            }
        }
        public class USED : Invokable {
            public new static string Key = "card.used";
            // Payload `card`
            public Card card;
            // Payload `side`
            public Side side;
            public USED(
                GS gameState,
                // Payload `card`,
                Card card,
                // Payload `side`,
                Side side
            ) : base(gameState, USED.Key) {
                // Payload `card`
                this.card = card;
                // Payload `side`
                this.side = side;
            }
        }
    }
    public class CREATURE {
        public class ATTACK : Invokable {
            public new static string Key = "creature.attack";
            // Payload `creature`
            public CreatureEntity creature;
            // Payload `targetCreature`
            public CreatureEntity targetCreature;
            public ATTACK(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature,
                // Payload `targetCreature`,
                CreatureEntity targetCreature
            ) : base(gameState, ATTACK.Key) {
                // Payload `creature`
                this.creature = creature;
                // Payload `targetCreature`
                this.targetCreature = targetCreature;
            }
        }
        public class DAMAGED : Invokable {
            public new static string Key = "creature.damaged";
            // Payload `creature`
            public CreatureEntity creature;
            // Payload `damage`
            public int damageAmount;
            public DAMAGED(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature,
                // Payload `damage`,
                int damageAmount
            ) : base(gameState, DAMAGED.Key) {
                // Payload `creature`
                this.creature = creature;
                // Payload `damage`
                this.damageAmount = damageAmount;
            }
        }
        public class DEATH : Invokable {
            public new static string Key = "creature.death";
            // Payload `creature`
            public CreatureEntity creature;
            public DEATH(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature
            ) : base(gameState, DEATH.Key) {
                // Payload `creature`
                this.creature = creature;
            }
        }
        public class SUMMON : Invokable {
            public new static string Key = "creature.summon";
            // Payload `creature`
            public CreatureEntity creature;
            public SUMMON(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature
            ) : base(gameState, SUMMON.Key) {
                // Payload `creature`
                this.creature = creature;
            }
        }
    }
    public class CREATURES {
        public class REMOVED : Invokable {
            public new static string Key = "creatures.removed";
            // Payload `creature`
            public CreatureEntity creature;
            // Payload `creatures`
            public CreatureCollection creatures;
            public REMOVED(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature,
                // Payload `creatures`,
                CreatureCollection creatures
            ) : base(gameState, REMOVED.Key) {
                // Payload `creature`
                this.creature = creature;
                // Payload `creatures`
                this.creatures = creatures;
            }
        }
        public class SPAWNED : Invokable {
            public new static string Key = "creatures.spawned";
            // Payload `creature`
            public CreatureEntity creature;
            // Payload `creatures`
            public CreatureCollection creatures;
            // Payload `index`
            public int index;
            public SPAWNED(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature,
                // Payload `creatures`,
                CreatureCollection creatures,
                // Payload `index`,
                int index
            ) : base(gameState, SPAWNED.Key) {
                // Payload `creature`
                this.creature = creature;
                // Payload `creatures`
                this.creatures = creatures;
                // Payload `index`
                this.index = index;
            }
        }
        public class SUMMONED : Invokable {
            public new static string Key = "creatures.summoned";
            // Payload `creature`
            public CreatureEntity creature;
            // Payload `creatures`
            public CreatureCollection creatures;
            public SUMMONED(
                GS gameState,
                // Payload `creature`,
                CreatureEntity creature,
                // Payload `creatures`,
                CreatureCollection creatures
            ) : base(gameState, SUMMONED.Key) {
                // Payload `creature`
                this.creature = creature;
                // Payload `creatures`
                this.creatures = creatures;
            }
        }
    }
    public class DECK {
        public class DRAW : Invokable {
            public new static string Key = "deck.draw";
            // Payload `side`
            public Side side;
            public DRAW(
                GS gameState,
                // Payload `side`,
                Side side
            ) : base(gameState, DRAW.Key) {
                // Payload `side`
                this.side = side;
            }
        }
    }
    public class EFFECT {
        public class TRIGGERED : Invokable {
            public new static string Key = "effect.triggered";
            // Payload `effect`
            public Effect effect;
            public TRIGGERED(
                GS gameState,
                // Payload `effect`,
                Effect effect
            ) : base(gameState, TRIGGERED.Key) {
                // Payload `effect`
                this.effect = effect;
            }
        }
    }
    public class ENERGY {
        public class PAY : Invokable {
            public new static string Key = "energy.pay";
            // Payload `energy`
            public Energy energyAmount;
            // Payload `side`
            public Side side;
            public PAY(
                GS gameState,
                // Payload `energy`,
                Energy energyAmount,
                // Payload `side`,
                Side side
            ) : base(gameState, PAY.Key) {
                // Payload `energy`
                this.energyAmount = energyAmount;
                // Payload `side`
                this.side = side;
            }
        }
        public class RECHARGE : Invokable {
            public new static string Key = "energy.recharge";
            // Payload `energy`
            public Energy energyAmount;
            // Payload `side`
            public Side side;
            public RECHARGE(
                GS gameState,
                // Payload `energy`,
                Energy energyAmount,
                // Payload `side`,
                Side side
            ) : base(gameState, RECHARGE.Key) {
                // Payload `energy`
                this.energyAmount = energyAmount;
                // Payload `side`
                this.side = side;
            }
        }
        public class SAC : Invokable {
            public new static string Key = "energy.sac";
            // Payload `energy`
            public Energy energyAmount;
            // Payload `side`
            public Side side;
            public SAC(
                GS gameState,
                // Payload `energy`,
                Energy energyAmount,
                // Payload `side`,
                Side side
            ) : base(gameState, SAC.Key) {
                // Payload `energy`
                this.energyAmount = energyAmount;
                // Payload `side`
                this.side = side;
            }
        }
    }
    public class HAND {
        public class ADDED : Invokable {
            public new static string Key = "hand.added";
            // Payload `card`
            public Card card;
            // Payload `hand`
            public Hand hand;
            public ADDED(
                GS gameState,
                // Payload `card`,
                Card card,
                // Payload `hand`,
                Hand hand
            ) : base(gameState, ADDED.Key) {
                // Payload `card`
                this.card = card;
                // Payload `hand`
                this.hand = hand;
            }
        }
        public class REMOVED : Invokable {
            public new static string Key = "hand.removed";
            // Payload `card`
            public Card card;
            // Payload `hand`
            public Hand hand;
            public REMOVED(
                GS gameState,
                // Payload `card`,
                Card card,
                // Payload `hand`,
                Hand hand
            ) : base(gameState, REMOVED.Key) {
                // Payload `card`
                this.card = card;
                // Payload `hand`
                this.hand = hand;
            }
        }
    }
    public class PHASE {
        public class ENTER : Invokable {
            public new static string Key = "phase.enter";
            // Payload `phase`
            public GamePhase phase;
            public ENTER(
                GS gameState,
                // Payload `phase`,
                GamePhase phase
            ) : base(gameState, ENTER.Key) {
                // Payload `phase`
                this.phase = phase;
            }
        }
        public class EXIT : Invokable {
            public new static string Key = "phase.exit";
            // Payload `phase`
            public GamePhase phase;
            public EXIT(
                GS gameState,
                // Payload `phase`,
                GamePhase phase
            ) : base(gameState, EXIT.Key) {
                // Payload `phase`
                this.phase = phase;
            }
        }
    }
    public class PLAYER {
        public class DAMAGED : Invokable {
            public new static string Key = "player.damaged";
            // Payload `damage`
            public int damageAmount;
            // Payload `player`
            public Player player;
            public DAMAGED(
                GS gameState,
                // Payload `damage`,
                int damageAmount,
                // Payload `player`,
                Player player
            ) : base(gameState, DAMAGED.Key) {
                // Payload `damage`
                this.damageAmount = damageAmount;
                // Payload `player`
                this.player = player;
            }
        }
        public class DIED : Invokable {
            public new static string Key = "player.died";
            // Payload `player`
            public Player player;
            public DIED(
                GS gameState,
                // Payload `player`,
                Player player
            ) : base(gameState, DIED.Key) {
                // Payload `player`
                this.player = player;
            }
        }
    }
}