// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System;
using CptS321;
using NUnit.Framework.Constraints;

namespace NUnit.Tests1
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestCreateCellName()
        {
            //edge case
            string test;
            test = Spreadsheet.CreateCellName(49, 0);
            Assert.AreEqual("A50", test);

            //normal case
            test = Spreadsheet.CreateCellName(15, 3);
            Assert.AreEqual("D16", test);

            //exception case
            //test = SpreadsheetEngine.Spreadsheet.CreateCellName('a', "str");
            //Assert.Throws();
        }
        [Test]
        public void TestSetVariableName()
        {
            ExpressionTree test = new ExpressionTree("0");


            //normal case
            test.SetVariable("hello", 5);
            Assert.AreEqual(5, test.Vars["hello"]);

            //edge case, should update to 25
            test.SetVariable("hello", 25);
            Assert.AreEqual(25, test.Vars["hello"]);

            //exception case should not except a character for a variable name
            //test.SetVariable('A', 56);
            //Assert.Throws();
        }
        [Test]
        public void TestEvaluate()
        {
            //normal case
            double result;
            ExpressionTree test = new ExpressionTree("13+4+20");
            result = test.Evaluate();
            Assert.That(result, Is.EqualTo(37));


            //edge case
            test = new ExpressionTree("12345678-12345677");
            result = test.Evaluate();
            Assert.That(result, Is.EqualTo(1));

            //tests with parenthesis
            test = new ExpressionTree("(2*3)+(3*4)");
            result = test.Evaluate();
            Assert.That(result, Is.EqualTo(18));

            //three operators
            test = new ExpressionTree("((12+16)/7)+(3+3)");
            result = test.Evaluate();
            Assert.That(result, Is.EqualTo(10));

            //all four operators
            test = new ExpressionTree("((12+16)/7)+(3+3)*((50-5)/5)");
            result = test.Evaluate();
            Assert.That(result, Is.EqualTo(58));

            //exception case
            //test = new ExpressionTree("4294967296 + 1");
            //ActualValueDelegate<object> testDelegate = () => test.Evaluate();

            //Assert
            //Assert.That(testDelegate, Throws.TypeOf<FormatException>());
        }

        [Test]
        public void TestSpreadSheet()
        {
            Spreadsheet test = new Spreadsheet(5, 5);
            SpreadSheetCell A1 = (SpreadSheetCell)test.GetCell(0, 0);
            SpreadSheetCell A2 = (SpreadSheetCell)test.GetCell(1, 0);
            SpreadSheetCell A3 = (SpreadSheetCell)test.GetCell(2, 0);
            SpreadSheetCell B1 = (SpreadSheetCell)test.GetCell(0, 1);
            SpreadSheetCell B2 = (SpreadSheetCell)test.GetCell(1, 1);
            A1.Text = "15";
            Assert.AreEqual(A1.Text, A1.Value);
            A2.Text = "456";
            Assert.AreEqual(A2.Text, A2.Value);
            B1.Text = "=A1";
            Assert.AreEqual(B1.Value, A1.Value);
            B2.Text = "=B1+A2";
            Assert.AreEqual("471", B2.Value);
            A3.Text = "=A1*3";
            Assert.AreEqual("45", A3.Value);
        }
    }
}

