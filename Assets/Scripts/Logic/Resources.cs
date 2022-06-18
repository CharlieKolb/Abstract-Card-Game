public class Resources {
    public int red = 0;
    public int green = 0;
    public int blue = 0;

    public Resources() {}
    public Resources(int r, int g, int b) {
        red = r;
        green = g;
        blue = b;
    }
    public Resources(Resources other) {
        red = other.red;
        green = other.green;
        blue = other.blue;
    }

    public bool canBePaid(Resources available) {
        return this.red <= available.red &&
            this.green <= available.green &&
            this.blue <= available.blue;
    }
    public Resources Without(Resources sub) {
        return new Resources(
            this.red - sub.red,
            this.green - sub.green,
            this.blue - sub.blue
        );
    }

    public static Resources FromRed(int val) {
        var res = new Resources();
        res.red = val;
        return res;
    }
    public Resources WithRed(int val) {
        this.red = val;
        return this;
    }

    public static Resources FromGreen(int val) {
        var res = new Resources();
        res.green = val;
        return res;
    }
    public Resources WithGreen(int val) {
        this.green = val;
        return this;
    }
    
    public static Resources FromBlue(int val) {
        var res = new Resources();
        res.blue = val;
        return res;
    }
    public Resources WithBlue(int val) {
        this.blue = val;
        return this;
    }
}