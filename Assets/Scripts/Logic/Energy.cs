public class Energy {
    public int red = 0;
    public int green = 0;
    public int blue = 0;

    public Energy() {}
    public Energy(int r, int g, int b) {
        red = r;
        green = g;
        blue = b;
    }
    public Energy(Energy other) {
        red = other.red;
        green = other.green;
        blue = other.blue;
    }

    public bool canBePaid(Energy available) {
        return this.red <= available.red &&
            this.green <= available.green &&
            this.blue <= available.blue;
    }
    public Energy Without(Energy sub) {
        return new Energy(
            this.red - sub.red,
            this.green - sub.green,
            this.blue - sub.blue
        );
    }

    public static Energy FromRed(int val) {
        var res = new Energy();
        res.red = val;
        return res;
    }
    public Energy WithRed(int val) {
        this.red = val;
        return this;
    }

    public static Energy FromGreen(int val) {
        var res = new Energy();
        res.green = val;
        return res;
    }
    public Energy WithGreen(int val) {
        this.green = val;
        return this;
    }
    
    public static Energy FromBlue(int val) {
        var res = new Energy();
        res.blue = val;
        return res;
    }
    public Energy WithBlue(int val) {
        this.blue = val;
        return this;
    }
}