namespace Klarity.Test;

// 1. Violation: Class name not PascalCase
class lowerCaseClass 
{
    public void BadMethod()
    {
        // 2. Violation: Empty catch block
        try {
            int x = 0;
        } catch (System.Exception) {
            // Error!
        }

        // 3. Violation: String concatenation in loop
        string s = "";
        for (int i = 0; i < 10; i++)
        {
            s += i.ToString(); 
        }

        // 4. Violation: Deep nesting
        if (true) {
            if (true) {
                if (true) {
                    if (true) {
                        System.Console.WriteLine("Too deep!");
                    }
                }
            }
        }
    }

    // 5. Violation: Long method
    public void VeryLongMethod()
    {
        System.Console.WriteLine("Line 1");
        System.Console.WriteLine("Line 2");
        System.Console.WriteLine("Line 3");
        System.Console.WriteLine("Line 4");
        System.Console.WriteLine("Line 5");
        System.Console.WriteLine("Line 6");
        System.Console.WriteLine("Line 7");
        System.Console.WriteLine("Line 8");
        System.Console.WriteLine("Line 9");
        System.Console.WriteLine("Line 10");
        System.Console.WriteLine("Line 11");
        System.Console.WriteLine("Line 12");
        System.Console.WriteLine("Line 13");
        System.Console.WriteLine("Line 14");
        System.Console.WriteLine("Line 15");
        System.Console.WriteLine("Line 16");
        System.Console.WriteLine("Line 17");
        System.Console.WriteLine("Line 18");
        System.Console.WriteLine("Line 19");
        System.Console.WriteLine("Line 20");
        System.Console.WriteLine("Line 21");
        System.Console.WriteLine("Line 22");
        System.Console.WriteLine("Line 23");
        System.Console.WriteLine("Line 24");
        System.Console.WriteLine("Line 25");
        System.Console.WriteLine("Line 26");
        System.Console.WriteLine("Line 27");
        System.Console.WriteLine("Line 28");
        System.Console.WriteLine("Line 29");
        System.Console.WriteLine("Line 30");
        System.Console.WriteLine("Line 31");
        System.Console.WriteLine("Line 32");
        System.Console.WriteLine("Line 33");
        System.Console.WriteLine("Line 34");
        System.Console.WriteLine("Line 35");
        System.Console.WriteLine("Line 36");
        System.Console.WriteLine("Line 37");
        System.Console.WriteLine("Line 38");
        System.Console.WriteLine("Line 39");
        System.Console.WriteLine("Line 40");
        System.Console.WriteLine("Line 41");
    }
}
