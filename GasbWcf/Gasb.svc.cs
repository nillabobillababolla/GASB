using System;
using System.Linq;
using Gasb.Contract;

namespace GasbWcf
{
    public class Gasb : IService
    {
        /// <summary>
        /// Register Servis Metodu.
        /// </summary>
        /// <param name="mail">Kullanıcının mail adresi.</param>
        /// <returns></returns>
        public Guid Register(string mail)
        {
            using (var dc = new PrimeDataContext())
            {
                var clientId = dc.Clients.Where(c => c.Mail == mail).Select(c => c.Id).FirstOrDefault();

                if (clientId == Guid.Empty)
                {
                    var item = new Client {Id = Guid.NewGuid(), Mail = mail};
                    
                    dc.Clients.InsertOnSubmit(item);

                    dc.SubmitChanges();

                    return item.Id;
                }

                return clientId;
            }
        }

        /// <summary>
        /// SaveResult Servis Metodu.
        /// </summary>
        /// <param name="clientId">Kullanıcı Id değeri.</param>
        /// <param name="n">Hesaplama yapılacak N sayısı.</param>
        /// <param name="isPrime">Sayının asal olup olmadığının kontrolü.</param>
        public void SaveResult(Guid clientId, int n, bool isPrime)
        {
            using (var dc = new PrimeDataContext())
            {
                var item = dc.Jobs.FirstOrDefault(c => c.Client_Id == clientId && c.N == n);

                if (item == null) return;

                item.Result = isPrime;
                item.Result_Date = DateTime.Now;

                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// GetNumber Servis Metodu.
        /// </summary>
        /// <param name="clientId">Kullanıcının Id değeri.</param>
        /// <returns></returns>
        public int GetNumber(Guid clientId)
        {
            using (var dc = new PrimeDataContext())
            {
                var n = dc.Jobs.Where(c =>
                    !c.Result.HasValue &&
                    c.Assign_Date < DateTime.Now.AddMinutes(-1) &&
                    dc.Jobs.Any(j => j.Result.HasValue || j.Assign_Date > DateTime.Now.AddMinutes(-1) && j.N == c.N)).Select(c => c.N).FirstOrDefault();

                if (n == 0)
                {
                    n = dc.Jobs.OrderByDescending(c => c.N).Select(c => c.N).FirstOrDefault();

                    if (n == 0)
                    {
                        n = 1009;
                    }
                    else
                    {
                        n = n + 2;
                    }
                }
                
                var item = new Job
                    { Id = Guid.NewGuid(),
                      N = n,
                      Client_Id = clientId,
                      Assign_Date = DateTime.Now
                    };
                dc.Jobs.InsertOnSubmit(item);
                dc.SubmitChanges();

                return n;

            }
        }
    }
}
