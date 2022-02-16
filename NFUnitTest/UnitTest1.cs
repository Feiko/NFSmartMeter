using nanoFramework.TestFramework;
using System;

namespace NFUnitTest
{
    [TestClass]
    public class Test1
    {
        public const string TestDataRaw = @"/XMX5LGF0000453094270

1-3:0.2.8(50)
0-0:1.0.0(210304120347W)
0-0:96.1.1(4530303531303035333039343237303139)
1-0:1.8.1(001819.387*kWh)
1-0:1.8.2(002093.302*kWh)
1-0:2.8.1(000088.650*kWh)
1-0:2.8.2(000157.206*kWh)
0-0:96.14.0(0002)
1-0:1.7.0(00.288*kW)
1-0:2.7.0(00.000*kW)
0-0:96.7.21(00015)
0-0:96.7.9(00002)
1-0:99.97.0(1)(0-0:96.7.19)(190226161118W)(0000000541*s)
1-0:32.32.0(00019)
1-0:32.36.0(00002)
0-0:96.13.0()
1-0:32.7.0(231.0*V)
1-0:31.7.0(001*A)
1-0:21.7.0(00.288*kW)
1-0:22.7.0(00.000*kW)
0-1:24.1.0(003)
0-1:96.1.0(4730303339303031393231393034393139)
0-1:24.2.1(210304120005W)(01980.598*m3)
!894F 
";


        string testMessagestring = @"/XMX5LGF0000453094270

1-3:0.2.8(50)
0-0:1.0.0(210123192453W)
0-0:96.1.1(4530303531303035333039343237303139) 
1-0:1.8.1(001674.508*kWh)
1-0:1.8.2(001904.526*kWh)
1-0:2.8.1(000087.040*kWh)
1-0:2.8.2(000156.037*kWh)
0-0:96.14.0(0001)
1-0:1.7.0(00.336*kW)
1-0:2.7.0(00.000*kW)
0-0:96.7.21(00015)
0-0:96.7.9(00002)
1-0:99.97.0(1)(0-0:96.7.19)(190226161118W)(0000000541*s)
1-0:32.32.0(00019)
1-0:32.36.0(00002)
0-0:96.13.0()
1-0:32.7.0(228.3*V)
1-0:31.7.0(001*A)
1-0:21.7.0(00.337*kW)
1-0:22.7.0(00.000*kW)
0-1:24.1.0(003)
0-1:96.1.0(4730303339303031393231393034393139)
0-1:24.2.1(210123192003W)(01688.519*m3)
!";


        [TestMethod]
        public void TestMethod1()
        {

            var model = NFSmartMeter.P1MessageDecoder.DecodeData(testMessagestring);
            Assert.Equal(model.MeterId, "XMX5LGF0000453094270");

        }
    }
}
