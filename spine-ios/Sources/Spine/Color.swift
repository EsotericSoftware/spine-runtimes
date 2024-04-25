/// A color made of red, green, blue, and alpha components,
/// ranging from 0-1.
public final class Color {
    public let r: Double
    public let g: Double
    public let b: Double
    public let a: Double
    
    public init(r: Double, g: Double, b: Double, a: Double) {
        self.r = r
        self.g = g
        self.b = b
        self.a = a
    }
}
