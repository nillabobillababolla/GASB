using System;
using System.Configuration;
using System.IO;
using System.Numerics;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Gasb.Contract;

namespace ClientApp
{
    public partial class Form1 : Form
    {
        private Guid ClientId;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = @"Gönüllü Asal Sayı Bulucu";
            notifyIcon1.BalloonTipText = @"Program arka planda çalışmaya devam edecek.";
            notifyIcon1.Text = @"GASB";

            try
            {
                var eMail = File.ReadAllText("user.txt");

                if (IsValidEmail(eMail))
                {
                    var channel = this.GetChannel();
                    var clientId = channel.Register(eMail);

                    this.ClientId = clientId;

                    this.panel1.Visible = false;
                    this.timer1.Enabled = true;
                }
            }
            catch { }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var eMail = txtMail.Text;

                if (!IsValidEmail(eMail))
                {
                    throw new Exception("Lütfen geçerli bir mail adresi giriniz.");
                }

                var channel = this.GetChannel();
                var clientId = channel.Register(eMail);

                this.ClientId = clientId;

                File.WriteAllText("user.txt", eMail);

                WindowState = FormWindowState.Minimized;

                this.panel1.Visible = false;
                this.timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
            else if (FormWindowState.Normal == WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var channel = this.GetChannel();
            var n = channel.GetNumber(this.ClientId);

            this.label2.Text = "Get number " + n;

            var isPrime = IsPrime(n);

            channel.SaveResult(this.ClientId, n, isPrime);

            this.label2.Text = "Save number " + n + " (isPrime = " + isPrime + ")";
        }

        private IService GetChannel()
        {
            var url = ConfigurationManager.AppSettings["ServiceAddress"];
            var binding = new BasicHttpBinding();
            var address = new EndpointAddress(url);
            var channel = ChannelFactory<IService>.CreateChannel(binding, address);

            return channel;
        }

        /// <summary>
        /// Lucas-Lehmer Testi sayının asal olup olmadığını kontrol için kullanılır.
        /// </summary>
        /// <param name="p">Asal kontrolü yapılacak sayı </param>
        /// <returns></returns>
        private static bool IsPrime(int p)
        {
            if (p < 2)
            {
                throw new ArgumentException(nameof(p));
            }

            // Eğer p değeri çift ise, sadece 2 değeri için Mersenne asalı olur.
            if (p % 2 == 0)
            {
                return p == 2;
            }

            // (2^p - 1) sayısının asal olması için, p değerinin de asal olması gerekiyor.
            // Tüm tek sayılara göre mod alma işlemi uygulanarak, p sayısının asal olup olmadığına bakılıyor.
            // Döngünün p^(1/2) ye kadar çalışması yeterli. Hiçbir sayı, karekök değerinden büyük bir sayıya tam olarak bölünemez.
            for (int i = 3; i <= (int)Math.Sqrt(p); i += 2)
            {
                if (p % i == 0)
                {
                    // Tam olarak bölünüyor, asal değil.
                    return false;
                }
            }

            //return true;

            //Değer çok büyük olabileceği için BigInteger kullanılmıştır.
            var mode = BigInteger.Pow(2, p) - 1;

            //S fonksiyonu başlangıç değeri
            var S = new BigInteger(4);

            //S(p - 2) değeri bulunacak
            for (int n = 1; n <= p - 2; n++)
            {
                //S(n) = (S(n - 1) ^ 2 - 2) mod(2p - 1)
                S = (BigInteger.Pow(S, 2) - 2) % mode;
            }

            //S(p - 2) değeri 0 ise, (2 ^ p - 1) değeri asaldır.
            return S == 0;
        }

        private bool IsValidEmail(string strMail)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strMail, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}
