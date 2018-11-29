public enum SourceType {
    Player,
    Rabbit,
    Vegetable,
}

public static class SourceTypeExtensions {

    public static string GetMapTag(this SourceType type) {
        switch(type) {
            case SourceType.Player:
                return "PlayerMap";
            case SourceType.Rabbit:
                return "RabbitMap";
            case SourceType.Vegetable:
                return "VegetableMap";
            default:
                return null;
        }
    }
}