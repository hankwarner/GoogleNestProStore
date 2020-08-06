using Xunit;

namespace NestProHelpers.Tests
{
    public class NestProHelperUnitTests
    {
        [Fact]
        public void WillSetShippingMethodToSecondDayAirEarly()
        {
            string shippingMethod = "Priority";
            string expectedShippingMethodName = "ups next day air early a.m.";

            string shippingMethodName = BigCommerceHelper.GetShippingMethodName(shippingMethod);

            Assert.Equal(expectedShippingMethodName, shippingMethodName);
        }


        [Fact]
        public void WillSetShippingMethodToUpsGround()
        {
            string shippingMethod = "Free Shipping";
            string expectedShippingMethodName = "ups ground";

            string shippingMethodName = BigCommerceHelper.GetShippingMethodName(shippingMethod);

            Assert.Equal(expectedShippingMethodName, shippingMethodName);
        }


        [Fact]
        public void WillReturnStateAbbreviation()
        {
            string stateName = "District of Columbia";

            string stateAbbreviation = NetSuiteHelper.GetStateByName(stateName);

            Assert.Equal("DC", stateAbbreviation);
        }
    }
}
