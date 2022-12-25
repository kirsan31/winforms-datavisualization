namespace DesignerTest;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        class1BindingSource.DataSource = new List<class1> { new class1 { }, new class1 { X = 2, Y1 = 3, Y2 = 4 } };
    }
}

public class class1
{
    public class1()
    {
    }

    public int X { get; set; } = 1;
    public int Y1 { get; set; } = 2;
    public int Y2 { get; set; } = 3;
}
