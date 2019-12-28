using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Diagnostics;


namespace BlueToothServer
{
    public class EmailConsole
    {
        Thread emailConsole;
        SqlCloudConsole sqlConsole;
        List<EmailDetails> emailList;
        string lSource = "TT Server";
        string lLog = "TT Server Log";

        public EmailConsole(SqlCloudConsole s)
        {
            sqlConsole = s;
            emailList=new List<EmailDetails>();
            emailConsole = new Thread(new ThreadStart(run)); // i am in my own thread
            emailConsole.Start();

        }
        private void run()
        {
            EmailDetails eDetails;
            int i = 0;
            Thread.Sleep(2000);
            addEmailRequest(9, 11, "Now ");
            while (true)
            {
                Thread.Sleep(2000);
                if (emailList.Count > 0)
                {
                    eDetails = emailList[0];
                    emailList.RemoveAt(0);
                    sendEmail(eDetails.getLink(), eDetails.getLevel(), eDetails.getTime());

                }
            }
        }
        public void addEmailRequest(int link, int level, string time)
        {
            lock (this)
            {
                emailList.Add(new EmailDetails(link, level, time));
            }

        }

        private void sendEmail(int link,int level,string time)
        {
            TransmitDetails transmitDetails=getDetails(link, level);
            if (transmitDetails != null)
            {
                string[] emails = transmitDetails.getEmails();
                string desc = transmitDetails.getDesc();
                string linkDesc = transmitDetails.getLinkDesc();
                int noOfAddress = emails.Length;
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("ian.bennetts@softgineering", "Softgineering TT Monitor");
                for (int i = 0; i < noOfAddress; i++)
                {
                    mail.To.Add(emails[i]);
                }
                mail.IsBodyHtml = false;
                mail.Subject = desc+" Link no " + link;
                mail.Body = "Link - " + link + " " + linkDesc+" "+ desc+"  at " + time;
                mail.Priority = MailPriority.High;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("ian.bennetts@softgineering.com", "seahorse99");
                smtp.EnableSsl = true;
                try
                {

                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    if (!EventLog.SourceExists(lSource))
                        EventLog.CreateEventSource(lSource, lLog);
                    EventLog.WriteEntry(lSource, "In Email Transmit "+ "link = "+link+" level "+level+" email no "+noOfAddress+" "+" Address = "+emails[0] +ex.ToString());
                }
            }

        }
        private TransmitDetails getDetails(int link, int level)
        {
            SqlConnection con;
            SqlCommand command;
            SqlDataReader Reader;
            TransmitDetails tds=null;
            Console.WriteLine("Link = " + link + " Level " + level);
            try
            {
                con = new SqlConnection(Connection.GlobalConnectionStringCloud);
                con.Open();
                command = new SqlCommand("select * from AlertDetails where linkNo=@l and level=@t", con);
                command.Parameters.AddWithValue("@l", link);
                command.Parameters.AddWithValue("@t", level);
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    tds = new TransmitDetails(Reader.GetString(2), Reader.GetString(3), Reader.GetString(5));

                }
                Reader.Close();
                con.Close();
                return tds;
            }
            catch (Exception ex)
            {
                if (!EventLog.SourceExists(lSource))
                    EventLog.CreateEventSource(lSource, lLog);
                EventLog.WriteEntry(lSource, "In Email Sql " + ex.ToString());
   
                
            }

            return null;
        }
    }
    class EmailDetails
    {
        int eLink;
        int eLevel;
        string eTime;
        
        public EmailDetails(int link, int level, string time){
            eLink = link;
            eLevel = level;
            eTime = time;
        }
        public int getLink()
        {
            return eLink;
        }
        public int getLevel()
        {
            return eLevel;
        }
        public string getTime()
        {
            return eTime;
        }
    }
    class TransmitDetails
    {
        string [] emailList;
        string desc;
        string linkDesc;

        public TransmitDetails(string d,string email,string lDesc)
        {
           // t = emails;
            emailList = email.Split(',');
            desc = d;
            linkDesc = lDesc;
        }
        public string getDesc()
        {
            return desc;
        }
        public string[] getEmails()
        {
            return emailList;
        }
        public string getLinkDesc()
        {
            return linkDesc;
        }
    }
}
