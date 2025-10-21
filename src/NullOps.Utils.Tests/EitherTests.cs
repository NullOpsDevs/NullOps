namespace NullOps.Utils.Tests;

public class EitherTests
{
    [Test]
    public void Empty_constructor_fails_to_initialize()
    {
        var either = new Either<int, bool>();
        
        Assert.That(either.IsBottom, Is.True);
        Assert.That(either.IsLeft, Is.False);
        Assert.That(either.IsRight, Is.False);
        
        Assert.Throws<InvalidOperationException>(() => { _ = new Either<int, bool>().Left; });
        Assert.Throws<InvalidOperationException>(() => { _ = new Either<int, bool>().Right; });
    }

    [Test]
    public void Left_constructor_test()
    {
        var either = new Either<int, bool>(1);
        
        Assert.That(either.IsBottom, Is.False);
        Assert.That(either.IsLeft, Is.True);
        Assert.That(either.IsRight, Is.False);
        
        Assert.That(either.Left, Is.EqualTo(1));

        try
        {
            _ = either.Right; 
            Assert.Fail("Should throw");
        } catch (InvalidOperationException) { }
    }

    [Test]
    public void Right_constructor_test()
    {
        var either = new Either<int, bool>(true);
        
        Assert.That(either.IsBottom, Is.False);
        Assert.That(either.IsLeft, Is.False);
        Assert.That(either.IsRight, Is.True);
        
        Assert.That(either.Right, Is.EqualTo(true));
        
        try
        {
            _ = either.Left; 
            Assert.Fail("Should throw");
        } catch (InvalidOperationException) { }
    }
    
    [Test]
    public void Bottom_equality_test()
    {
        var bottom1 = new Either<int, bool>();
        var bottom2 = new Either<int, bool>();
    
        Assert.That(bottom1, Is.EqualTo(bottom2));
        Assert.That(bottom1 == bottom2, Is.True);
    }

    [Test]
    public void Left_equality_test()
    {
        var either1 = new Either<int, bool>(5);
        var either2 = new Either<int, bool>(5);
        var either3 = new Either<int, bool>(10);
    
        Assert.That(either1, Is.EqualTo(either2));
        Assert.That(either1, Is.Not.EqualTo(either3));
    }

    [Test]
    public void Left_and_Right_not_equal()
    {
        Either<int, int> left = new Either<int, int>(leftValue: 5);
        Either<int, int> right = new Either<int, int>(rightValue: 5);
    
        Assert.That(left, Is.Not.EqualTo(right));
    }

    [Test]
    public void Implicit_conversion_from_value()
    {
        Either<int, bool> fromLeft = 42;
        Either<int, bool> fromRight = true;
    
        Assert.That(fromLeft.IsLeft, Is.True);
        Assert.That(fromLeft.Left, Is.EqualTo(42));
        Assert.That(fromRight.IsRight, Is.True);
        Assert.That(fromRight.Right, Is.True);
    }
}