using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Exchange.WebServices.Data;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace EWSconnector1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ExchangeService service = Connect_To_MSexchg();
                CalendarView cvCalendarView = Appointment_Filter();

                // Creating empty list for storing Appointments using our created date filter
                cvCalendarView.PropertySet = new PropertySet(BasePropertySet.FirstClassProperties);
                FindItemsResults<Appointment> fiApts = service.FindAppointments(WellKnownFolderName.Calendar, cvCalendarView);
                List<Appointment> filteredApt = new List<Appointment>();
                service.LoadPropertiesForItems(from Item item in fiApts select item, PropertySet.FirstClassProperties);

                SqlConnection conn = Connect_To_DB();
                conn.Open();

                foreach (Appointment apt in fiApts.Items)
                {
                    filteredApt.Add(apt); // Adding apointments to List 

                    Add_Meeting(apt, conn); // Calling method which stores appointments info in database

                    string ID_Appointment = ((apt as Appointment).Id).ToString();

                    var optional = (apt as Appointment).OptionalAttendees;
                    foreach (var item in optional)
                    {
                        Add_Meeting_Attendees(item.ToString(), conn, ID_Appointment, 0); // Calling method which stores info about optional attendee in database
                    }

                    var required = (apt as Appointment).RequiredAttendees;
                    foreach (var item in required)
                    {
                        Add_Meeting_Attendees(item.ToString(), conn, ID_Appointment, 1); // Calling method which stores info about required attendee in database
                    }
                }
                conn.Close();
                Console.WriteLine("Done");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Output_exceptions(ex);
            }

            
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        private static ExchangeService Connect_To_MSexchg()
        {
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
            //Credentials needed to connect MS exchange

            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013);
            service.Credentials = new WebCredentials("administrator@adform.local", "Operation1");
            service.AutodiscoverUrl("administrator@adform.local", RedirectionUrlValidationCallback);
            return service;
        }

        private static SqlConnection Connect_To_DB()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            return conn;
        }

        // Method for creating appointments filter by range date
        private static CalendarView Appointment_Filter()
        {
            DateTime date_from = new DateTime(2012, 1, 1);
            DateTime date_to = new DateTime(2013, 12, 31);
            //DateTime date_from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            //DateTime date_to = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
            CalendarView cvCalendarView = new CalendarView(date_from, date_to, 1000); // limits to first 1000 appointments in selected date range
            return cvCalendarView;
        }

        private static void Add_Meeting(Appointment apt, SqlConnection conn)
        {
            string ID_Meeting = ((apt as Appointment).Id).ToString();
            string Subject = (apt as Appointment).Subject;
            string Location = (apt as Appointment).Location;
            DateTime StartTime = (apt as Appointment).Start;
            DateTime EndTime = (apt as Appointment).End;
            string Organizer_info = (apt as Appointment).Organizer.ToString();

            //Parsing organizer username and email
            string Organizer__info_parse = Organizer_info.Replace("<SMTP:", "").Replace(">", "");
            string[] Username_email = Organizer__info_parse.Split(' ');

            try
            {
                SqlCommand organizer = new SqlCommand("IF NOT EXISTS (SELECT * FROM Users WHERE Username = '" + Username_email[0] + "' AND Email = '" + Username_email[1] + "') BEGIN INSERT INTO Users (Email, Username) VALUES (@Email, @Username) END", conn);
                organizer.Parameters.AddWithValue("@Email", Username_email[1]); //Username_email[1] contains email
                organizer.Parameters.AddWithValue("@Username", Username_email[0]); //Username_email[0] contains username
                organizer.ExecuteNonQuery();

                SqlCommand Add_Organizer_Meeting_Attenders_Table = new SqlCommand("IF NOT EXISTS (SELECT * FROM Meeting_attenders WHERE User_Username = '" + Username_email[0] + "'" + "AND ID_Meting = '" + ID_Meeting + "') BEGIN INSERT INTO Meeting_attenders (ID_Meting, User_Username, Is_Required, Have_Evaluated, Is_Organizer) VALUES ('" + ID_Meeting + "', '" + Username_email[0] + "', 1, 0, 1) END", conn);
                Add_Organizer_Meeting_Attenders_Table.ExecuteNonQuery();
				
                SqlCommand Organizer_ID = new SqlCommand("SELECT ID_Attender FROM Meeting_Attenders WHERE ID_Meting = '" + ID_Meeting + "' AND Is_Organizer = 1", conn);
                int result = ((int)Organizer_ID.ExecuteScalar());
                //Console.WriteLine(result);

                SqlCommand Meeting = new SqlCommand("IF NOT EXISTS (SELECT * FROM Meetings WHERE ID_Meeting = '" + ID_Meeting + "') BEGIN INSERT INTO Meetings (ID_Meeting, Subject, Location, Start_time, End_time, Organizer) VALUES (@ID_Meeting, @Subject, @Location, @Start_time, @End_time, @Organizer) END", conn);
                Meeting.Parameters.AddWithValue("@ID_Meeting", ID_Meeting);
                Meeting.Parameters.AddWithValue("@Subject", Subject);
                Meeting.Parameters.AddWithValue("@Location", Location);
                Meeting.Parameters.AddWithValue("@Start_time", StartTime);
                Meeting.Parameters.AddWithValue("@End_time", EndTime);
                Meeting.Parameters.AddWithValue("@Organizer", result);
                Meeting.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Output_exceptions(ex);
            }
            
        }

        /* string item - optional/required attendee
         * string ID_Meeting - meeting id (unique)
         * int Is_required - possible only two values 1 or 0 (1 - required, 0 - optional)
         */
        private static void Add_Meeting_Attendees(string item, SqlConnection conn, string ID_Meeting, int Is_required)
        {
            // Parsing email and username
            string reqAtten = item.Replace("<SMTP:", "").Replace(">", "");
            string[] words = reqAtten.Split(' ');
            try
            {
                SqlCommand User_Table = new SqlCommand("IF NOT EXISTS (SELECT * FROM Users WHERE Username = '" + words[0] + "' AND Email = '" + words[1] + "') BEGIN INSERT INTO Users (Email, Username) VALUES (@Email, @Username) END", conn);
                User_Table.Parameters.AddWithValue("@Email", words[1]); // words[1] contains email
                User_Table.Parameters.AddWithValue("@Username", words[0]); // words[0] contains username
                User_Table.ExecuteNonQuery();

                SqlCommand Meeting_Attenders_Table = new SqlCommand("IF NOT EXISTS (SELECT * FROM Meeting_attenders WHERE User_Username = '" + words[0] + "'" + "AND ID_Meting = '" + ID_Meeting + "') BEGIN INSERT INTO Meeting_attenders (ID_Meting, User_Username, Is_Required, Have_Evaluated, Is_Organizer) VALUES ('" + ID_Meeting + "', '" + words[0] + "', '" + Is_required + "', 0, 0) END", conn);
                Meeting_Attenders_Table.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Output_exceptions(ex);
            }
            
        }

        private static void Output_exceptions(Exception ex)
        {
            Console.WriteLine(ex);
            using (FileStream fs = new FileStream("C:/Errors Log/Error_Log.txt", FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
               sw.WriteLine("Time: " + DateTime.Now);
               sw.WriteLine("Error: " + ex.Message + "\n");
            }
            
        }

        // Method for validating X509Certificate (recommended by MS Exchange documentation)
        private static bool CertificateValidationCallBack(
        object sender,
        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        System.Security.Cryptography.X509Certificates.X509Chain chain,
        System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}