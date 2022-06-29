using System;
using System.IO;

namespace Easy2D.Tests;

[TestClass]
public class UnitTest1
{
    private static string resourcesDirectory = @"../../../Resources";

    [TestMethod]
    public void test_ImportDotOsuFile()
    {
        // Read beatmap from resources folder

        

        using (var stream = File.OpenRead(resourcesDirectory + "/testmap.osu"))
        {
            
        }

    }
}