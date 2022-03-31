using System;
using System.Windows.Forms.DataVisualization.Charting;

using Xunit;

namespace Tests;
public class ChartNamedElementCollection_Tests
{    
    [Theory]
    [InlineData(1, new string[] { })]
    [InlineData(1, new string[] { "test" })]
    [InlineData(1, new string[] { "test", "tes2" })]
    [InlineData(1, new string[] { "test", "10", "20", "3", "4", "5" })]    
    public void Insert_Test(int fakeParam, string [] names)
    {
        NamedClass ncl = new NamedClass(null);
        foreach (var n in names)
        {
            ncl.Insert(0, new DataPointCustomProperties() { Name = n });

            for (int i = 0; i < ncl.Count; i++)
            {
                Assert.Equal(ncl.Count - i - 1, ncl.IndexOf(names[i]));
            }
        }
    }

    [Theory]
    [InlineData(1, new string[] { })]
    [InlineData(1, new string[] { "test" })]
    [InlineData(1, new string[] { "test", "tes2" })]
    [InlineData(1, new string[] { "test", "10", "20", "3", "4", "5" })]
    public void RemoveAt_Test(int fakeParam, string[] names)
    {
        NamedClass ncl = new NamedClass(null);

        foreach (var n in names)
            ncl.Insert(0, new DataPointCustomProperties() { Name = n });

        for (int i = 0; i < names.Length; i++)
        {
            ncl.RemoveAt(0);
            for (int j = 0; j < ncl.Count; j++)
                Assert.Equal(ncl.Count - j - 1, ncl.IndexOf(names[j]));
        }
    }

    [Theory]
    [InlineData(1, new string[] { })]
    [InlineData(1, new string[] { "test" })]
    [InlineData(1, new string[] { "test", "tes2" })]
    [InlineData(1, new string[] { "test", "10", "20", "3", "4", "5" })]
    public void Remove_Test(int fakeParam, string[] names)
    {
        NamedClass ncl = new NamedClass(null);

        foreach (var n in names)
            ncl.Insert(0, new DataPointCustomProperties() { Name = n });

        while(ncl.Count > 0)
        {
            ncl.Remove(ncl[0]);
            for (int j = 0; j < ncl.Count; j++)
                Assert.Equal(ncl.Count - j - 1, ncl.IndexOf(names[j]));
        }
    }

    [Theory]
    [InlineData(1, new string[] { })]
    [InlineData(1, new string[] { "test" })]
    [InlineData(1, new string[] { "test", "tes2" })]
    [InlineData(1, new string[] { "test", "10", "20", "3", "4", "5" })]
    public void Clear_Test(int fakeParam, string[] names)
    {
        NamedClass ncl = new NamedClass(null);

        foreach (var n in names)
            ncl.Insert(0, new DataPointCustomProperties() { Name = n });

        ncl.Clear();

        foreach (var n in names)
            Assert.Equal(-1, ncl.IndexOf(n));

    }

    [Theory]
    [InlineData(1, new string[] { })]
    [InlineData(1, new string[] { "test" })]
    [InlineData(1, new string[] { "test", "tes2" })]
    [InlineData(1, new string[] { "test", "10", "20", "3", "31", "5" })]
    public void Rename_Test(int fakeParam, string[] names)
    {
        NamedClass ncl = new NamedClass(null);

        foreach (var n in names)
            ncl.Add(new DataPointCustomProperties() { Name = n });

        foreach (var n in names)
        {
            var oldInd = ncl.IndexOf(n);
            if (ncl.IndexOf(n + 1) != -1)
            {
                Assert.Throws<ArgumentException>(() => ncl[n].Name = n + 1);
            }
            else
            {
                ncl[n].Name = n + 1;
                Assert.Equal(oldInd, ncl.IndexOf(n + 1));
            }
        }
    }
}

internal class NamedClass : ChartNamedElementCollection<DataPointCustomProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedClass`1"/> class.
    /// </summary>
    /// <param name="parent">The parent chart element.</param>
    internal NamedClass(System.Windows.Forms.DataVisualization.Charting.IChartElement? parent) : base(parent)
    {

    }
}