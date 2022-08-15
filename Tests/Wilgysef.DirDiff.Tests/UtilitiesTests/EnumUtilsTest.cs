using Wilgysef.DirDiff.Utilities;
using System.Runtime.Serialization;

namespace Wilgysef.DirDiff.Tests.UtilitiesTests;

public class EnumUtilsTest
{
    [Fact]
    public void Parses_Enum_Member_Values()
    {
        EnumUtils.ParseEnumMemberValue<TestValue>(Value1Value).ShouldBe(TestValue.Value1);
        EnumUtils.ParseEnumMemberValue<TestValue>(Value2Value).ShouldBe(TestValue.Value2);
        EnumUtils.ParseEnumMemberValue<TestValue>(TestValue.Value3.ToString()).ShouldBe(TestValue.Value3);
        EnumUtils.ParseEnumMemberValue<TestValue>(Value4Value).ShouldBe(TestValue.Value4);

        EnumUtils.ParseEnumMemberValue<TestValue>(Value1Value.ToLower()).ShouldBe(TestValue.Value1);
        EnumUtils.ParseEnumMemberValue<TestValue>(Value2Value.ToLower()).ShouldBe(TestValue.Value2);
        EnumUtils.ParseEnumMemberValue<TestValue>(TestValue.Value3.ToString().ToLower()).ShouldBe(TestValue.Value3);
        EnumUtils.ParseEnumMemberValue<TestValue>(Value4Value.ToLower()).ShouldBe(TestValue.Value4);
    }

    [Fact]
    public void Parses_Enum_Member_Values_NoIgnoreCase()
    {
        EnumUtils.ParseEnumMemberValue<TestValue>(Value1Value, ignoreCase: false).ShouldBe(TestValue.Value1);

        Should.Throw<ArgumentException>(
            () => EnumUtils.ParseEnumMemberValue<TestValue>(Value2Value.ToLower(), ignoreCase: false));
    }

    [Fact]
    public void Parses_Enum_Member_Values_Required()
    {
        EnumUtils.ParseEnumMemberValue<TestValue>(Value1Value, enumMemberValueRequired: true).ShouldBe(TestValue.Value1);

        Should.Throw<ArgumentException>(
            () => EnumUtils.ParseEnumMemberValue<TestValue>(TestValue.Value3.ToString(), enumMemberValueRequired: true));
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
