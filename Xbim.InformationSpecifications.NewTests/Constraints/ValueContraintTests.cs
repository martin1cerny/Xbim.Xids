﻿using FluentAssertions;
using System;
using Xunit;

namespace Xbim.InformationSpecifications.Tests
{

    public class ValueContraintTests
    {
        [Fact]
        public void xbimIfcElementsHandled()
        {
            var id = "2O2Fr$t4X7Zf8NOew3FLOH";
            Ifc2x3.UtilityResource.IfcGloballyUniqueId ifcGuid = new(id);
            var vc = new ValueConstraint(id); // this is a string constraint
            vc.IsSatisfiedBy(ifcGuid).Should().BeTrue("string == IfcGuid");

            var undefinedString = ValueConstraint.SingleUndefinedExact(id);
            undefinedString.IsSatisfiedBy(ifcGuid).Should().BeTrue("object == IfcGuid");

            var differentId = "2O2Fr$t4X7Zf8NOew3FLOh";    // different due to lower-case 'h'
            var anotherString = new ValueConstraint(differentId);
            anotherString.IsSatisfiedBy(ifcGuid).Should().BeFalse("IfcGuids are different");
        }

        [Fact]
        public void ExactContraintSatisfactionTest()
        {
            var vc = new ValueConstraint(NetTypeName.String, "2");
            vc.IsSatisfiedBy("2").Should().BeTrue();
            vc.IsSatisfiedBy("1").Should().BeFalse();
            vc.IsSatisfiedBy(1).Should().BeFalse();
            vc.IsSatisfiedBy(2).Should().BeTrue(); // conversion ToString is valid match

            vc = new ValueConstraint(375.230);
            vc.IsSatisfiedBy(375.23).Should().BeTrue();
            vc.IsSatisfiedBy(375.230000).Should().BeTrue();
            vc.IsSatisfiedBy(375.230001).Should().BeTrue();     // within 1e-6 tolerance
            vc.IsSatisfiedBy(375.229999).Should().BeTrue();     // ""
            vc.IsSatisfiedBy(375.230500).Should().BeFalse();    // not within 1e-6 tolerance
            vc.IsSatisfiedBy(375.229500).Should().BeFalse();    // ""

            vc = new ValueConstraint(375.230m);
            vc.IsSatisfiedBy(375.230m).Should().BeTrue();
            vc.IsSatisfiedBy(375.23m).Should().BeFalse();


            vc = new ValueConstraint("red");
            vc.IsSatisfiedBy("red").Should().BeTrue();
            vc.IsSatisfiedBy("blue").Should().BeFalse();

            vc = new ValueConstraint(NetTypeName.Floating);
            vc.IsSatisfiedBy(2f).Should().BeTrue();
            vc.IsSatisfiedBy("blue").Should().BeFalse();
            vc.IsSatisfiedBy(2d).Should().BeFalse();
        }

        [Fact]
        public void BuildingSmartConstraintValues()
        {
            // in case where the type is not defined,
            // we cast the value of the string to the type provided to see if they match
            //
            var vc = new ValueConstraint(NetTypeName.Undefined, "42.0");
            vc.IsSatisfiedBy(42d).Should().BeTrue();
        }

        [Fact]
        public void ConstraintFromBasicString()
        {
            ValueConstraint vc = "Some";
            vc.IsSatisfiedBy("Some").Should().BeTrue();
            vc.IsSatisfiedBy("SomeOther").Should().BeFalse();

            vc.IsEmpty().Should().BeFalse();
            vc.IsSingleExact(out var _).Should().BeTrue();
        }

        [Fact]
        public void ConstraintWithSpecificallyFormattedNumbers()
        {
            // from IDS test attributes/fail-only_specifically_formatted_numbers_are_allowed_2_4
            ValueConstraint vc = "123,4.5";
            vc.IsSatisfiedBy("1234.5").Should().BeFalse();
            vc.IsSatisfiedBy(1234.5d).Should().BeFalse();
        }

        [Fact]
        public void ConstraintWithSpecificallyFormattedNumbers2()
        {
            // from IDS test attributes/fail-only_specifically_formatted_numbers_are_allowed_2_4
            ValueConstraint vc = "43,3";
            vc.IsSatisfiedBy("42.3").Should().BeFalse();
            vc.IsSatisfiedBy(42.3d).Should().BeFalse();
        }

        [Fact]
        public void LongValueTypeIsInferred()
        {
            // from IDS test    property\pass-integer_values_are_checked_using_type_casting_1_4.ifc
            // and              attribute\pass-integers_follow_the_same_rules_as_numbers.ids
            ValueConstraint vc = new ValueConstraint(42);
            vc.BaseType = NetTypeName.Undefined;    // E.g. on Attribute values we won't have a BaseType on a simpleValue constraint
            vc.IsSatisfiedBy(42L).Should().BeTrue("42==42");
        }

        [Fact]
        public void IntValueTypeIsInferred()
        {
            ValueConstraint vc = new ValueConstraint(42);
            vc.BaseType = NetTypeName.Undefined;
            vc.IsSatisfiedBy(42).Should().BeTrue("42==42");

        }

        [Fact]
        public void DecimalValueTypeIsInferred()
        {
            ValueConstraint vc = new ValueConstraint(42m);
            vc.BaseType = NetTypeName.Undefined;
            vc.IsSatisfiedBy(42m).Should().BeTrue("42m==42m");
        }

        [Fact]
        public void FloatValueTypeIsInferred()
        {
            ValueConstraint vc = new ValueConstraint(42f);
            vc.BaseType = NetTypeName.Undefined;
            vc.IsSatisfiedBy(42d).Should().BeTrue("42f==42d");

        }

        [Fact]
        public void DoubleValueTypeIsInferred()
        {
            ValueConstraint vc = new ValueConstraint(42d);
            vc.BaseType = NetTypeName.Undefined;
            vc.IsSatisfiedBy(42f).Should().BeTrue("42d==42f");

        }

        // For Completeness
        [Fact]
        public void TimeSpanValueTypeIsInferred()
        {
            ValueConstraint vc = new ValueConstraint(TimeSpan.FromDays(42).ToString());
            vc.BaseType = NetTypeName.Undefined;
            vc.IsSatisfiedBy(TimeSpan.FromDays(42)).Should().BeTrue("42==42");

        }

        [Fact]
        public void CaseSensitiviyTests()
        {
            ValueConstraint t2 = "ABC";
            t2.BaseType = NetTypeName.String;
            t2.IsSatisfiedBy("abc").Should().BeFalse();
            t2.IsSatisfiedIgnoringCaseBy("abc").Should().BeTrue();

            var t = ValueConstraint.CreatePattern("CDE");
            t.IsSatisfiedBy("cde").Should().BeFalse();
            t.IsSatisfiedIgnoringCaseBy("cde").Should().BeTrue();
        }


        [Fact]
        public void RangeConstraintSatisfactionTest()
        {
            var vc = new ValueConstraint(NetTypeName.Double);
            var t = new RangeConstraint()
            {
                MinValue = 2.ToString(),
                MinInclusive = true,
                MaxValue = 4.ToString(),
                MaxInclusive = true,
            };
            vc.AddAccepted(t);

            vc.IsSatisfiedBy(1d).Should().BeFalse();
            vc.IsSatisfiedBy(2d).Should().BeTrue();
            vc.IsSatisfiedBy(2.01).Should().BeTrue();
            vc.IsSatisfiedBy(3.99).Should().BeTrue();
            vc.IsSatisfiedBy(4d).Should().BeTrue();
            vc.IsSatisfiedBy(4.01).Should().BeFalse();

            t.MinInclusive = false;
            t.MaxInclusive = false;

            vc.IsSatisfiedBy(1d).Should().BeFalse();
            vc.IsSatisfiedBy(2d).Should().BeFalse();
            vc.IsSatisfiedBy(2.01d).Should().BeTrue();
            vc.IsSatisfiedBy(3.99d).Should().BeTrue();
            vc.IsSatisfiedBy(4d).Should().BeFalse();
            vc.IsSatisfiedBy(4.01d).Should().BeFalse();
        }

        [Fact]
        public void DecimalRangesAreInclusive()
        {
            var vc = new ValueConstraint(NetTypeName.Decimal);
            var t = new RangeConstraint()
            {
                MinValue = 42.ToString(),
                MinInclusive = true,
                MaxValue = 42.ToString(),
                MaxInclusive = true,
            };
            vc.AddAccepted(t);

            vc.IsSatisfiedBy(42.1).Should().BeFalse("40.1 failure");
            vc.IsSatisfiedBy(41.999999f).Should().BeFalse("41.9999 failure");
            vc.IsSatisfiedBy(42L).Should().BeTrue("42L failure");
            vc.IsSatisfiedBy(42M).Should().BeTrue("42M failure");
            vc.IsSatisfiedBy(42d).Should().BeTrue("42d failure");
        }

        [Fact]
        public void EnumConstraintSatisfactionTest()
        {
            var vc = new ValueConstraint(NetTypeName.Integer);
            vc.AddAccepted(new ExactConstraint(30.ToString()));
            vc.AddAccepted(new ExactConstraint(60.ToString()));
            vc.AddAccepted(new ExactConstraint(90.ToString()));

            vc.IsSatisfiedBy(1d).Should().BeFalse("1d failure");
            vc.IsSatisfiedBy(30L).Should().BeTrue("30L failure");
            vc.IsSatisfiedBy(60).Should().BeTrue("60 failure");
            vc.IsSatisfiedBy(60L).Should().BeTrue("60L failure");
        }

        
        [InlineData("text", "Text")]
        [InlineData("à rénover", "A RENOVER")]
        [InlineData("abîmer", "Abimer")]
        [InlineData("tårn", "Tarn")]

        [Theory]
        public void CanCompareCaseInsensitivelyIgnoringAccents(string input, string constraint)
        {

            ValueConstraint vc = constraint;
            vc.IsSatisfiedBy(input, true).Should().BeTrue();
            
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void DoubleValueSupports1e6Tolerances(bool setBaseType)
        {
            // from IDS test property/pass-floating_point_numbers_are_compared_with_a_1e_6_tolerance_1_*.ifc && attribute equivalents
            ValueConstraint vc = new ValueConstraint(42d);
            if (!setBaseType)
                vc.BaseType = NetTypeName.Undefined;
            
            vc.IsSatisfiedBy(42.000042d).Should().BeTrue("Within 1e-6 tolerances - high");
            vc.IsSatisfiedBy(41.999958d).Should().BeTrue("Within 1e-6 tolerances - low");

            vc.IsSatisfiedBy(42.000084d).Should().BeFalse("Outside 1e-6 tolerances - high");
            vc.IsSatisfiedBy(41.999916d).Should().BeFalse("Outside 1e-6 tolerances - low");
        }

    }
}
