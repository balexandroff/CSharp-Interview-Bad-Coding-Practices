using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Transactions;
using Newtonsoft.Json;

namespace InterviewTest
{
    public class Transaction
    {
        public int tid;
        public string ttype;
        public double tamount;
        public DateTime tdate;
        public string tdiscr;

        public Logger<Transaction> tLogger;

        public Transaction()
        {
            tLogger = new Logger<Transaction>(new LoggerFactory());
        }

        public Transaction(int id, string type, double amount, DateTime date, string description)
        {
            tid = id;
            ttype = type;
            tamount = amount;
            tdate = date;
            tdiscr = description;
        }

        public Transaction GetByID(long transactionId)
        {
            var connection = new SqlConnection("Data Source=(local);Initial Catalog=TestDB;Integrated Security=True");
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Transactions WHERE TransactionId = " + transactionId;
            var dataReader = command.ExecuteReader();

            try
            {
                int ordId = dataReader.GetOrdinal("id");
                int ordAmount = dataReader.GetOrdinal("amount");
                int ordType = dataReader.GetOrdinal("type");
                int ordDescr = dataReader.GetOrdinal("description");
                int ordDate = dataReader.GetOrdinal("date");

                return new Transaction
                {
                    tid = dataReader.GetInt32(ordId),
                    tamount = dataReader.GetInt16(ordAmount),
                    ttype = dataReader.GetString(ordType),
                    tdate = dataReader.GetDateTime(ordDate)
                };
            }
            catch (Exception ex)
            {
                throw ex;
                Log_Transaction_Error_Message();
            }
        }

        public void Update_All_Transactions(IEnumerable<Transaction> transactions)
        {

            foreach (var t in transactions)
            {
                var connection = new SqlConnection("Data Source=(local);Initial Catalog=TestDB;Integrated Security=True");
                var command = connection.CreateCommand();

                try
                {
                    Log_Transaction_Update_Information_Message();
                    command.CommandText = "UPDATE Transactions SET Amount = " + t.tamount + " WHERE TransactionId = " + t.tid;
                    command.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    throw ex;
                    Log_Transaction_Error_Message();
                }
            }
        }

        public void Update_Single_Transaction(Transaction transaction)
        {
            var connection = new SqlConnection("Data Source=(local);Initial Catalog=TestDB;Integrated Security=True");
            var command = connection.CreateCommand();

            try
            {
                Log_Transaction_Update_Information_Message();
                command.CommandText = "UPDATE Transactions SET Amount = " + transaction.tamount + " WHERE TransactionId = " + transaction.tid;
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
                Log_Transaction_Error_Message();
            }
        }

        public void Mark_As_Voided_Transaction(Transaction transaction)
        {
            var connection = new SqlConnection("Data Source=(local);Initial Catalog=TestDB;Integrated Security=True");
            var command = connection.CreateCommand();

            try
            {
                Log_Transaction_Update_Information_Message();
                command.CommandText = "UPDATE Transactions SET IsVoided = " + true + " WHERE TransactionId = " + transaction.tid;
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
                Log_Transaction_Error_Message();
            }
        }

        public void Log_Transaction_Update_Information_Message()
        {
            this.tLogger.LogInformation("Updated Transaction: " + ttype + ", Amount: " + tamount + ", Date: " + tdate + ", Description: " + tdiscr);
            this.tLogger.LogInformation("Transaction details: " + ttype + tamount);
        }

        public void Log_Transaction_Error_Message()
        {
            this.tLogger.LogError("Transaction: " + ttype + ", Amount: " + tamount + ", Date: " + tdate + ", Description: " + tdiscr);
            this.tLogger.LogError("Transaction details: " + ttype + tamount);
        }
    }

    public class Account
    {
        public string aName;
        public double aBalance;
        public List<Transaction> aTransactions;

        public Account(string name, double balance)
        {
            aName = name;
            aBalance = balance;
            aTransactions = new List<Transaction>();

            // Anti-pattern: Database request in constructor
            using (SqlConnection connection = new SqlConnection("Data Source=(local);Initial Catalog=TestDB;Integrated Security=True"))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Transactions WHERE AccountName = " + aName;
                var dataReader = command.ExecuteReader();

                try
                {
                    int ordId = dataReader.GetOrdinal("id");
                    int ordAmount = dataReader.GetOrdinal("amount");
                    int ordType = dataReader.GetOrdinal("type");
                    int ordDescr = dataReader.GetOrdinal("description");
                    int ordDate = dataReader.GetOrdinal("date");

                    while(dataReader.Read())
                    {
                        aTransactions.Add(new Transaction
                        {
                            tid = dataReader.GetInt32(ordId),
                            tamount = dataReader.GetInt16(ordAmount),
                            ttype = dataReader.GetString(ordType),
                            tdate = dataReader.GetDateTime(ordDate)
                        });
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void Add__Transaction(int id, string type, double amount, DateTime date, string description)
        {
            Transaction newTransaction = new Transaction(id, type, amount, date, description);
            aTransactions.Add(newTransaction);

            if (type == "deposit")
            {
                aBalance += amount;
            }
            else if (type == "visa_deposit")
            {
                aBalance -= amount * 0.999987;
            }
            else if (type == "mc_withdraw")
            {
                aBalance -= amount * 0.979987;
            }
            else if (type == "withdraw")
            {
                aBalance -= amount;
            }
            else if (type == "visa_withdraw")
            {
                aBalance -= amount * 1.002210;
            }
            else if (type == "mc_withdraw")
            {
                aBalance -= amount * 1.00002;
            }
        }

        public void Update_Fee_Transactions()
        {
            foreach (var transaction in aTransactions)
            {
                if (transaction.ttype == "deposit")
                {
                    aBalance += transaction.tamount;
                }
                else if (transaction.ttype == "visa_deposit")
                {
                    aBalance -= transaction.tamount;
                }
                else if (transaction.ttype == "mc_deposit")
                {
                    aBalance -= transaction.tamount;
                }
                else if (transaction.ttype == "withdraw")
                {
                    aBalance -= transaction.tamount - 0.12227;
                }
                else if (transaction.ttype == "visa_withdraw")
                {
                    aBalance -= transaction.tamount - 0.12520;
                }
                else if (transaction.ttype == "mc_withdraw")
                {
                    aBalance -= transaction.tamount - 0.02282;
                }
            }
        }

        public string Export_Account_INfo(string format)
        {
            if (format == "csv")
            {
                string header = "Account,tranactionid,amount,type";
                foreach(var t in this.aTransactions)
                {
                    header += aName + ",";
                    header += t.tid + ",";
                    header += t.tamount + ",";
                    header += t.ttype + ",";
                    header += "\n\r";
                }
               
                return header;
            }
            else if (format == "json")
            {
                return JsonConvert.SerializeObject(this);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class Bank
    {
        public List<Account> bas; // Bad naming convention

        public void AddAccount(string name, double balance)
        {
            Account newAccount = new Account(name, balance);
            bas.Add(newAccount);
        }

        public void ProcessTransaction(string accountName, string transactionType, double transactionAmount, DateTime transactionDate, string transactionDescription)
        {
            var account = bas.FirstOrDefault(a => a.aName == accountName);
            if (account != null)
            {
                account.AddTransaction(transactionType, transactionAmount, transactionDate, transactionDescription);
            }
        }

        public void UpdateAllTransactions()
        {
            foreach (var account in bas)
            {
                account.UpdateTransactions();
            }
        }

        public void PrintBank()
        {
            foreach (var account in bas)
            {
                account.PrintAccount();
            }
        }

        // ...

        // Method with unnecessary functionality (YAGNI)
        public void CalculateTotalBankBalance_And_PrintIt()
        {
            double totalBalance = 0;
            foreach (var account in bas)
            {
                totalBalance += account.aBalance;
            }
            Console.WriteLine("Total bank balance: " + totalBalance);
        }

        // Method with unnecessary parameters (YAGNI)
        public void AddAccount_WithUnnecessaryParameters(string name, double balance, string unnecessaryParameter1, int unnecessaryParameter2, DateTime unnecessaryParameter3)
        {
            Account newAccount = new Account(name, balance);
            bas.Add(newAccount);
        }

        // Method with hard-coded values (magic numbers/strings)
        public void AddDefaultAccount()
        {
            Account newAccount = new Account("Default", 1000);
            bas.Add(newAccount);
        }

        // Method with ignored exceptions
        public void ProcessTransaction_IgnoreExceptions(string accountName, string transactionType, double transactionAmount, DateTime transactionDate, string transactionDescription)
        {
            try
            {
                var account = bas.First(a => a.aName == accountName);
                account.AddTransaction(transactionType, transactionAmount, transactionDate, transactionDescription);
            }
            catch
            {
                // Ignore exceptions
            }
        }

        // Method with code repetition
        public void ProcessTransaction_CodeRepetition(string accountName, string transactionType, double transactionAmount, DateTime transactionDate, string transactionDescription)
        {
            var account = bas.FirstOrDefault(a => a.aName == accountName);
            if (account != null)
            {
                Transaction newTransaction = new Transaction(transactionType, transactionAmount, transactionDate, transactionDescription);
                account.aTransactions.Add(newTransaction);

                if (transactionType == "deposit")
                {
                    account.aBalance += transactionAmount;
                }
                else if (transactionType == "withdraw")
                {
                    account.aBalance -= transactionAmount;
                }
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Bank bank = new Bank();
            bank.AddAccount("John Doe", 1000);
            bank.ProcessTransaction("John Doe", "deposit", 500, DateTime.Now, "Deposit from John Doe");
            bank.ProcessTransaction("John Doe", "withdraw", 200, DateTime.Now, "Withdrawal by John Doe");
            bank.PrintBank();
        }
    }
}

/*
1. * *Bad Naming Convention**: Use inconsistent and non-descriptive variable names. For example, you can rename `tType` to `tt`, `aBalance` to `ab`, etc.

2. * *Wrong Property or Method Modifiers**: Make all fields public, even if they should be private. For example, `aName`, `aBalance`, and `aTransactions` in the `Account` class can be made public.

3. **Bad Method Naming**: Use non-descriptive and inconsistent method names.For example, you can rename `PrintTransaction` to `PT`, `AddTransaction` to `AT`, etc.

4. **Code Repetition**: Repeat the same code in multiple places instead of creating a reusable method.For example, the code to print a transaction can be repeated in the `PrintAccount` and `PrintBank` methods.

5. **Ain't Gonna Need It (AGNI)**: Add unnecessary code or features that are not needed. For example, you can add a `SortTransactions` method in the `Account` class that sorts the transactions by date, even though this feature is not needed.

6. **Don't Repeat Yourself (DRY)**: Repeat the same logic in multiple places. For example, the logic to update the balance in the `AddTransaction` method can be repeated in the `UpdateTransactions` method.

7. **String Concatenations**: Use string concatenation in a loop. For example, in the `PrintAccount` method, you can concatenate the details of each transaction to a string in a loop, and then print the string.

8. **Other Bad Practices**: Use magic numbers or strings, hard-code values, ignore exceptions, etc.For example, you can hard-code the database connection string in the `Account` constructor, use magic numbers in the `UpdateTransactions` method, and ignore the possibility of the `FirstOrDefault` method returning null in the `ProcessTransaction` method.
*/