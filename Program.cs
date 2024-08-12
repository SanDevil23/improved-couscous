using System;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration.Attributes;


class Program
{
    public class Statement
    {
        public string Date { get; set; }
        public string Transaction { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Currency { get; set; }
        public string CardName { get; set; }
        public string TransactionType { get; set; }
        public string Location { get; set; }
    }


    public static void Main()
    {
        string filePath =
        @"D:\coding\OneBanc\OneBancTask\HDFC-Input-Case1.csv";
        StreamReader reader = null;



        if (File.Exists(filePath))
        {

            reader = new StreamReader(File.OpenRead(filePath));
            List<string> data = new List<string>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == null) continue;
                string[] values = line.Split('\n');
                foreach (var item in values)
                {
                    data.Add(item);
                }

            }
            processData(data);
            sortDate();
            return;
        }
        else
        {
            Console.WriteLine("File doesn't exist");
        }
        Console.ReadLine();
    }


    private static void processData(List<string> data)
    {

        string filePath = @"D:\coding\OneBanc\OneBancTask\output.csv";
        StringBuilder csv = new StringBuilder();
        string header = "Date, Transaction Description, Debit, Credit, Currency, CardName, Transaction, Location";
        csv.AppendLine(header);

        string transType = "";
        string user = "";
        foreach (var i in data)
        {
            string credit = "0";
            string debit = "0";
            string city = "Delhi";
            string currency = "INR";

            String[] item = i.Split(',');
            string date = item[0];
            string transaction = item[1];
            string amt = item[2];

            // biffurcation 1
            if (date == "" && amt == "" && transaction == "")
            {
                continue;
            }

            // biffurcation 2
            if (String.Equals(date.Trim(), "Date") && String.Equals(transaction.Trim(), "Transaction Description") && String.Equals(amt.Trim(), "Amount"))
            {
                Console.WriteLine("I am inside the condition");
                continue;
            }
            // biffurcation 3
            if (String.Equals(date, "") && String.Equals(amt, ""))
            {
                if (String.Equals(transaction.Trim(), "Domestic Transactions")) transType = "Domestic";
                else if (String.Equals(transaction.Trim(), "International Transactions")) transType = "International";
                else user = transaction;
                continue;
            }

            // handling amount : credits & debits
            bool amount = getAmount(amt);
            {
                if (amount == true)
                {
                    credit = amt.Substring(0, amt.Length - 3);
                    debit = "0";
                }
                else
                {
                    debit = amt;
                    credit = "0";
                }
            }


            // EXTRACTING CITY AND CURRENCY DATA
            if (String.Equals(transType, "Domestic"))
            {
                string pattern = @"\s+";
                string[] vals = Regex.Split(transaction.Trim(), pattern);
                city = vals[vals.Length - 1].ToLower();
            }
            if (transType == "International")
            {
                string pattern = @"\s+";
                string[] vals = Regex.Split(transaction, pattern);
                currency = vals[vals.Length - 1];
                city = vals[vals.Length - 2].ToLower();
            }

            Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", date, transaction, debit, credit, currency, user, transType, city);
            string newLine = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", date, transaction.Trim(), debit, credit, currency, user, transType, city);

            csv.AppendLine(newLine);

        }

        File.WriteAllText("D:\\coding\\OneBanc\\OneBancTask\\output.csv", csv.ToString());
    }

    private static bool getAmount(string amt)
    {
        amt = amt.Trim();
        if (amt.Length < 2)
        {
            return false;
        }

        string lastTwoChars = amt.Substring(amt.Length - 2);

        if (lastTwoChars == "cr")
        {
            return true;
        }
        return false;
    }


    private static void sortDate()
    {
        // Date Sorting Function
        // Quick Sort

        var lines = File.ReadAllLines("D:\\coding\\OneBanc\\OneBancTask\\output.csv");

        if (lines.Length == 0)
        {
            Console.WriteLine("The file is empty.");
            return;
        }

        var header = lines[0];
        var dataLines = new List<string>(lines[1..]);

        var dateLinePairs = new List<(DateTime Date, string Line)>();
        var dateFormats = new[] { "dd-MM-yyyy", "yyyy-MM-dd" };

        foreach (var line in dataLines)
        {
            var columns = line.Split(',');
            if (DateTime.TryParseExact(columns[0], dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                dateLinePairs.Add((date, line));
            }
            else
            {
                Console.WriteLine($"Invalid date format in line: {line}");
            }

        }

        QuickSort(dateLinePairs, 0, dateLinePairs.Count - 1);

        using (var writer = new StreamWriter("D:\\coding\\OneBanc\\OneBancTask\\output.csv"))
        {
            writer.WriteLine(header);
            foreach (var pair in dateLinePairs)
            {
                writer.WriteLine(pair.Line);
                Console.WriteLine("Sorted and added");
            }
        }

        Console.WriteLine("CSV file sorted and saved.");
    }

    private static void QuickSort(List<(DateTime Date, string Line)> list, int low, int high)
    {
        if (low < high)
        {
            int pivotIndex = Partition(list, low, high);
            QuickSort(list, low, pivotIndex - 1);
            QuickSort(list, pivotIndex + 1, high);
        }
    }

    private static int Partition(List<(DateTime Date, string Line)> list, int low, int high)
    {
        var pivot = list[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (list[j].Date.CompareTo(pivot.Date) < 0)
            {
                i++;
                Swap(list, i, j);
            }
        }

        Swap(list, i + 1, high);
        return i + 1;
    }

    private static void Swap(List<(DateTime Date, string Line)> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}
