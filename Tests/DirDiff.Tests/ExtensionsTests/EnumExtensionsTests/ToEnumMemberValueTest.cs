using DirDiff.Extensions;
using System.Runtime.Serialization;

namespace DirDiff.Tests.ExtensionsTests.EnumExtensionsTests;

public class ToEnumMemberValueTest
{
    [Fact]
    public void Gets_Enum_Member_Values()
    {
        TestValue.Value1.ToEnumMemberValue().ShouldBe(Value1Value);
        TestValue.Value2.ToEnumMemberValue().ShouldBe(Value2Value);
        TestValue.Value3.ToEnumMemberValue().ShouldBe("Value3");
        TestValue.Value4.ToEnumMemberValue().ShouldBe(Value4Value);
    }

    [Fact]
    public void Gets_Enum_Member_Values_Required()
    {
        TestValue.Value1.ToEnumMemberValue(required: true).ShouldBe(Value1Value);

        Should.Throw<ArgumentException>(() => TestValue.Value3.ToEnumMemberValue(required: true));
    }

    private const string Value1Value = "Value One";
    private const string Value2Value = "Value Two";
    private const string Value4Value = "Value Four";

    private enum TestValue
    {
        [EnumMember(Value = Value1Value)]
        Value1,
        [EnumMember(Value = Value2Value)]
        Value2,
        Value3,
        [EnumMember(Value = Value4Value)]
        Value4,
    }
}
