﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MyPrivateFinance.Models
{
    public static class DataAccess
    {
        static SqlConnection con;
        private static void OpenConnection()
        {
            try
            {
                string conString = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
                con = new SqlConnection(conString);
                con.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private static void CheckConnection()
        {
            if (con == null || con.State != System.Data.ConnectionState.Open)
            {
                OpenConnection();
            }
        }

        private static SqlDataReader GetDataReader(string query)
        {
            CheckConnection();
            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataReader rdr = cmd.ExecuteReader();
            return rdr;
        }

        public static List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();
            SqlDataReader rdr = GetDataReader("SELECT * FROM Categories");
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Category c = new Category();
                    c.Id = rdr.GetInt32(0);
                    c.Name = rdr[1].ToString();
                    categories.Add(c);
                }
            }
            rdr.Close();
            return categories;
        }

        public static List<Payment> GetPayments()
        {
            List<Payment> payments = new List<Payment>();
            List<Category> categories = GetCategories();
            SqlDataReader rdr = GetDataReader("SELECT * FROM Payments");
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Payment p = new Payment();
                    p.Id = rdr.GetInt32(0);
                    p.Description = rdr[1].ToString();
                    p.Date = rdr.GetDateTime(2);
                    p.Amount = rdr.GetDouble(3);
                    p.IsIncome = rdr.GetBoolean(4);
                    p.Category = categories.Find(c => c.Id == rdr.GetInt32(5));
                    payments.Add(p);
                }
            }
            rdr.Close();
            return payments;
        }

        public static void AddPayment(Payment p)
        {
            CheckConnection();
            string query = "INSERT Payments Values(@D, @Da, @A, @I, @C)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@D", p.Description);
            cmd.Parameters.AddWithValue("@A", p.Amount);
            cmd.Parameters.AddWithValue("@Da", p.Date);
            cmd.Parameters.AddWithValue("@I", p.IsIncome);
            cmd.Parameters.AddWithValue("@C", p.Category.Id);
            cmd.ExecuteNonQuery();
        }

        public static void UpdatePayment(Payment p)
        {
            CheckConnection();
            string query = "UPDATE Payments SET Description=@D, Date=@Da, Amount=@A, IsIncome=@I, CategoryId=@C WHERE Id=@Id";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", p.Id);
            cmd.Parameters.AddWithValue("@D", p.Description);
            cmd.Parameters.AddWithValue("@A", p.Amount);
            cmd.Parameters.AddWithValue("@Da", p.Date);
            cmd.Parameters.AddWithValue("@I", p.IsIncome);
            cmd.Parameters.AddWithValue("@C", p.Category.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeletePayment(Payment p)
        {
            try
            {
                CheckConnection();
                string query = "DELETE Payments WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void AddCategory(Category c)
        {
            try
            {
                CheckConnection();
                string query = "INSERT Categories Values(@N)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@N", c.Name);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public static void DeleteCategory(Category c)
        {
            try
            {
                CheckConnection();
                string query = "DELETE Categories WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", c.Id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static double GetPaymentSum()
        {
            double sum = 0.00;
            List<Payment> payments = GetPayments();
            foreach (var p in payments)
            {
                if (p.IsIncome)
                {
                    sum += p.Amount;
                }
                else
                {
                    sum -= p.Amount;
                }
            }
            return sum;
        }

        public static List<Payment> GetPaymentsByMothYear(DateTime dt)
        {
            CheckConnection();
            List<Payment> payments = new List<Payment>();
            List<Category> categories = new List<Category>();
            string query = "SELECT * FROM Payments WHERE MONTH(Date)=MONTH(@M) AND YEAR(Date)=YEAR(@M)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@M", dt);
            SqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Payment p = new Payment()
                    {
                        Id = rdr.GetInt32(0),
                        Description = rdr[1].ToString(),
                        Date = rdr.GetDateTime(2),
                        Amount = rdr.GetDouble(3),
                        IsIncome = rdr.GetBoolean(4),
                        Category = categories.Find(c => c.Id == rdr.GetInt32(5))
                    };
                    payments.Add(p);
                }
            }
            rdr.Close();
            return payments;
        }
    }
}
